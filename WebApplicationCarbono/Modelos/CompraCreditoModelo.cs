


// Modelo para representar a compra de créditos por um usuário
public class ComprarCredito
{
   
    public int idUsuario { get; set; }
    public decimal quantidadeCredito { get; set; }
    public string emailUsuario { get; set; }

}
// Resultado da compra de créditos, contendo o QR Code e o ID do pagamento
public class CompraCreditoResultado
{
    public string qrCode { get; set; }
    public string pagamentoId { get; set; }
}

// Modelo para deserializar a notificação webhook MercadoPago
public class MercadoPagoNotification
{
    public string Type { get; set; }
    public DataObject Data { get; set; }

    // Classe interna para representar o objeto de dados da notificação
    public class DataObject
    {
        public string Id { get; set; }
    }
}
