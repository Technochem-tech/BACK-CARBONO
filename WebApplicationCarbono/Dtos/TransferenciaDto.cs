namespace WebApplicationCarbono.Dtos
{
    public class TransferenciaDto
    {
        // usado no TransferenciaController, pq o id do rementente já é passado automaticamente pelo token.
        public string DestinatarioEmailOuCnpj { get; set; }
        public decimal QuantidadeCredito { get; set; }
    }
}