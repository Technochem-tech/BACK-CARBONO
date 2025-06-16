using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplicationCarbono.Interface;

namespace WebApplicationCarbono.Controller
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class HistoricoTransacaoController : ControllerBase
    {
        private readonly IHistoricoTransacao _transacaoServico;

        public HistoricoTransacaoController(IHistoricoTransacao transacaoServico)
        {
            _transacaoServico = transacaoServico;
        }

      
        [HttpGet("ConsultarHistorico")]
        public IActionResult ConsultarHistorico(
            [FromQuery] DateTime? dataInicio = null,
            [FromQuery] DateTime? dataFim = null,
            [FromQuery] string? tipo = null)
        {
            try
            {
                var idUsuario = Helpers.UserHelper.ObterIdUsuarioLogado(HttpContext);
                if (idUsuario == 0)
                {
                    return Unauthorized(new { erro = "Usuário não autenticado corretamente." });
                }

                var historico = _transacaoServico.BuscarTransacoes(idUsuario, dataInicio, dataFim, tipo);
                return Ok(new { historicodetransacao = historico });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }
    }
}
