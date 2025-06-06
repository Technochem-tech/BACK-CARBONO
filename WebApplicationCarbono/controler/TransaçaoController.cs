using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplicationCarbono.Interface;
using WebApplicationCarbono.Serviços;

namespace WebApplicationCarbono.controler
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TransaçaoController : ControllerBase
    {
        private readonly ITransaçao _trasaçaoServiços;

        public TransaçaoController(ITransaçao TrasaçaoSaldo)
        {
            _trasaçaoServiços = TrasaçaoSaldo;
        }

        [HttpGet("ConsultarHistorico")]
        public IActionResult ConsultarHistorico()
        {
            try
            {   var idUsuario = Helpers.UserHelper.ObterIdUsuarioLogado(HttpContext);
                if (idUsuario == 0)
                {
                    return Unauthorized(new { erro = "Usuário não autenticado corretamente." });
                }

                var historico = _trasaçaoServiços.ConsultarHistorico(idUsuario);
                return Ok(new { historicodetransacao = historico });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
