namespace WebApplicationCarbono.Dtos
{
    namespace WebApplicationCarbono.Dtos
    {
        public class HistoricoTransacaoDto
        {
            public DateTime DataHora { get; set; }
            public string Descricao { get; set; } = string.Empty;
            public decimal Quantidade { get; set; }
            public string Status { get; set; } = string.Empty;
            public string Tipo { get; set; } = string.Empty; // Ex: "Compra", "Venda", "Transferência"
            public string? CopiaColaPix { get; set; }

        }
    }
}
