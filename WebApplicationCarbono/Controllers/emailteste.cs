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

        // Classe modelo para receber os dados do corpo da requisição
        public class EmailRequest
        {
            public string Destinatario { get; set; } = string.Empty;
        }

        [HttpPost("teste")]
        public IActionResult TestarEnvio([FromBody] EmailRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Destinatario))
                return BadRequest("O campo 'Destinatario' é obrigatório.");

            _gmail.EnviarEmailAsync(request.Destinatario, "Teste", "Esse é um teste de envio Gmail API");

            return Ok($"E-mail enviado para {request.Destinatario}!");
        }
    }
}
