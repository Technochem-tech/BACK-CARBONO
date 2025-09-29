
namespace WebApplicationCarbono.Serviços
{
    public interface IBinanceServico
    {
        Task<Dictionary<string, decimal>> ObterSaldosAsync();
        Task<string> ComprarPorValorAsync(string symbol, decimal valorEmReais);
        Task<(decimal valorMinimoReais, decimal quantidadeMinima)> ObterCompraMinimaAsync(string symbol);
    }
}

