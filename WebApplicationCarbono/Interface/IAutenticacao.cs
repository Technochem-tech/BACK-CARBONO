using WebApplicationCarbono.Dtos;

namespace WebApplicationCarbono.Interface
{
    public interface IAutenticacao
    {
        string Logar(LoginUsuarioDto loginDto);
    }
}
