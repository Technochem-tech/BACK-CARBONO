using Npgsql;
using WebApplicationCarbono.Interface;

namespace WebApplicationCarbono.Serviços
{
    public class TransacaoServiço : ITransaçao
    {
        // passando a conexão do banco
        private readonly String _stringConexao;
        public TransacaoServiço(IConfiguration configuaração)
        {
            _stringConexao = configuaração.GetConnectionString("DefaultConnection");
        }


        public List<object> ConsultarHistorico(int idUsuario)
        {
            var lista = new List<object>();

            try
            {
                using (var conexao = new NpgsqlConnection(_stringConexao))
                {
                    conexao.Open();

                    string query = @"
                        SELECT t.data, t.descricao, t.tipo, t.quantidade, t.valor, 
                               u.nome AS usuario, d.nome AS destinatario, t.status
                        FROM transacoes t
                        LEFT JOIN usuarios u ON u.id = t.id_usuario
                        LEFT JOIN usuarios d ON d.id = t.id_destinatario
                        WHERE t.id_usuario = @idUsuario OR t.id_destinatario = @idUsuario";

                    using (var comando = new NpgsqlCommand(query, conexao))
                    {
                        comando.Parameters.AddWithValue("@idUsuario", idUsuario);

                        using (var leitor = comando.ExecuteReader())
                        {
                            while (leitor.Read())
                            {
                                lista.Add(new
                                {
                                    data = leitor.GetDateTime(leitor.GetOrdinal("data")).ToString("dd/MM/yyyy"),
                                    descricao = leitor["descricao"].ToString(),
                                    tipo = leitor["tipo"].ToString(),
                                    quantidade = Convert.ToDecimal(leitor["quantidade"]),
                                    valor = Convert.ToDecimal(leitor["valor"]),
                                    usuario = leitor["usuario"].ToString(),
                                    destinatario = leitor["destinatario"].ToString(),
                                    status = leitor["status"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao consultar o histórico de transações: " + ex.Message);
            }

            return lista;
        }
    }
}
