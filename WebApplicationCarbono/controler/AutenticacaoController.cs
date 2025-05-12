using Microsoft.AspNetCore.Mvc;
using WebApplicationCarbono.Dtos;
using WebApplicationCarbono.Interface;

namespace WebApplicationCarbono.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AutenticacaoController : ControllerBase
    {
        private readonly IAutenticacao _servicoAutenticacao;

        public AutenticacaoController(IAutenticacao servicoAutenticacao)
        {
            _servicoAutenticacao = servicoAutenticacao;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginUsuarioDto loginDto)
        {
            if (loginDto == null || string.IsNullOrEmpty(loginDto.Email) || string.IsNullOrEmpty(loginDto.Senha))
            {
                return BadRequest(new { mensagem = "Email e senha são obrigatórios." });
            }

            try
            {
                var token = _servicoAutenticacao.Logar(loginDto);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { mensagem = ex.Message });
            }
        }
    }
}
