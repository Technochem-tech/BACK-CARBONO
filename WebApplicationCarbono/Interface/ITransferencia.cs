using WebApplicationCarbono.Modelos;

namespace WebApplicationCarbono.Interface
{
    public interface ITransferencia
    {
        object VerificarDestinatario(string emailOuCnpj);
        string RealizarTransferencia(TransferenciaModelo transferencia);
    }
}
