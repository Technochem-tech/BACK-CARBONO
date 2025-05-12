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

        [HttpGet("GetSaldo/{IdUsuario}")]
        public IActionResult GetSaldo (int IdUsuario)
        {
            try
            {
                var saldo = _saldoServiços.GetSaldo(IdUsuario);
                return Ok(new {saldoemconta =  saldo});
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }

    }
}
