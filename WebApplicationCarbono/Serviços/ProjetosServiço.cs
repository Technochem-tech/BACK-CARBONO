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
            _stringConexao = configuaração.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("DefaultConnection", "A string de conexão não pode ser nula.");
        }

        public void CadastrarProjetos(CadastroProjetosDto dto, byte[] imagemBytes)
        {
            using var conexao = new NpgsqlConnection(_stringConexao);
            conexao.Open();

            var comando = new NpgsqlCommand(
                "INSERT INTO projetos (titulo, descricao, valor, img_projetos) VALUES (@Titulo, @Descricao, @Valor, @Projetos)", conexao);

            comando.Parameters.AddWithValue("@Titulo", dto.titulo);
            comando.Parameters.AddWithValue("@Descricao", dto.descriçao);
            comando.Parameters.AddWithValue("@Valor", dto.valor);
            comando.Parameters.AddWithValue("@Projetos", imagemBytes);

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

                    var consulta = "SELECT titulo, valor, descricao, img_projetos FROM projetos";
                    using (var comando = new NpgsqlCommand(consulta, conexao))
                    {
                        using (var reader = comando.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                byte[]? imagemBytes = reader["img_projetos"] as byte[];

                                string? imagemBase64 = imagemBytes != null
                                    ? Convert.ToBase64String(imagemBytes)
                                    : null;

                                listaProjetos.Add(new
                                {
                                    titulo = reader["titulo"].ToString(),
                                    valor = Convert.ToDecimal(reader["valor"]),
                                    descricao = reader["descricao"].ToString(),
                                    imgBase64 = imagemBase64
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
