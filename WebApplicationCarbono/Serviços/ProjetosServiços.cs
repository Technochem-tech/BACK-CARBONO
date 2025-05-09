using Npgsql;
using WebApplicationCarbono.Interface;

namespace WebApplicationCarbono.Serviços
{
    public class ProjetosServiços : IProjetos
    {

        // passando a conexão do banco
        private readonly String _stringConexao;
        public ProjetosServiços(IConfiguration configuaração)
        {
            _stringConexao = configuaração.GetConnectionString("DefaultConnection");
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
