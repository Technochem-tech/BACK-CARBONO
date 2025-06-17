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
                "INSERT INTO projetos (titulo, descricao, valor, img_projetos, creditos_disponivel) VALUES (@Titulo, @Descricao, @Valor, @Projetos, @creditos_disponivel)", conexao);

            comando.Parameters.AddWithValue("@Titulo", dto.titulo);
            comando.Parameters.AddWithValue("@Descricao", dto.descriçao);
            comando.Parameters.AddWithValue("@Valor", dto.valor);
            comando.Parameters.AddWithValue("@Projetos", imagemBytes);
            comando.Parameters.AddWithValue("@creditos_disponivel", dto.creditosDisponivel);

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

            var campos = new List<string>();
            var comando = new NpgsqlCommand();
            comando.Connection = conexao;

            if (!string.IsNullOrEmpty(dto.titulo))
            {
                campos.Add("titulo = @Titulo");
                comando.Parameters.AddWithValue("@Titulo", dto.titulo);
            }

            if (!string.IsNullOrEmpty(dto.descriçao))
            {
                campos.Add("descricao = @Descricao");
                comando.Parameters.AddWithValue("@Descricao", dto.descriçao);
            }

            if (dto.valor.HasValue)
            {
                campos.Add("valor = @Valor");
                comando.Parameters.AddWithValue("@Valor", dto.valor.Value);
            }

            if (dto.creditosDisponivel.HasValue)
            {
                campos.Add("creditos_disponivel = @Creditos");
                comando.Parameters.AddWithValue("@Creditos", dto.creditosDisponivel.Value);
            }

            if (dto.imagemBytes != null)
            {
                campos.Add("img_projetos = @Imagem");
                comando.Parameters.AddWithValue("@Imagem", dto.imagemBytes);
            }

            if (campos.Count == 0)
            {
                throw new Exception("Nenhum campo enviado para atualização.");
            }

            comando.CommandText = $"UPDATE projetos SET {string.Join(", ", campos)} WHERE id = @Id";
            comando.Parameters.AddWithValue("@Id", id);

            int linhas = comando.ExecuteNonQuery();
            if (linhas == 0)
            {
                throw new Exception("Projeto não encontrado para editar.");
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

                    var consulta = "SELECT id, titulo, valor, descricao, img_projetos, creditos_disponivel FROM projetos";
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
                                {   id = reader["id"],
                                    titulo = reader["titulo"].ToString(),
                                    valor = Convert.ToDecimal(reader["valor"]),
                                    descricao = reader["descricao"].ToString(),
                                    imgBase64 = imagemBase64,
                                    creditosDisponivel = Convert.ToDecimal(reader["creditos_disponivel"])

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

