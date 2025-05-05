using Npgsql;
using WebApplicationCarbono.Interface;
using WebApplicationCarbono.Modelos;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace WebApplicationCarbono.Serviços
{
    public class SaldoServiços : ISaldo
    {
        // passando a conexão do banco
        private readonly String _stringConexao;
        public SaldoServiços(IConfiguration configuaração)
        {
            _stringConexao = configuaração.GetConnectionString("DefaultConnection");
        }

        

        public List<object> ConsultarHistorico()
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
                        LEFT JOIN usuarios d ON d.id = t.id_destinatario";

                    using (var comando = new NpgsqlCommand(query, conexao))
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
            catch (Exception ex)
            {
                throw new Exception("Erro ao consultar o histórico de transações: " + ex.Message);
            }

            return lista;
        }

        public decimal GetCreditos(int IdUsuario)
        {
            decimal creditosDeCarbonoEmConta = 0.0m;

            try
            {

                using (var conexao = new NpgsqlConnection(_stringConexao))
                {

                    conexao.Open();

                    var consulta = "SELECT * FROM saldos WHERE id_usuario = @IdUsuario";
                    using (var comando = new NpgsqlCommand(consulta, conexao))
                    {
                        comando.Parameters.AddWithValue("IdUsuario", IdUsuario);

                        using (var consultarCreditos = comando.ExecuteReader())
                        {
                            if (consultarCreditos.Read())
                            {
                                creditosDeCarbonoEmConta = consultarCreditos.GetDecimal(consultarCreditos.GetOrdinal("creditos_carbono"));
                            }
                        }

                    }
                }

            }
            catch (Exception ex)
            {

                throw new Exception("Erro ao buscar os cretidos de carbono : " + ex.Message);
            }

            return creditosDeCarbonoEmConta;
        }




        public decimal GetSaldo(int IdUsuario)
        {
            decimal saldoEmConta = 0.00m;

            try
            {
                using (var conexao = new NpgsqlConnection(_stringConexao))
                {
                    conexao.Open();

                    var consulta = "SELECT * FROM saldos WHERE id_usuario = @IdUsuario";
                    using (var comando = new NpgsqlCommand(consulta, conexao))
                    {
                        comando.Parameters.AddWithValue("IdUsuario", IdUsuario);

                        using (var consultaSaldo = comando.ExecuteReader())
                        {
                            if (consultaSaldo.Read())
                            {
                                saldoEmConta = consultaSaldo.GetDecimal(consultaSaldo.GetOrdinal("saldo"));
                            }
                        }
                    }
                }


            }
            catch (Exception ex)
            {

                throw new Exception("Erro ao buscar o saldo: " + ex.Message);
            }
            return saldoEmConta;

        }

        public Usuario GetUsuario(int IdUsuario)
        {
            Usuario usuario = null;

            try
            {
                using (var conexao = new NpgsqlConnection(_stringConexao))
                {
                    conexao.Open();

                    var query = "SELECT * FROM usuarios WHERE id = @Id";
                    using (var comando = new NpgsqlCommand(query, conexao))
                    {
                        comando.Parameters.AddWithValue("Id", IdUsuario);

                        using (var reader = comando.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                usuario = new Usuario
                                {
                                    codigoCadastro = reader.GetInt32(reader.GetOrdinal("id")),
                                    Nome = reader.GetString(reader.GetOrdinal("nome")),
                                    Email = reader.GetString(reader.GetOrdinal("email")),
                                    empresa = reader.GetString(reader.GetOrdinal("empresa")),
                                    CNPJ = reader.GetString(reader.GetOrdinal("cnpj")),
                                    Telefone = reader.GetString(reader.GetOrdinal("telefone"))
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao consultar usuário: " + ex.Message);
            }

            return usuario;
        }
        public List<object> ListarProjetos()
        {
            var listaProjetos = new List<object>();

            try
            {
                using (var conexao = new NpgsqlConnection(_stringConexao))
                {
                    conexao.Open();

                    var consulta = "SELECT titulo, valor, descricao FROM projetos";
                    using (var comando = new NpgsqlCommand(consulta, conexao))
                    {
                        using (var reader = comando.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                listaProjetos.Add(new
                                {
                                    titulo = reader["titulo"].ToString(),
                                    valor = Convert.ToDecimal(reader["valor"]),
                                    descricao = reader["descricao"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao listar os projetos sustentáveis: " + ex.Message);
            }

            return listaProjetos;
        }

        
    }
}
