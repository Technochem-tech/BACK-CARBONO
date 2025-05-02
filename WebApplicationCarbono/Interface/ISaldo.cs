using WebApplicationCarbono.Modelos;

namespace WebApplicationCarbono.Interface
{
    public interface ISaldo
    {
        decimal GetSaldo(int IdUsuario);
        decimal GetCreditos(int IdUsuario);
        List<object> ListarProjetos();
    }
}
