using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebApplicationCarbono.Serviços;

namespace WebApplicationCarbono.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BinaceController : ControllerBase
    {
        private readonly IBinanceServico _binanceServico;

        public BinaceController(IBinanceServico binanceServico)
        {
            _binanceServico = binanceServico;
        }

        [HttpGet("Cripto-Saldo")]
        public async Task<IActionResult> GetSaldo()
        {
            try
            {
                var saldos = await _binanceServico.ObterSaldosAsync();
                return Ok(saldos);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }
    }
}
