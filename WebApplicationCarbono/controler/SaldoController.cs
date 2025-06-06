using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplicationCarbono.Interface;
using WebApplicationCarbono.Modelos;
using WebApplicationCarbono.Serviços;

namespace WebApplicationCarbono.controler
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SaldoController : ControllerBase
    {
        private readonly ISaldo _saldoServiços;

        public SaldoController(ISaldo SaldoServiços)
        {
            _saldoServiços = SaldoServiços;
        }

        [HttpGet("GetSaldo/")]
        public IActionResult GetSaldo ()
        {
            try
            {   var idUsuario = Helpers.UserHelper.ObterIdUsuarioLogado(HttpContext);
                if (idUsuario == 0)
                {
                    return Unauthorized(new { erro = "Usuário não autenticado corretamente." });
                }

                var saldo = _saldoServiços.GetSaldo(idUsuario);
                return Ok(new {saldoemconta =  saldo});
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }

    }
}
