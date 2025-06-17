using WebApplicationCarbono.Dtos;
using WebApplicationCarbono.Serviços;

namespace WebApplicationCarbono.Interface
{
    public interface IProjetos
    {
        List<object> ListarProjetos();
        void CadastrarProjetos(CadastroProjetosDto dto, byte[] imagemBytes);
        void EditarProjeto (int id, EditarProjetoDto dto);
        void DeletarProjeto (int id);
        List<object> ListarProjetosPorValorAproximado(decimal valorEstimado);
    }
}
