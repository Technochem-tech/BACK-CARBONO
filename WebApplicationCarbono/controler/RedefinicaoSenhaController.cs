
using Microsoft.AspNetCore.Mvc;
using WebApplicationCarbono.Interface;
using WebApplicationCarbono.Dtos;

namespace WebApplicationCarbono.controler
{
    [Route("api/[controller]")]
    [ApiController]
    public class RedefinicaoSenhaController : ControllerBase
    {
        private readonly IRedefinicaoSenha _servicoRedefinicao;

        public RedefinicaoSenhaController (IRedefinicaoSenha servicoRedefinicao)
        {
            _servicoRedefinicao = servicoRedefinicao;
        }

        [HttpPost("solicitar")]
        public IActionResult SolicitarRedefinicao([FromBody] ResetSenhaRequestDto dto)
        {
            if (string.IsNullOrEmpty(dto.Email))
                return BadRequest(new { mensagem = "Email é obrigatório" });

            try
            {
                _servicoRedefinicao.EnviarEmailRedefinicao(dto);
                return Ok(new { mensagem = "Email enviado com instruções para redefinir a senha." });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpPost("validar-token")]
        public IActionResult ValidarToken([FromQuery] string token)
        {
            bool valido = _servicoRedefinicao.ValidarToken(token);
            if (!valido)
                return BadRequest(new { mensagem = "Token inválido ou expirado." });

            return Ok(new { mensagem = "Token válido." });
        }

        [HttpPost("atualizar-senha")]
        public IActionResult AtualizarSenha([FromQuery] string token, [FromBody] string novaSenha)
        {
            if (string.IsNullOrEmpty(novaSenha))
                return BadRequest(new { mensagem = "Nova senha é obrigatória." });

            try
            {
                _servicoRedefinicao.AtualizarSenha(token, novaSenha);
                return Ok(new { mensagem = "Senha atualizada com sucesso." });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }
    }
}
    

