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
    }
}
