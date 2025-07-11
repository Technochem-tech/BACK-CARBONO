
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
                if(decimal.TryParse(b.free.Replace(".", ","), out var quantidade) && quantidade > 0)
                {
                        saldos[b.asset] = Math.Round(quantidade, 8); // mantém precisão
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

        private class BinanceAccountResponse
        {
            public List<Balance> balances { get; set; }
        }

        private class Balance
        {
            public string asset { get; set; } = string.Empty;
            public string free { get; set; } = string.Empty;
            public string locked { get; set; } = string.Empty;
        }
    }
}
