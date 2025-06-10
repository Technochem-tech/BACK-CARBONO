using Npgsql;
using WebApplicationCarbono.Interface;
using WebApplicationCarbono.Modelos;

namespace WebApplicationCarbono.Serviços
{
    public class TransferenciaServiço : ITransferencia
    {
        private readonly string _conexao;

        public TransferenciaServiço(IConfiguration config)
        {
            _conexao = config.GetConnectionString("DefaultConnection");
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
            if (transferencia.QuantidadeCredito <= 0)
            {
                return "A quantidade de créditos a transferir deve ser maior que zero.";
            }

            using var conexao = new NpgsqlConnection(_conexao);
            conexao.Open();
            using var transacao = conexao.BeginTransaction();

            int destinatarioId = 0;
            return "ok";



        }

        // Método principal para realizar transferência de saldo
        public string RealizarTransferenciaSaldo(TransferenciaModeloSaldo transferencia)
        {
            using var conexao = new NpgsqlConnection(_conexao);
            conexao.Open();
            using var transacao = conexao.BeginTransaction();

            try
            {
                int DestinatarioId = ObterDestinatarioId(conexao, transferencia.DestinatarioEmailOuCnpj);

                // Verifica saldo do remetente
                if (!TemSaldoSuficiente(conexao, transferencia.RemetenteId, transferencia.QuantidadeSaldo))
                    return "Saldo insuficiente para transferência.";

                // Debita do remetente (registro de saída)
                RegistrarMovimentacao(conexao, transferencia.RemetenteId, "saida", "transferencia", -transferencia.QuantidadeSaldo,
                    $"Transferência para usuário {DestinatarioId}: {transferencia.Descricao}");

                // Credita para o destinatário (registro de entrada)
                RegistrarMovimentacao(conexao, DestinatarioId, "entrada", "transferencia", transferencia.QuantidadeSaldo,
                    $"Transferência recebida do usuário {transferencia.RemetenteId}: {transferencia.Descricao}");

                transacao.Commit();
                return "Transferência realizada com sucesso.";
            }
            catch (Exception ex)
            {
                transacao.Rollback();
                return "Erro na transferência: " + ex.Message;
            }
        }

        // Verifica se usuário tem saldo suficiente
        private bool TemSaldoSuficiente(NpgsqlConnection conexao, int usuarioId, decimal valor)
        {
            var comando = new NpgsqlCommand(
                "SELECT COALESCE(SUM(valor), 0) FROM saldo_usuarios_dinamica WHERE usuario_id = @usuarioId", conexao);
            comando.Parameters.AddWithValue("usuarioId", usuarioId);
            var saldo = (decimal)comando.ExecuteScalar();
            return saldo >= valor;
        }

        // Registra uma movimentação na tabela
        private void RegistrarMovimentacao(NpgsqlConnection conexao, int usuarioId, string tipo, string categoria, decimal valor, string descricao)
        {
            var comando = new NpgsqlCommand(@"
            INSERT INTO saldo_usuarios_dinamica (usuario_id, tipo, categoria, valor, descricao) 
            VALUES (@usuarioId, @tipo, @categoria, @valor, @descricao)", conexao);
            comando.Parameters.AddWithValue("usuarioId", usuarioId);
            comando.Parameters.AddWithValue("tipo", tipo);
            comando.Parameters.AddWithValue("categoria", categoria);
            comando.Parameters.AddWithValue("valor", valor);
            comando.Parameters.AddWithValue("descricao", descricao ?? "");
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
