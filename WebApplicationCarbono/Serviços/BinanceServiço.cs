using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace WebApplicationCarbono.Serviços
{
    public class BinanceServico : IBinanceServico
    {
        private readonly string apiKey;
        private readonly string secretKey;
        private readonly string baseUrl = "https://api.binance.com";

        public BinanceServico(IConfiguration configuration)
        {
            apiKey = configuration["Binance:ApiKey"] ?? throw new ArgumentNullException("Binance:ApiKey não configurado");
            secretKey = configuration["Binance:SecretKey"] ?? throw new ArgumentNullException("Binance:SecretKey não configurado");
        }

        public async Task<string> ComprarCriptoAsync(string symbol, decimal quantidade)
        {
            using var client = new HttpClient();

            // 👇 Ajustar quantidade antes de comprar
            quantidade = await AjustarQuantidadeParaRegrasAsync(symbol, quantidade);

            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var query = $"symbol={symbol}&side=BUY&type=MARKET&quantity={quantidade.ToString(CultureInfo.InvariantCulture)}&timestamp={timestamp}";
            string signature = GerarAssinatura(query, secretKey);

            string url = $"{baseUrl}/api/v3/order?{query}&signature={signature}";
            client.DefaultRequestHeaders.Add("X-MBX-APIKEY", apiKey);

            var response = await client.PostAsync(url, null);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Erro ao comprar: {content}");

            return content;
        }

        public async Task<Dictionary<string, decimal>> ObterSaldosAsync()
        {
            using var client = new HttpClient();

            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            string query = $"timestamp={timestamp}&recvWindow=60000";
            string signature = GerarAssinatura(query, secretKey);

            string url = $"{baseUrl}/api/v3/account?{query}&signature={signature}";
            client.DefaultRequestHeaders.Add("X-MBX-APIKEY", apiKey);

            var response = await client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Erro da Binance: {content}");

            var resultado = JsonConvert.DeserializeObject<BinanceAccountResponse>(content);

            if (resultado == null || resultado.balances == null)
                throw new Exception("Resposta inválida da Binance");

            var saldos = new Dictionary<string, decimal>();

            foreach (var b in resultado.balances)
            {
                if (decimal.TryParse(b.free.Replace(".", ","), out var quantidade) && quantidade > 0)
                {
                    saldos[b.asset] = Math.Round(quantidade, 8);
                }
            }

            return saldos;
        }

        private string GerarAssinatura(string query, string secret)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secret);
            var messageBytes = Encoding.UTF8.GetBytes(query);

            using var hmac = new HMACSHA256(keyBytes);
            var hash = hmac.ComputeHash(messageBytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        // ✅ Função para ajustar a quantidade com base nas regras da Binance
        private async Task<decimal> AjustarQuantidadeParaRegrasAsync(string symbol, decimal quantidadeDesejada)
        {
            using var client = new HttpClient();
            var url = $"{baseUrl}/api/v3/exchangeInfo?symbol={symbol.ToUpper()}";

            var response = await client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Erro ao obter info de símbolo: {content}");

            var data = JsonConvert.DeserializeObject<ExchangeInfoResponse>(content);

            if (data == null || data.symbols == null || !data.symbols.Any())
                throw new Exception("Informações do símbolo não encontradas");

            var lotSizeFilter = data.symbols[0].filters
                .FirstOrDefault(f => f.filterType == "LOT_SIZE");

            if (lotSizeFilter == null)
                throw new Exception("LOT_SIZE não encontrado");

            decimal stepSize = decimal.Parse(lotSizeFilter.stepSize, CultureInfo.InvariantCulture);
            decimal minQty = decimal.Parse(lotSizeFilter.minQty, CultureInfo.InvariantCulture);

            // Arredonda a quantidade para baixo, de acordo com o stepSize
            decimal quantidadeAjustada = Math.Floor(quantidadeDesejada / stepSize) * stepSize;

            if (quantidadeAjustada < minQty)
                throw new Exception($"Quantidade ajustada ({quantidadeAjustada}) é menor que o mínimo permitido ({minQty})");

            return quantidadeAjustada;
        }

        // ✅ Classes auxiliares para parse da resposta
        private class BinanceAccountResponse
        {
            public List<Balance>? balances { get; set; }
        }

        private class Balance
        {
            public string asset { get; set; } = string.Empty;
            public string free { get; set; } = string.Empty;
            public string locked { get; set; } = string.Empty;
        }

        private class ExchangeInfoResponse
        {
            public List<ExchangeSymbol> symbols { get; set; }
        }

        private class ExchangeSymbol
        {
            public string symbol { get; set; }
            public List<ExchangeFilter> filters { get; set; }
        }

        private class ExchangeFilter
        {
            public string filterType { get; set; }
            public string minQty { get; set; }
            public string stepSize { get; set; }
        }
    }
}
