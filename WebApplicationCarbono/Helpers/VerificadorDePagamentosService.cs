using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using WebApplicationCarbono.Interface;

public class VerificadorDePagamentosService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public VerificadorDePagamentosService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    // Método que será executado em segundo plano
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var pagamento = scope.ServiceProvider.GetRequiredService<IPagamento>();

            try
            {
                await pagamento.VerificarPagamentosPendentesAsync();
                await pagamento.VerificarPagamentosAprovadosasync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao verificar pagamentos pendentes: {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Executa a cada 5 minutos
        }
    }
}
