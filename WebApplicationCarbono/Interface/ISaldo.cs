using WebApplicationCarbono.Modelos;

namespace WebApplicationCarbono.Interface
{
    public interface ISaldo
    {
        decimal GetSaldo(int IdUsuario);
   
        List<object> ListarProjetos();
        List<object> ConsultarHistorico();
        BuscarUsuario GetUsuario(int IdUsuario);



    }
}
