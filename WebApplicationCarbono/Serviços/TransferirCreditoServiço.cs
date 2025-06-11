using Npgsql;
using WebApplicationCarbono.Interface;
using WebApplicationCarbono.Modelos;

namespace WebApplicationCarbono.Serviços
{
    public class TransferirCreditoServiço : ITransferirCredito
    {
        private readonly string _conexao;

        public TransferirCreditoServiço(IConfiguration config)
        {
            _conexao = config.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("Connection string 'DefaultConnection' não encontrada na configuração.");
        }

        public object VerificarDestinatario(string emailOuCnpj)
        {
            using var conexao = new NpgsqlConnection(_conexao);
            conexao.Open();

            var query = "SELECT id, nome, email, cnpj FROM usuarios WHERE email = @valor OR cnpj = @valor";
            using var comando = new NpgsqlCommand(query, conexao);
            comando.Parameters.AddWithValue("valor", emailOuCnpj);

            using var leitor = comando.ExecuteReader();
            if (leitor.Read())
            {
                return new
                {
                    Id = leitor.GetInt32(0),
                    Nome = leitor.GetString(1),
                    Email = leitor.GetString(2),
                    Cnpj = leitor.GetString(3)
                };
            }

            throw new Exception("Destinatário não encontrado.");
        }

        public string RealizarTransferencia(TransferenciaModelo transferencia)
        {

            using var conexao = new NpgsqlConnection(_conexao);
            conexao.Open();
            using var transacao = conexao.BeginTransaction();

            try
            {

                int DestinatarioId = ObterDestinatarioId(conexao, transferencia.DestinatarioEmailOuCnpj);

                if (transferencia.RemetenteId == DestinatarioId)
                    return "Não é possível transferir créditos para si mesmo.";

                if (transferencia.QuantidadeCredito <= 0)
                    return "A quantidade de créditos a ser transferida deve ser maior que zero.";

                if (!TemSaldoSuficiente(conexao, transferencia.RemetenteId, transferencia.QuantidadeCredito))
                    return "Saldo insuficiente para transferência.";

                RegistrarMovimentacao(conexao, transferencia.RemetenteId, "transferência_saida", -transferencia.QuantidadeCredito,
                    $"Transferência para usuário {DestinatarioId}: {transferencia.Descricao}", DestinatarioId);

                RegistrarMovimentacao(conexao, DestinatarioId, "transferência_entrada", transferencia.QuantidadeCredito,
                    $"Transferência recebida do usuário {transferencia.RemetenteId}: {transferencia.Descricao}", transferencia.RemetenteId);


                transacao.Commit();
                return "Transferência realizada com sucesso.";
            }
            catch (Exception ex)
            {
                transacao.Rollback();
                return "Erro na transferência: " + ex.Message;
            }
        }

        private bool TemSaldoSuficiente(NpgsqlConnection conexao, int usuarioId, decimal valor)
        {
            var comando = new NpgsqlCommand(
                "SELECT COALESCE(SUM(valor_creditos), 0) FROM saldo_usuario_dinamica WHERE id_usuario = @usuarioId", conexao);
            comando.Parameters.AddWithValue("usuarioId", usuarioId);
            var saldo = (decimal?)comando.ExecuteScalar();
            return saldo >= valor;
        }

        private void RegistrarMovimentacao(NpgsqlConnection conexao, int usuarioId, string tipo_transacao, decimal valor, string descricao, int? usuarioDestinoId = null)
        {
            var comando = new NpgsqlCommand(@"
            INSERT INTO saldo_usuario_dinamica 
            (id_usuario, tipo_transacao, valor_creditos, descricao, id_usuario_destino, status_transacao) 
            VALUES (@usuarioId, @tipo_transacao, @valor, @descricao, @usuarioDestinoId, @status_transacao)", conexao);

            comando.Parameters.AddWithValue("usuarioId", usuarioId);
            comando.Parameters.AddWithValue("tipo_transacao", tipo_transacao);
            comando.Parameters.AddWithValue("valor", valor);
            comando.Parameters.AddWithValue("descricao", descricao ?? "");
            comando.Parameters.AddWithValue("usuarioDestinoId", (object?)usuarioDestinoId ?? DBNull.Value);
            comando.Parameters.AddWithValue("status_transacao", "concluído");

            comando.ExecuteNonQuery();
        }


        private int ObterDestinatarioId(NpgsqlConnection conexao, string emailOUCnpj)
        {
            var query = "SELECT id FROM usuarios WHERE email = @valor OR cnpj = @valor";
            using var comando = new NpgsqlCommand(query, conexao);
            comando.Parameters.AddWithValue("valor", emailOUCnpj);

            var resultado = comando.ExecuteScalar();
            if (resultado != null)
            {
                return (int)resultado;
            }


            throw new Exception("Destinatário não encontrado.");
        }
    }
}
