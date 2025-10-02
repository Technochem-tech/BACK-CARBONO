using MimeKit;
using MailKit.Net.Smtp;
using Npgsql;
using Microsoft.Extensions.Configuration;
using WebApplicationCarbono.Interface;
using WebApplicationCarbono.Dtos;

namespace WebApplicationCarbono.Serviços
{
    public class VerificacaoEmailServico : IVerificacaoEmail
    {
        private readonly string _stringConexao;
        private readonly IConfiguration _config;

        public VerificacaoEmailServico(IConfiguration config)
        {
            _config = config;
            _stringConexao = config.GetConnectionString("DefaultConnection")!;
        }

        public void EnviarCodigoVerificacao(string email)
        {
            string codigo = new Random().Next(100000, 999999).ToString();

            using var conexao = new NpgsqlConnection(_stringConexao);
            conexao.Open();

            // verifca se já existe um email cadastrado
            var comandoVerificacao = new NpgsqlCommand("SELECT COUNT(*) FROM usuarios WHERE email = @Email", conexao);
            comandoVerificacao.Parameters.AddWithValue("@Email", email);
            int count = Convert.ToInt32(comandoVerificacao.ExecuteScalar());

            if (count > 0)
            {
                throw new ArgumentException("Já existe um usuário cadastrado com este email.");
            }

            var comando = new NpgsqlCommand(@"
                INSERT INTO verificacao_email (email, codigo, validade, confirmado)
                VALUES (@Email, @Codigo, @Validade, false)
                ON CONFLICT (email)
                DO UPDATE SET codigo = @Codigo, validade = @Validade, confirmado = false
            ", conexao);

            comando.Parameters.AddWithValue("@Email", email);
            comando.Parameters.AddWithValue("@Codigo", codigo);
            comando.Parameters.AddWithValue("@Validade", DateTime.UtcNow.AddMinutes(10));
            comando.ExecuteNonQuery();

            var mensagem = new MimeMessage();
            mensagem.From.Add(new MailboxAddress("Sistema", _config["EmailSettings:From"]));
            mensagem.To.Add(new MailboxAddress("", email));
            mensagem.Subject = "Código de verificação";
            mensagem.Body = new TextPart("plain")
            {
                Text = $"Seu código de verificação é: {codigo}"
            };

            //using var client = new SmtpClient();
            //client.Connect(_config["EmailSettings:SmtpServer"], int.Parse(_config["EmailSettings:SmtpPort"]), true);
            //client.Authenticate(_config["EmailSettings:Username"], _config["EmailSettings:Password"]);
            // client.Send(mensagem);
            // client.Disconnect(true);
            using var client = new SmtpClient();

            // Conecta via TLS na porta 587
            client.Connect(_config["EmailSettings:SmtpServer"], 587, MailKit.Security.SecureSocketOptions.StartTls);

            // Autentica
            client.Authenticate(_config["EmailSettings:Username"], _config["EmailSettings:Password"]);

            // Envia a mensagem
            client.Send(mensagem);

            // Desconecta
            client.Disconnect(true);
        }

        public bool ConfirmarCodigo(string email, string codigo)
        {
            email = email.Trim();
            codigo = codigo.Trim();

            using var conexao = new NpgsqlConnection(_stringConexao);
            conexao.Open();

            var comando = new NpgsqlCommand("SELECT validade FROM verificacao_email WHERE email = @Email AND codigo = @Codigo", conexao);
            comando.Parameters.AddWithValue("@Email", email);
            comando.Parameters.AddWithValue("@Codigo", codigo);

            var validadeObj = comando.ExecuteScalar();

            if (validadeObj == null)
                return false;

            DateTimeOffset validadeOffset;

            if (validadeObj is DateTimeOffset dto)
                validadeOffset = dto;
            else if (DateTimeOffset.TryParse(validadeObj.ToString(), out var dtoParsed))
                validadeOffset = dtoParsed;
            else
                return false;

            if (validadeOffset.UtcDateTime < DateTime.UtcNow)
                return false;

            var comandoUpdate = new NpgsqlCommand("UPDATE verificacao_email SET confirmado = true WHERE email = @Email", conexao);
            comandoUpdate.Parameters.AddWithValue("@Email", email);
            comandoUpdate.ExecuteNonQuery();

            return true;
        }

        public bool EstaConfirmado(string email)
        {
            using var conexao = new NpgsqlConnection(_stringConexao);
            conexao.Open();

            var comando = new NpgsqlCommand("SELECT confirmado FROM verificacao_email WHERE email = @Email", conexao);
            comando.Parameters.AddWithValue("@Email", email);
            var resultado = comando.ExecuteScalar();

            return resultado != null && Convert.ToBoolean(resultado);
        }
    }
}
