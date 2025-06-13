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
            _stringConexao = configuaração.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("DefaultConnection");
        }

        public async Task<decimal> SaldoCredito(int IdUsuario)
        {
            decimal saldoEmConta = 0.00m;

            try
            {
                await using (var conexao = new NpgsqlConnection(_stringConexao))
                {
                    conexao.Open();

                    var comando = new NpgsqlCommand(@"SELECT COALESCE(SUM(valor_creditos), 0) FROM saldo_usuario_dinamica 
                        WHERE id_usuario = @idUsuario;", conexao);
                    comando.Parameters.AddWithValue("idUsuario", IdUsuario);

                    var saldoObj = await comando.ExecuteScalarAsync();
                    if (saldoObj != null && saldoObj != DBNull.Value)
                    {
                        return saldoEmConta = Convert.ToDecimal(saldoObj);
                    }
                    else
                    {
                        return saldoEmConta = 0.00m;
                    }

                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao buscar o saldo: " + ex.Message);
            }

        }

        public async Task<decimal> SaldoDinheiro(int IdUsuario)
        {
            decimal saldoEmConta = 0.00m;
            try
            {
                await using (var conexao = new NpgsqlConnection(_stringConexao))
                {
                    conexao.Open();

                    var comando = new NpgsqlCommand(@"SELECT COALESCE(SUM(saldo_dinheiro), 0) FROM saldo_usuario_dinamica 
                        WHERE id_usuario = @idUsuario;", conexao);
                    comando.Parameters.AddWithValue("idUsuario", IdUsuario);

                    var saldoObj = await comando.ExecuteScalarAsync();

                    if (saldoObj != null && saldoObj != DBNull.Value)
                    {
                        return saldoEmConta = Convert.ToDecimal(saldoObj);
                    }
                    else
                    {
                        return saldoEmConta = 0.00m; 
                    }

                }

            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao buscar o saldo: " + ex.Message);
            }
        }

        
    }
}
