using Npgsql;
using System.Data;
using WebApplicationCarbono.Interface;
using WebApplicationCarbono.Modelos;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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
                    // Buscar destinatário
                    var comandoBusca = new NpgsqlCommand("SELECT id FROM usuarios WHERE email = @valor OR cnpj = @valor", conexao);
                    comandoBusca.Parameters.AddWithValue("valor", transferencia.DestinatarioEmailOuCnpj);
                    var leitor = comandoBusca.ExecuteReader();

                    if (leitor.Read())
                    {
                        destinatarioId = leitor.GetInt32(0);
                        leitor.Close();
                    }
                    else
                    {
                        leitor.Close();
                        return "Destinatário inválido.";
                    }

                    if (destinatarioId == transferencia.RemetenteId)
                    {
                        return "Não é possível realizar transferência para você mesmo.";        
                    }



                    // Verifica saldo
                    var VerificarSaldo = new NpgsqlCommand("SELECT creditos_carbono FROM saldos WHERE id_usuario = @id", conexao);
                    VerificarSaldo.Parameters.AddWithValue("id", transferencia.RemetenteId);
                    var saldoAtual = (decimal?)VerificarSaldo.ExecuteScalar();

                    if (saldoAtual == null || saldoAtual < transferencia.QuantidadeCredito)
                    {
                        return "Saldo insuficiente para realizar a transferência.";
                    }



                    // Debitar saldo
                    var comandoSub = new NpgsqlCommand("UPDATE saldos SET creditos_carbono = creditos_carbono - @qtd WHERE id_usuario = @id", conexao);
                    comandoSub.Parameters.AddWithValue("qtd", transferencia.QuantidadeCredito);
                    comandoSub.Parameters.AddWithValue("id", transferencia.RemetenteId);
                    comandoSub.ExecuteNonQuery();

                    // Verificar se destinatário já tem saldo 
                    var verificaDest = new NpgsqlCommand("SELECT COUNT(*) FROM saldos WHERE id_usuario = @id", conexao);
                    verificaDest.Parameters.AddWithValue("id", destinatarioId);
                    var existe = (long)verificaDest.ExecuteScalar();

                    if (existe == 0)
                    {   // se n houver, insere com o valor recebido
                        var insereDest = new NpgsqlCommand("INSERT INTO saldos (id_usuario, saldo, creditos_carbono) VALUES (@id, 0, @qtd)", conexao);
                        insereDest.Parameters.AddWithValue("id", destinatarioId);
                        insereDest.Parameters.AddWithValue("qtd", transferencia.QuantidadeCredito);
                        insereDest.ExecuteNonQuery();
                    }
                    else
                    {   // se já houver, soma o atual com o recebido
                        var atualizaDest = new NpgsqlCommand("UPDATE saldos SET creditos_carbono = creditos_carbono + @qtd WHERE id_usuario = @id", conexao);
                        atualizaDest.Parameters.AddWithValue("qtd", transferencia.QuantidadeCredito);
                        atualizaDest.Parameters.AddWithValue("id", destinatarioId);
                        atualizaDest.ExecuteNonQuery();
                    }

                     // Inserir no histórico a trasnferência como concluído
                    var ConcluidoRemente = new NpgsqlCommand(@"
                    INSERT INTO transacoes
                    (data, descricao, tipo, quantidade, valor, id_usuario, id_destinatario, status)
                    VALUES (NOW(), @desc, 'transferencia', @qtd, @valor, @remetente, @destinatario, 'concluido')", conexao);

                    ConcluidoRemente.Parameters.AddWithValue("desc", "Transferência de créditos realizada com sucesso");
                    ConcluidoRemente.Parameters.AddWithValue("qtd", transferencia.QuantidadeCredito);
                    ConcluidoRemente.Parameters.AddWithValue("remetente", transferencia.RemetenteId);
                    ConcluidoRemente.Parameters.AddWithValue("destinatario", destinatarioId);
                    ConcluidoRemente.Parameters.AddWithValue("valor", 0);
                    ConcluidoRemente.ExecuteNonQuery();
                    
                    //Inserir no histórico a trasnferência recibida como concluído
                    var comandoConcluido = new NpgsqlCommand(@"
                    INSERT INTO transacoes
                    (data, descricao, tipo, quantidade, valor, id_usuario, id_destinatario, status)
                    VALUES (NOW(), @desc, 'transferencia', @qtd, @valor, @remetente, @destinatario, 'concluido')", conexao);

                    comandoConcluido.Parameters.AddWithValue("desc", "Transferência de créditos recebida com sucesso");
                    comandoConcluido.Parameters.AddWithValue("qtd", transferencia.QuantidadeCredito);
                    comandoConcluido.Parameters.AddWithValue("remetente", destinatarioId);
                    comandoConcluido.Parameters.AddWithValue("destinatario", transferencia.RemetenteId);
                    comandoConcluido.Parameters.AddWithValue("valor", 0); 
                    comandoConcluido.ExecuteNonQuery();

                    transacao.Commit();
                    return "Transferência realizada com sucesso.";


                }
                catch (Exception ex)
                {


                    transacao.Rollback();

                    if (conexao.State != ConnectionState.Open)
                    conexao.Open(); 

                    var comandoErro = new NpgsqlCommand(@"
                    INSERT INTO transacoes
                    (data, descricao, tipo, quantidade, valor, id_usuario, id_destinatario, status)
                    VALUES (NOW(), @desc, 'transferencia', @qtd, @valor, @remetente, @destinatario, 'falhou')", conexao);

                    comandoErro.Parameters.AddWithValue("desc", "Erro interno: " + ex.Message);
                    comandoErro.Parameters.AddWithValue("qtd", transferencia.QuantidadeCredito);
                    comandoErro.Parameters.AddWithValue("remetente", transferencia.RemetenteId);
                    comandoErro.Parameters.AddWithValue("destinatario", destinatarioId);
                    comandoErro.Parameters.AddWithValue("valor", 0);  
                    comandoErro.ExecuteNonQuery();

                    conexao.Close();
                    
                    return "Erro ao realizar a transferência: " + ex.Message;
                }
            }
    } 

}
