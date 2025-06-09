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

            try
            {
                destinatarioId = ObterDestinatarioId(conexao, transferencia.DestinatarioEmailOuCnpj);
                if (destinatarioId == 0)
                {
                    return "Destinatário não encontrado.";
                }


                if (destinatarioId == transferencia.RemetenteId)
                    return "Não é possível realizar transferência para você mesmo.";

                if (!VerficarCredito(conexao, transferencia.RemetenteId, transferencia.QuantidadeCredito))
                    return "Saldo insuficiente para realizar a transferência.";

                DebitarCredito(conexao, transferencia.RemetenteId, transferencia.QuantidadeCredito);
                CreditarCredito(conexao, destinatarioId, transferencia.QuantidadeCredito);

                RegistrarTransacao(conexao, transferencia.RemetenteId, destinatarioId, transferencia.QuantidadeCredito, "Transferência de créditos realizada com sucesso");
                RegistrarTransacao(conexao, destinatarioId, transferencia.RemetenteId, transferencia.QuantidadeCredito, "Transferência de créditos recebida com sucesso");

                transacao.Commit();
                return "Transferência realizada com sucesso.";
            }
            catch (Exception ex)
            {
                transacao.Rollback();
                RegistrarTransacao(conexao, transferencia.RemetenteId, destinatarioId, transferencia.QuantidadeCredito, "Erro ao realizar transferência: " + ex.Message, "erro");
                return "Erro ao realizar a transferência: " + ex.Message;
            }
        }

        public string RealizarTransferenciaSaldo(TransferenciaModeloSaldo transferenciaSaldo)
        {
            if (transferenciaSaldo.QuantidadeSaldo <= 0)
            {
                return "O valor a transferir deve ser maior que zero.";
            }

            using var conexao = new NpgsqlConnection(_conexao);
            conexao.Open();
            using var transacao = conexao.BeginTransaction();

            int destinatarioId = 0;
            

            try
            {
                destinatarioId = ObterDestinatarioId(conexao, transferenciaSaldo.DestinatarioEmailOuCnpj);
                if (destinatarioId == 0)
                {
                    return "Destinatário não encontrado.";
                }



                if (destinatarioId == transferenciaSaldo.RemetenteId)
                    return "Não é possível realizar transferência para você mesmo.";

                if (!VerficarSaldo(conexao, transferenciaSaldo.RemetenteId, transferenciaSaldo.QuantidadeSaldo))
                    return "Saldo insuficiente para realizar a transferência.";

                DebitarSaldo(conexao, transferenciaSaldo.RemetenteId, transferenciaSaldo.QuantidadeSaldo);
                CreditarSaldo(conexao, destinatarioId, transferenciaSaldo.QuantidadeSaldo);

                RegistrarTransacao(conexao, transferenciaSaldo.RemetenteId, destinatarioId, transferenciaSaldo.QuantidadeSaldo, "Transferência de saldo realizada com sucesso");
                RegistrarTransacao(conexao, destinatarioId, transferenciaSaldo.RemetenteId, transferenciaSaldo.QuantidadeSaldo, "Transferência de saldo recebida com sucesso");

                transacao.Commit();
                return "Transferência realizada com sucesso.";
            }
            catch (Exception ex)
            {
                transacao.Rollback();
                RegistrarTransacao(conexao, transferenciaSaldo.RemetenteId, destinatarioId, transferenciaSaldo.QuantidadeSaldo, "Erro ao realizar transferência: " + ex.Message, "erro");
                return "Erro ao realizar a transferência: " + ex.Message;
            }
        }

        // Métodos auxiliares

        private int ObterDestinatarioId(NpgsqlConnection conexao, string emailOUCnpj)
        {
            var ComandoBusca = new NpgsqlCommand("SELECT id FROM usuarios WHERE email = @valor OR cnpj = @valor", conexao);
            ComandoBusca.Parameters.AddWithValue("valor", emailOUCnpj);
            var leitor = ComandoBusca.ExecuteReader();
            if (leitor.Read())
            {
                int id = leitor.GetInt32(0);
                leitor.Close();
                return id;
            }
            else
            {

                int id = 0; 
                return id = 0;
            }

        }


        private bool VerficarCredito(NpgsqlConnection conexao, int remetenteId, decimal quantidadeCredito)
        {
            var comando = new NpgsqlCommand("SELECT creditos_carbono FROM saldos WHERE id_usuario = @id", conexao);
            comando.Parameters.AddWithValue("id", remetenteId);
            var saldo = (decimal?)comando.ExecuteScalar();
            return saldo != null && saldo >= quantidadeCredito;
        }

        private void DebitarCredito(NpgsqlConnection conexao, int remetenteId, decimal quantidade)
        {
            var comando = new NpgsqlCommand("UPDATE saldos SET creditos_carbono = creditos_carbono - @qtd WHERE id_usuario = @id", conexao);
            comando.Parameters.AddWithValue("qtd", quantidade);
            comando.Parameters.AddWithValue("id", remetenteId);
            comando.ExecuteNonQuery();
        }

        private void CreditarCredito(NpgsqlConnection conexao, int destinatarioId, decimal quantidade)
        {
            var verifica = new NpgsqlCommand("SELECT COUNT(*) FROM saldos WHERE id_usuario = @id", conexao);
            verifica.Parameters.AddWithValue("id", destinatarioId);
            var existe = (long)verifica.ExecuteScalar();

            if (existe == 0)
            {
                var inserir = new NpgsqlCommand("INSERT INTO saldos (id_usuario, saldo, creditos_carbono) VALUES (@id, 0, @qtd)", conexao);
                inserir.Parameters.AddWithValue("id", destinatarioId);
                inserir.Parameters.AddWithValue("qtd", quantidade);
                inserir.ExecuteNonQuery();
            }
            else
            {
                var atualizar = new NpgsqlCommand("UPDATE saldos SET creditos_carbono = creditos_carbono + @qtd WHERE id_usuario = @id", conexao);
                atualizar.Parameters.AddWithValue("qtd", quantidade);
                atualizar.Parameters.AddWithValue("id", destinatarioId);
                atualizar.ExecuteNonQuery();
            }
        }

        private bool VerficarSaldo(NpgsqlConnection conexao, int remetenteId, decimal quantidadeSaldo)
        {
            var comando = new NpgsqlCommand("SELECT saldo FROM saldos WHERE id_usuario = @id", conexao);
            comando.Parameters.AddWithValue("id", remetenteId);
            var saldo = (decimal?)comando.ExecuteScalar();
            return saldo != null && saldo >= quantidadeSaldo;
        }

        private void DebitarSaldo(NpgsqlConnection conexao, int remetenteId, decimal quantidade)
        {
            var comando = new NpgsqlCommand("UPDATE saldos SET saldo = saldo - @qtd WHERE id_usuario = @id", conexao);
            comando.Parameters.AddWithValue("qtd", quantidade);
            comando.Parameters.AddWithValue("id", remetenteId);
            comando.ExecuteNonQuery();
        }

        private void CreditarSaldo(NpgsqlConnection conexao, int destinatarioId, decimal quantidade)
        {
            var verifica = new NpgsqlCommand("SELECT COUNT(*) FROM saldos WHERE id_usuario = @id", conexao);
            verifica.Parameters.AddWithValue("id", destinatarioId);
            var existe = (long)verifica.ExecuteScalar();

            if (existe == 0)
            {
                var inserir = new NpgsqlCommand("INSERT INTO saldos (id_usuario, saldo, creditos_carbono) VALUES (@id, @qtd, 0)", conexao);
                inserir.Parameters.AddWithValue("id", destinatarioId);
                inserir.Parameters.AddWithValue("qtd", quantidade);
                inserir.ExecuteNonQuery();
            }
            else
            {
                var atualizar = new NpgsqlCommand("UPDATE saldos SET saldo = saldo + @qtd WHERE id_usuario = @id", conexao);
                atualizar.Parameters.AddWithValue("qtd", quantidade);
                atualizar.Parameters.AddWithValue("id", destinatarioId);
                atualizar.ExecuteNonQuery();
            }
        }

        private void RegistrarTransacao(NpgsqlConnection conexao, int remetenteId, int destinatarioId, decimal quantidade, string descricao, string status = "concluido")
        {
            var comando = new NpgsqlCommand(@"
                INSERT INTO transacoes
                (data, descricao, tipo, quantidade, valor, id_usuario, id_destinatario, status)
                VALUES (NOW(), @desc, 'transferencia', 0, @qtd, @remetente, @destinatario, @status)", conexao);

            comando.Parameters.AddWithValue("desc", descricao);
            comando.Parameters.AddWithValue("qtd", quantidade);
            comando.Parameters.AddWithValue("remetente", remetenteId);
            comando.Parameters.AddWithValue("destinatario", destinatarioId);
            comando.Parameters.AddWithValue("status", status);
            comando.ExecuteNonQuery();
        }
    }
}
