﻿
namespace WebApplicationCarbono.Serviços
{
    public interface IBinanceServico
    {
        Task<Dictionary<string, decimal>> ObterSaldosAsync();
        Task<string> ComprarCriptoAsync(string symbol, decimal quantidade);
    }
}

