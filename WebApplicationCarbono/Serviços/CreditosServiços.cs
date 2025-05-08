using Npgsql;
using WebApplicationCarbono.Interface;

namespace WebApplicationCarbono.Serviços
{
    public class CreditosServiços : ICreditos
    {

        private readonly string _stringConexao;

        public CreditosServiços(IConfiguration configuração)
        {
            _stringConexao = configuração.GetConnectionString("DefaultConnection");
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
    }
}
