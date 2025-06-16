using WebApplicationCarbono.Dtos.WebApplicationCarbono.Dtos;

namespace WebApplicationCarbono.Interface
{
    public interface IHistoricoTransacao
    {
        List<HistoricoTransacaoDto> BuscarTransacoes(int idUsuario, DateTime? dataInicio, DateTime? dataFim, string? tipo);

    }
}
