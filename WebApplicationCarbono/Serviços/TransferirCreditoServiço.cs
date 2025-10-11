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
        // Método para verificar se o destinatário existe com base no email ou CNPJ
        public object VerificarDestinatario(string emailOuCnpj)
        {
            using var conexao = new NpgsqlConnection(_conexao);
            conexao.Open();
            string emailLower = emailOuCnpj.Trim().ToLower();
            var query = "SELECT id, nome, email, cnpj FROM usuarios WHERE email = @valor OR cnpj = @valor";
            using var comando = new NpgsqlCommand(query, conexao);
            comando.Parameters.AddWithValue("valor", emailLower);

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
        // Método para realizar a transferência de créditos entre usuários
        public string RealizarTransferencia(TransferenciaModelo transferencia)
        {

            using var conexao = new NpgsqlConnection(_conexao);
            conexao.Open();
            using var transacao = conexao.BeginTransaction();

            try
            {

                int DestinatarioId = ObterDestinatarioId(conexao, transferencia.DestinatarioEmailOuCnpj);

                if (transferencia.RemetenteId == DestinatarioId)
                    throw new Exception("Não é possível transferir créditos para si mesmo.");

                if (transferencia.QuantidadeCredito <= 0)
                    throw new Exception("A quantidade de créditos a ser transferida deve ser maior que zero.");

                if (!TemSaldoSuficiente(conexao, transferencia.RemetenteId, transferencia.QuantidadeCredito))
                   throw new Exception("Saldo insuficiente para transferência.");

                var nomeRemetente = ObterNomeUsuario(conexao, transferencia.RemetenteId);
                var nomeDestinatario = ObterNomeUsuario(conexao, DestinatarioId);

                RegistrarMovimentacao(conexao, transferencia.RemetenteId, "transferência_saida", -transferencia.QuantidadeCredito,
                    $"Transferência para {nomeDestinatario}: {transferencia.Descricao}", DestinatarioId);

                RegistrarMovimentacao(conexao, DestinatarioId, "transferência_entrada", transferencia.QuantidadeCredito,
                    $"Transferência recebida de {nomeRemetente}: {transferencia.Descricao}", transferencia.RemetenteId);


                transacao.Commit();
                return "Transferência realizada com sucesso.";
            }
            catch (Exception ex)
            {
                transacao.Rollback();
                return "Erro na transferência: " + ex.Message;
            }
        }
        // Método para verificar se o usuário tem saldo suficiente para a transferência
        private bool TemSaldoSuficiente(NpgsqlConnection conexao, int usuarioId, decimal valor)
        {
            var comando = new NpgsqlCommand(
                "SELECT COALESCE(SUM(valor_creditos), 0) FROM saldo_usuario_dinamica WHERE id_usuario = @usuarioId", conexao);
            comando.Parameters.AddWithValue("usuarioId", usuarioId);
            var saldo = (decimal?)comando.ExecuteScalar();
            return saldo >= valor;
        }

        // Método para registrar a movimentação de créditos na tabela saldo_usuario_dinamica
        private void RegistrarMovimentacao(NpgsqlConnection conexao, int usuarioId, string tipo_transacao, decimal valor, string descricao, int? usuarioDestinoId = null)
        {
            var comando = new NpgsqlCommand(@"
            INSERT INTO saldo_usuario_dinamica 
            (id_usuario, tipo_transacao, valor_creditos, creditos_reservados, descricao, id_usuario_destino, status_transacao, id_projetos) 
            VALUES (@usuarioId, @tipo_transacao, @valor, @reservados, @descricao, @usuarioDestinoId, @status_transacao, @idProjeto)", conexao);

            comando.Parameters.AddWithValue("usuarioId", usuarioId);
            comando.Parameters.AddWithValue("tipo_transacao", tipo_transacao);
            comando.Parameters.AddWithValue("valor", valor);
            comando.Parameters.AddWithValue("reservados", 0); // ← creditos_reservados padrão
            comando.Parameters.AddWithValue("descricao", descricao ?? "");
            comando.Parameters.AddWithValue("usuarioDestinoId", (object?)usuarioDestinoId ?? DBNull.Value);
            comando.Parameters.AddWithValue("status_transacao", "concluído");
            comando.Parameters.AddWithValue("idProjeto", DBNull.Value); // ou 0 se preferir

            comando.ExecuteNonQuery();
        }

        // // Método para obter o nome do usuário com base no ID
        private string ObterNomeUsuario(NpgsqlConnection conexao, int usuarioId)
        {
            var comando = new NpgsqlCommand("SELECT nome FROM usuarios WHERE id = @id", conexao);
            comando.Parameters.AddWithValue("id", usuarioId);

            var resultado = comando.ExecuteScalar();
            return resultado?.ToString() ?? "Desconhecido";
        }

        // Método para obter o ID do destinatário com base no email ou CNPJ
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
