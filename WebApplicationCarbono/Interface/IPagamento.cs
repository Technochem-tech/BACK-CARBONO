using MercadoPago.Resource.Payment;
using System.Threading.Tasks;

public interface IPagamento
{
    Task<Payment> CriarPagamentoPixAsync(decimal valor, string emailCliente);
    Task<string> ObterStatusPagamentoAsync(string pagamentoId);

    Task VerificarPagamentosPendentesAsync(); 

}
