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
            throw new NotImplementedException();
        }

       
    }
}
