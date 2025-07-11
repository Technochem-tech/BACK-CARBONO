
namespace WebApplicationCarbono.Serviços
{
    public interface IBinanceServico
    {
        Task<Dictionary<string, decimal>> ObterSaldosAsync();
    }
}
