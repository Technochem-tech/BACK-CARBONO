using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplicationCarbono.Modelos
{
    [Table("saldos")]
    public class Saldo
    {
        public int IdUsuario { get; set; }
        public decimal SaldoConta { get; set; }
        public decimal CreditosCarbono { get; set; }

    }
}
