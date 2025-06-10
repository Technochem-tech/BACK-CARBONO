using Npgsql;
using System.Drawing;
using WebApplicationCarbono.Interface;
using WebApplicationCarbono.Modelos;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace WebApplicationCarbono.Serviços
{
    public class SaldoServiço : ISaldo
    {
        // passando a conexão do banco
        private readonly String _stringConexao;
        public SaldoServiço(IConfiguration configuaração)
        {
            _stringConexao = configuaração.GetConnectionString("DefaultConnection");
        }




        public decimal GetSaldo(int IdUsuario)
        {
            decimal saldoEmConta = 0.00m;

            try
            {
                using (var conexao = new NpgsqlConnection(_stringConexao))
                {
                    conexao.Open();

                    var comando = new NpgsqlCommand(
                    "SELECT COALESCE(SUM(valor), 0) FROM saldo_usuarios_dinamica WHERE usuario_id = @usuarioId", conexao);
                    comando.Parameters.AddWithValue("usuarioId", IdUsuario);
                    var saldo = (decimal)comando.ExecuteScalar();
                    return saldoEmConta = saldo;
                }


            }
            catch (Exception ex)
            {

                throw new Exception("Erro ao buscar o saldo: " + ex.Message);
            }


        }




    }
}
