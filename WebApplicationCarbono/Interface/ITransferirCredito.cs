using WebApplicationCarbono.Modelos;

namespace WebApplicationCarbono.Interface
{
    public interface ITransferirCredito
    {
        object VerificarDestinatario(string emailOuCnpj);
        string RealizarTransferencia(TransferenciaModelo transferencia);
    }
}
