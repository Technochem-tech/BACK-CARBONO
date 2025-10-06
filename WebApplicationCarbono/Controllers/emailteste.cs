using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplicationCarbono.Serviços;

namespace WebApplicationCarbono.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailTesteController : ControllerBase
    {
        private readonly GmailServico _gmail;

        public EmailTesteController(GmailServico gmail)
        {
            _gmail = gmail;
        }

        [HttpGet]
        public IActionResult TestarEnvio()
        {
            _gmail.EnviarEmail("marlonjose150404@gmail.com", "Teste", "Esse é um teste de envio Gmail API");
            return Ok("E-mail enviado!");
        }
    }
}