using Npgsql;
using WebApplicationCarbono.Dtos;
using WebApplicationCarbono.Interface;

namespace WebApplicationCarbono.Serviços
{
    public class ProjetosServiço : IProjetos
    {

        // passando a conexão do banco
        private readonly String _stringConexao;
        public ProjetosServiço(IConfiguration configuaração)
        {
            _stringConexao = configuaração.GetConnectionString("DefaultConnection");
        }

        public void CadastrarProjetos(CadastroProjetosDto dto)
        {
            using var conexao = new NpgsqlConnection(_stringConexao);
            conexao.Open();

            var comando = new NpgsqlCommand(
                "INSERT INTO projetos (titulo, descricao, valor) VALUES (@Titulo, @Descricao, @Valor)", conexao);

            comando.Parameters.AddWithValue("@Titulo", dto.titulo);
            comando.Parameters.AddWithValue("@Descricao", dto.descriçao);
            comando.Parameters.AddWithValue("@Valor", dto.valor);

            comando.ExecuteNonQuery();
        }

        public void DeletarProjeto(int id)
        {
            using var conexao = new NpgsqlConnection(_stringConexao);
            conexao.Open();

            var comando = new NpgsqlCommand("DELETE FROM projetos WHERE id = @Id", conexao);
            comando.Parameters.AddWithValue("@Id", id);

            int linhasAfetadas = comando.ExecuteNonQuery();
            if (linhasAfetadas == 0)
            {
                throw new Exception("Projeto não encontrado para deletar.");
            }


        }

        public void EditarProjeto(int id, EditarProjetoDto dto)
        {
            using var conexao = new NpgsqlConnection(_stringConexao);
            conexao.Open();

            var comando = new NpgsqlCommand("UPDATE projetos SET titulo = @Titulo, descricao = @Descricao, valor = @Valor WHERE id = @Id", conexao);
            comando.Parameters.AddWithValue("@Titulo", dto.titulo);
            comando.Parameters.AddWithValue("@Descricao", dto.descriçao);
            comando.Parameters.AddWithValue("@Valor", (dto.valor));
            comando.Parameters.AddWithValue("@Id", id);

            int linhasAfetadas = comando.ExecuteNonQuery();
            if (linhasAfetadas == 0)
            {
                throw new Exception("Projeto não encontrado.");
            }
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
