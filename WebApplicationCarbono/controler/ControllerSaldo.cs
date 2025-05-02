using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplicationCarbono.Interface;
using WebApplicationCarbono.Serviços;

namespace WebApplicationCarbono.controler
{
    [Route("api/[controller]")]
    [ApiController]
    public class ControllerSaldo : ControllerBase
    {
        private readonly ISaldo _saldoServiços;

        public ControllerSaldo(ISaldo SaldoServiços)
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

        [HttpGet("GetCreditos/{IdUsuario}")]
        public IActionResult GetCreditos (int IdUsuario)
        {
            try
            {
                var creditosCarbono = _saldoServiços.GetCreditos(IdUsuario);
                return Ok((new{ creditosdecarbonoemconta =  creditosCarbono}));
            }
            catch (Exception ex)
            {

               return BadRequest(ex.Message);
            }
        }


        [HttpGet]
        public IActionResult ListarProjetos()
        {
            try
            {
                var projetos = _saldoServiços.ListarProjetos();
                return Ok(new { projetossustentaveis = projetos });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
