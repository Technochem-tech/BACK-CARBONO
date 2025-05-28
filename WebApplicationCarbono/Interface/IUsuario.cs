using WebApplicationCarbono.Dtos;
using WebApplicationCarbono.Modelos;

namespace WebApplicationCarbono.Interface
{
    public interface IUsuario
    {
        BuscarUsuario GetUsuario(int IdUsuario);
        void CadastrarUsuario(CadastroUsuarioDto cadastroUsuarioDto);
        void EditarTelefone(int id, EditarTelefoneUsuarioDto dto);
    }
}
