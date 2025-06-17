namespace WebApplicationCarbono.Modelos
{
    // Modelo para representar a compra de créditos por um usuário
    public class ComprarCredito
    {
        public int IdUsuario { get; set; }
        public int IdProjeto { get; set; }  // novo campo
        public string EmailUsuario { get; set; } = string.Empty;
        public decimal ValorReais { get; set; }  // valor que o usuário digitou
    }

    // Resultado da compra de créditos, contendo o QR Code e o ID do pagamento
    public class CompraCreditoResultado
    {
        public string? QrCode { get; set; }
        public string? PagamentoId { get; set; }
    }

    // Modelo para deserializar a notificação webhook MercadoPago
    public class MercadoPagoNotification
    {
        public string Type { get; set; } = string.Empty;
        public DataObject Data { get; set; } = new DataObject();

        // Classe interna para representar o objeto de dados da notificação
        public class DataObject
        {
            public string Id { get; set; } = string.Empty;
        }
    }
}
