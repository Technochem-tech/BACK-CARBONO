using MercadoPago.Client.Payment;
using MercadoPago.Resource.Payment;
using System.Threading.Tasks;

public class PagamentoService : IPagamentoService
{
    public async Task<Payment> CriarPagamentoPixAsync(decimal valor, string emailCliente)
    {
        var paymentRequest = new PaymentCreateRequest
        {
            TransactionAmount = valor,
            Description = "Pagamento via Pix - Créditos de Carbono",
            PaymentMethodId = "pix",
            Payer = new PaymentPayerRequest
            {
                Email = emailCliente
            }
        };

        var client = new PaymentClient();
        var payment = await client.CreateAsync(paymentRequest);
        return payment;
    }
}
