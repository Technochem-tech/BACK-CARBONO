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
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(new { mensagem = "E-mail não pode estar vazio." });

            try
            {
                _servico.EnviarCodigoVerificacao(email);
                return Ok(new { mensagem = "Código de verificação enviado para o e-mail informado." });
            }
            catch (ArgumentException ex)
            {
                // erro esperado (e-mail inválido, domínio inexistente, etc)
                return BadRequest(new { mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                // erro inesperado (falha de SMTP, etc)
                return StatusCode(500, new { mensagem = "Erro ao enviar e-mail.", detalhe = ex.Message });
            }
        }

        [HttpPost("confirmar")]
        public IActionResult Confirmar([FromBody] VerificarCodigoDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Codigo))
                return BadRequest(new { mensagem = "E-mail e código são obrigatórios." });

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
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro ao confirmar código.", detalhe = ex.Message });
            }
        }
    }
}
