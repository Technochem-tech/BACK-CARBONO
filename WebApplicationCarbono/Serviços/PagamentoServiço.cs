using MercadoPago.Client.Payment;
using MercadoPago.Resource.Payment;

public class PagamentoServico : IPagamento
{
  
    public async Task<Payment> CriarPagamentoPixAsync(decimal valor, string emailCliente)
    {
        var paymentRequest = new PaymentCreateRequest
        {
            TransactionAmount = valor,
            Description = "Pagamento via Pix - Créditos de Carbono",
            PaymentMethodId = "pix",
            Payer = new PaymentPayerRequest { Email = emailCliente }
        };

        var client = new PaymentClient();
        var payment = await client.CreateAsync(paymentRequest);
        return payment;
    }

    public async Task<string> ObterStatusPagamentoAsync(string pagamentoId)
    {
        if (!long.TryParse(pagamentoId, out long idPagamento))
            throw new ArgumentException("ID de pagamento inválido.");

        var client = new PaymentClient();
        var pagamento = await client.GetAsync(idPagamento);
        return pagamento.Status; // "approved", "pending", etc.
    }
}
