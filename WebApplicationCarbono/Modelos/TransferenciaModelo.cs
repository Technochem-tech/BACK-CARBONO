namespace WebApplicationCarbono.Modelos
{
    public class TransferenciaModelo
    {
        public int RemetenteId { get; set; }
        public string DestinatarioEmailOuCnpj { get; set; }
        public decimal QuantidadeCredito { get; set; }
    }

    public class TransferenciaModeloSaldo
    {
        public int RemetenteId { get; set; }
        public string DestinatarioEmailOuCnpj { get; set; }
        public decimal QuantidadeSaldo { get; set; }
        public string Descricao { get; set; }
    }
}
