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
            try
            {
                _servico.EnviarCodigoVerificacao(email);
                return Ok(new { mensagem = "Codigo de Verificação Enviado Para Email Informado" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }


        [HttpPost("confirmar")]
        public IActionResult Confirmar([FromBody] VerificarCodigoDto dto)
        {
            try
            {
                bool confirmado = _servico.ConfirmarCodigo(dto.Email, dto.Codigo);
                if (!confirmado)
                    return BadRequest(new { mensagem = "Código inválido ou expirado." });

                return Ok(new { mensagem = "E-mail confirmado com sucesso." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { mensagem = "Erro inesperado. Tente novamente." });
            }
        }

    }
}
