using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplicationCarbono.Interface;
using WebApplicationCarbono.Serviços;

namespace WebApplicationCarbono.controler
{
    [Route("api/[controller]")]
    [ApiController]
    public class CreditosController : ControllerBase
    {

       private readonly ICreditos _CreditosServiços;
       public CreditosController(ICreditos CreditosServiços) 
       {
            _CreditosServiços = CreditosServiços;
       }

        [HttpGet("GetCreditos/{IdUsuario}")]
        public IActionResult GetCreditos(int IdUsuario)
        {
            try
            {
                var creditosCarbono = _CreditosServiços.GetCreditos(IdUsuario);
                return Ok((new { creditosdecarbonoemconta = creditosCarbono }));
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }
    }
}
