using MimeKit;
using Npgsql;
using System.Security.Cryptography;
using WebApplicationCarbono.Dtos;
using WebApplicationCarbono.Interface;
using MailKit.Net.Smtp;

namespace WebApplicationCarbono.Serviços
{
    public class RedefinicaoSenhaServiço : IRedefinicaoSenha
    {
        private readonly string _stringConexao;
        private readonly IConfiguration _configuracao;

        public RedefinicaoSenhaServiço(IConfiguration configuracao)
        {
            _configuracao = configuracao;
            _stringConexao = configuracao.GetConnectionString("DefaultConnection");
        }

        public void EnviarEmailRedefinicao(EditarSenhaRequestDto dto)
        {
            string token = GerarToken();

            using (var conexao = new NpgsqlConnection(_stringConexao))
            {
                conexao.Open();
                var comando = new NpgsqlCommand("INSERT INTO redefinicao_senha (email, token, validade) VALUES (@Email, @Token, @Validade)", conexao);
                comando.Parameters.AddWithValue("@Email", dto.Email);
                comando.Parameters.AddWithValue("@Token", token);
                comando.Parameters.AddWithValue("@Validade", DateTime.Now.AddHours(1));
                comando.ExecuteNonQuery();
            }

            var mensagem = new MimeMessage();
            mensagem.From.Add(new MailboxAddress("Suporte", _configuracao["EmailSettings:From"]));
            mensagem.To.Add(new MailboxAddress("", dto.Email));
            mensagem.Subject = "Redefinição de senha";

            string url = $"{_configuracao["FrontendUrl"]}/redefinir-senha?token={token}";

            mensagem.Body = new TextPart("html")
            {
                Text = $"<p>Você solicitou redefinição de senha. Clique no link abaixo para alterar sua senha:</p>" +
            $"<p><a href=\"{url}\">{url}</a></p>" +
            $"<p>Se não foi você, ignore este email.</p>"
            };


            using var client = new SmtpClient();
            client.Connect(_configuracao["EmailSettings:SmtpServer"], int.Parse(_configuracao["EmailSettings:SmtpPort"]), true);
            client.Authenticate(_configuracao["EmailSettings:Username"], _configuracao["EmailSettings:Password"]);
            client.Send(mensagem);
            client.Disconnect(true);
        }

        public bool ValidarToken(string token)
        {
            using var conexao = new NpgsqlConnection(_stringConexao);
            conexao.Open();
            var comando = new NpgsqlCommand("SELECT validade FROM redefinicao_senha WHERE token = @Token", conexao);
            comando.Parameters.AddWithValue("@Token", token);
            var validadeObj = comando.ExecuteScalar();

            if (validadeObj == null) return false;

            DateTime validade;
            try
            {
                validade = Convert.ToDateTime(validadeObj);
            }
            catch
            {
                return false;
            }

            if (validade < DateTime.Now) return false;


            return true;
        }

        public void AtualizarSenha(string token, string senha)
        {
            using var conexao = new NpgsqlConnection(_stringConexao);
            conexao.Open();

            var comandoBusca = new NpgsqlCommand("SELECT email FROM redefinicao_senha WHERE token = @Token AND validade > @Agora", conexao);
            comandoBusca.Parameters.AddWithValue("@Token", token);
            comandoBusca.Parameters.AddWithValue("@Agora", DateTime.UtcNow);

            var emailObj = comandoBusca.ExecuteScalar();
            if (emailObj == null)
                throw new Exception("Token inválido ou expirado.");

            string email = emailObj.ToString();

            string senhaHash = BCrypt.Net.BCrypt.HashPassword(senha);
            var comandoUpdate = new NpgsqlCommand("UPDATE usuarios SET senha = @Senha WHERE email = @Email", conexao);
            comandoUpdate.Parameters.AddWithValue("@Senha", senhaHash);
            comandoUpdate.Parameters.AddWithValue("@Email", email);
            comandoUpdate.ExecuteNonQuery();

            var comandoDelete = new NpgsqlCommand("DELETE FROM redefinicao_senha WHERE token = @Token", conexao);
            comandoDelete.Parameters.AddWithValue("@Token", token);
            comandoDelete.ExecuteNonQuery();
        }

        private string GerarToken()
        {
            var bytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes).Replace("+", "").Replace("/", "").Replace("=", "");
        }
    }
}
