using System;
using BCrypt.Net;
using Npgsql;
using WebApplicationCarbono.Dtos;
using WebApplicationCarbono.Interface;
using WebApplicationCarbono.Modelos;
using Microsoft.Extensions.Configuration;

namespace WebApplicationCarbono.Serviços
{
    public class UsuarioServiço : IUsuario
    {
        private readonly string _stringConexao;

        public UsuarioServiço(IConfiguration configuration)
        {
            _stringConexao = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("A string de conexão não foi configurada.");
        }

        

        public void CadastrarUsuario(CadastroUsuarioDto cadastroUsuarioDto)
        {
            try
            {
                using (var conexao = new NpgsqlConnection(_stringConexao))
                {
                    conexao.Open();

                    
                    string senhaHash = BCrypt.Net.BCrypt.HashPassword(cadastroUsuarioDto.Senha);

                    string query = @"
                        INSERT INTO usuarios (nome, email, senha, empresa, cnpj, telefone)
                        VALUES (@nome, @email, @senha, @empresa, @cnpj, @telefone);
                    ";

                    using (var comando = new NpgsqlCommand(query, conexao))
                    {
                        comando.Parameters.AddWithValue("@nome", cadastroUsuarioDto.Nome);
                        comando.Parameters.AddWithValue("@email", cadastroUsuarioDto.Email);
                        comando.Parameters.AddWithValue("@senha", senhaHash);
                        comando.Parameters.AddWithValue("@empresa", cadastroUsuarioDto.Empresa);
                        comando.Parameters.AddWithValue("@cnpj", cadastroUsuarioDto.CNPJ);
                        comando.Parameters.AddWithValue("@telefone", cadastroUsuarioDto.Telefone);

                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao cadastrar usuário: " + ex.Message);
            }
        }

        public void EditarTelefone(int id, EditarTelefoneUsuarioDto dto)
        {
            using var conexao = new NpgsqlConnection(_stringConexao);
            conexao.Open();

            var comando = new NpgsqlCommand("UPDATE usuarios SET telefone = @Telefone WHERE id = @Id", conexao);
            comando.Parameters.AddWithValue("@Telefone", dto.telefone);
            comando.Parameters.AddWithValue("@Id", id);

            int linhasAfetadas = comando.ExecuteNonQuery();
            if (linhasAfetadas == 0)
            {
                throw new Exception("Projeto não encontrado.");
            }


        }

        public BuscarUsuarioModelo? GetUsuario(int IdUsuario)
        {
            BuscarUsuarioModelo? usuario = null;

            try
            {
                using var conexao = new NpgsqlConnection(_stringConexao);
                conexao.Open();

                var query = "SELECT * FROM usuarios WHERE id = @Id";
                using var comando = new NpgsqlCommand(query, conexao);
                comando.Parameters.AddWithValue("Id", IdUsuario);

                using var reader = comando.ExecuteReader();
                if (reader.Read())
                {
                    usuario = new BuscarUsuarioModelo
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("id")),
                        Nome = reader.GetString(reader.GetOrdinal("nome")),
                        Email = reader.GetString(reader.GetOrdinal("email")),
                        empresa = reader.GetString(reader.GetOrdinal("empresa")),
                        CNPJ = reader.GetString(reader.GetOrdinal("cnpj")),
                        Telefone = reader.GetString(reader.GetOrdinal("telefone")),
                        DataCadastro = reader.GetDateTime(reader.GetOrdinal("data_registro")),
                    };
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao consultar usuário: " + ex.Message);
            }

            return usuario;
        }




        public byte[] BuscarImagemUsuario(int idUsuario)
        {
            using var conexao = new NpgsqlConnection(_stringConexao); 
            conexao.Open();

            var comado = new NpgsqlCommand("SELECT img_usuario FROM usuarios WHERE id = @Id", conexao);
            comado.Parameters.AddWithValue("@Id", idUsuario);
            var resultado = comado.ExecuteScalar();

            if (resultado == DBNull.Value || resultado == null)
            {
                throw new Exception("Imagem não encontrada.");
            }

            return (byte[])resultado;
        }

        public void SalvarOuAtualizarImagem(int idUsuario, IFormFile imagem)
        {
            using var conexao = new NpgsqlConnection(_stringConexao);
            conexao.Open();

            using var stream = new MemoryStream();
            imagem.CopyTo(stream);
            byte[] bytesImagem = stream.ToArray();

            var comando = new NpgsqlCommand("UPDATE usuarios SET img_usuario = @Imagem WHERE id = @Id", conexao);
            comando.Parameters.AddWithValue("@Imagem", bytesImagem);
            comando.Parameters.AddWithValue("@Id", idUsuario);

            int linhasAfetadas = comando.ExecuteNonQuery();
            if (linhasAfetadas == 0)
            {
                throw new Exception("Usuário não encontrado.");
            }


        }

        public void DeletarImagemUsuario(int idUsuario)
        {
            using var conexao = new NpgsqlConnection( _stringConexao);
            conexao.Open();

            var comandoSelect = new NpgsqlCommand("SELECT img_usuario FROM usuarios WHERE id = @Id", conexao);
            comandoSelect.Parameters.AddWithValue("@Id", idUsuario);

            var resultado = comandoSelect.ExecuteScalar();

            if (resultado == null)
            {
                throw new Exception("Usuário não encontrado.");
            }

            if (resultado == DBNull.Value)
            {
                throw new Exception("Nenhuma imagem para deletar.");
            }

            var comandoUpdate = new NpgsqlCommand("UPDATE usuarios SET img_usuario = NULL WHERE id = @Id", conexao);
            comandoUpdate.Parameters.AddWithValue("@Id", idUsuario);
            comandoUpdate.ExecuteNonQuery();

        }
    }
}
