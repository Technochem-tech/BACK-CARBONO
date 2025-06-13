using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplicationCarbono.Interface;

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

        [HttpGet("GetSaldo-dinheiro/")]
        public async Task<IActionResult> SaldoDinheiro()
        {
            try
            {
                var idUsuario = Helpers.UserHelper.ObterIdUsuarioLogado(HttpContext);
                if (idUsuario == 0)
                {
                    return Unauthorized(new { erro = "Usuário não autenticado corretamente." });
                }

                var saldo = await _saldoServiços.SaldoDinheiro(idUsuario);
                return Ok(new { saldoemconta = saldo }); 
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetSaldo-Creditos/")]
        public async Task<IActionResult> SaldoCredito()
        {
            try
            {
                var idUsuario = Helpers.UserHelper.ObterIdUsuarioLogado(HttpContext);
                if (idUsuario == 0)
                {
                    return Unauthorized(new { erro = "Usuário não autenticado corretamente." });
                }

                var saldo = await _saldoServiços.SaldoCredito(idUsuario);
                return Ok(new { saldoemconta = saldo });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
