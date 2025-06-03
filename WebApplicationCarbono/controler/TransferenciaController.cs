using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplicationCarbono.Interface;

namespace WebApplicationCarbono.controler
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransferenciaController : ControllerBase
    {

        private readonly ITransferencia _serviço;

        public TransferenciaController(ITransferencia serviço)
        {
            _serviço = serviço;
        }


        [HttpGet("verificar-destinatario")]
        public IActionResult VerificarDestinatario([FromQuery] string emailOuCnpj)
        {
            try
            {
                var resultado = _serviço.VerificarDestinatario(emailOuCnpj);
                return Ok(resultado);
            }
            catch (Exception ex)
            {

               return NotFound(new {mensagem = ex.Message});
            }
        }

    }
}
