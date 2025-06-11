using MercadoPago.Client.Common;
using MercadoPago.Client.Payment;
using MercadoPago.Resource.Payment;

namespace WebApplicationCarbono.Interface
{
    public interface IVendaCredito
    {
        Task<string> RealizarVendaAsync(int idUsuario, decimal quantidadeCreditos);


    }
}
