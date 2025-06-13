using WebApplicationCarbono.Modelos;

namespace WebApplicationCarbono.Interface
{
    public interface ISaldo
    {
        Task<decimal> SaldoDinheiro (int IdUsuario);
        Task<decimal> SaldoCredito(int IdUsuario);

    }
}
