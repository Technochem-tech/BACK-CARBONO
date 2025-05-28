using WebApplicationCarbono.Dtos;
using WebApplicationCarbono.Serviços;

namespace WebApplicationCarbono.Interface
{
    public interface IProjetos
    {
        List<object> ListarProjetos();
        void CadastrarProjetos (CadastroProjetosDto dto);
        void EditarProjeto (int id, EditarProjetoDto dto);
        void DeletarProjeto (int id);
    }
}
