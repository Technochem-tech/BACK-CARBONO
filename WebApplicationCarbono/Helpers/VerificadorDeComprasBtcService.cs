using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using WebApplicationCarbono.Serviços;

namespace WebApplicationCarbono.Helpers
{
    public class VerificadorDeComprasBtcService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public VerificadorDeComprasBtcService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();
                var compraBtcServico = scope.ServiceProvider.GetRequiredService<CompraBtcServico>();

                try
                {
                    await compraBtcServico.ProcessarComprasPendentesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao processar compras de BTC: {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken); // Executa a cada 1 minuto
            }
        }
    }
}
