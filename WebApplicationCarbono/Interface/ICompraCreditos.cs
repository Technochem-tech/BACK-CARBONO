using WebApplicationCarbono.Modelos;

public interface ICompraCreditos
{
    CompraCreditoResultado IniciarCompraCredito(ComprarCredito compra);
    Task<string> ConfirmarCompraWebhookAsync(MercadoPagoNotification notification);
   
   

}
