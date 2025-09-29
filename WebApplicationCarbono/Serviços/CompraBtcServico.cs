using Npgsql;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace WebApplicationCarbono.Serviços
{
    public class CompraBtcServico
    {
        private readonly string _conexao;
        private readonly BinanceServico _binanceServico;

        public CompraBtcServico(IConfiguration config, BinanceServico binanceServico)
        {
            _conexao = config.GetConnectionString("DefaultConnection")
                       ?? throw new ArgumentNullException("DefaultConnection");
            _binanceServico = binanceServico;
        }

        public async Task ProcessarComprasPendentesAsync()
        {
            using var conexao = new NpgsqlConnection(_conexao);
            await conexao.OpenAsync();

            // 🔹 Busca todas as compras de BTC pendentes
            using var selectCmd = new NpgsqlCommand(@"
                SELECT id, id_usuario, valor_reais, quantidade_creditos 
                FROM compra_btc
                WHERE status = 'Pendente'", conexao);

            using var reader = await selectCmd.ExecuteReaderAsync();

            var pendentes = new List<(int id, int idUsuario, decimal valorReais, decimal creditos)>();
            while (await reader.ReadAsync())
            {
                pendentes.Add((
                    reader.GetInt32(0),
                    reader.GetInt32(1),
                    reader.GetDecimal(2),
                    reader.GetDecimal(3)
                ));
            }
            reader.Close();

            foreach (var pendente in pendentes)
            {
                try
                {
                    // 🔹 Faz a compra de BTC na Binancestring resposta = await _binanceServico.ComprarPorValorAsync("BTCBRL", pendente.valorReais);
                    string resposta = await _binanceServico.ComprarPorValorAsync("BTCBRL", pendente.valorReais);


                    // 🔹 Descobre quanto de BTC foi comprado
                    decimal quantidadeBtc = ExtrairQuantidadeBtc(resposta);

                    // 🔹 Atualiza a linha como concluída
                    using var updateCmd = new NpgsqlCommand(@"
                        UPDATE compra_btc
                        SET status = 'Concluido',
                            descricao = @descricao,
                            quantidade_btc = @quantidadeBtc
                        WHERE id = @id", conexao);

                    updateCmd.Parameters.AddWithValue("@descricao", $"✅ Compra de BTC realizada com sucesso. Resposta Binance: {resposta}");
                    updateCmd.Parameters.AddWithValue("@quantidadeBtc", quantidadeBtc);
                    updateCmd.Parameters.AddWithValue("@id", pendente.id);

                    await updateCmd.ExecuteNonQueryAsync();
                }
                catch (Exception ex)
                {
                    // 🔹 Atualiza como erro
                    using var updateCmd = new NpgsqlCommand(@"
                        UPDATE compra_btc
                        SET status = 'Erro',
                            descricao = @descricao
                        WHERE id = @id", conexao);

                    updateCmd.Parameters.AddWithValue("@descricao", $"❌ Erro ao comprar BTC: {ex.Message}");
                    updateCmd.Parameters.AddWithValue("@id", pendente.id);

                    await updateCmd.ExecuteNonQueryAsync();
                }
            }
        }

        // 🔹 Extrair quantidade de BTC da resposta JSON da Binance
        private decimal ExtrairQuantidadeBtc(string respostaJson)
        {
            try
            {
                dynamic obj = Newtonsoft.Json.JsonConvert.DeserializeObject(respostaJson);
                if (obj != null && obj.fills != null && obj.fills.Count > 0)
                {
                    return Convert.ToDecimal((string)obj.fills[0].qty, System.Globalization.CultureInfo.InvariantCulture);
                }
            }
            catch { }
            return 0;
        }
    }
}
