using Microsoft.AspNetCore.Mvc;
using WebApplicationCarbono.Dtos;
using WebApplicationCarbono.Interface;

namespace WebApplicationCarbono.Controllers
{
    [ApiController]
    [Route("api/verificacao-email")]
    public class VerificacaoEmailController : ControllerBase
    {
        private readonly IVerificacaoEmail _servico;

        public VerificacaoEmailController(IVerificacaoEmail servico)
        {
            _servico = servico;
        }

        [HttpPost("enviar")]
        public IActionResult Enviar([FromBody] string email)
        {
            _servico.EnviarCodigoVerificacao(email);
            return Ok("Código enviado para o e-mail.");
        }

        [HttpPost("confirmar")]
        public IActionResult Confirmar([FromBody] VerificarCodigoDto dto)
        {
            bool confirmado = _servico.ConfirmarCodigo(dto.Email, dto.Codigo);
            if (!confirmado)
                return BadRequest("Código inválido ou expirado.");

            return Ok("E-mail confirmado com sucesso.");
        }
    }
}
