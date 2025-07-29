using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplicationCarbono.Dtos;
using WebApplicationCarbono.Interface;
using WebApplicationCarbono.Modelos;
using WebApplicationCarbono.Serviços;
using WebApplicationCarbono.Helpers;    

namespace WebApplicationCarbono.controler
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuario _usuarioServiços;

        public UsuarioController(IUsuario UsuarioServiço)
        {
            _usuarioServiços = UsuarioServiço;
        }

        [Authorize]
        [HttpGet("ConsultarUsuario")]
        public ActionResult<BuscarUsuarioModelo> GetUsuario()
        {
            var idUsuario = UserHelper.ObterIdUsuarioLogado(HttpContext);
            if (idUsuario <= 0)
            {
                return Unauthorized(new { mensagem = "Usuário não autenticado." });
            }

        
            var usuario = _usuarioServiços.GetUsuario(idUsuario);
            if (usuario == null)
            {
                return NotFound(new { mensagem = "Usuário não encontrado." });
            }
            return Ok(usuario);
        }


        [HttpPost("Cadastrar")]
        public IActionResult Cadastrar([FromBody] CadastroUsuarioDto cadastroUsuarioDto)

        {
            try
            {
                _usuarioServiços.CadastrarUsuario(cadastroUsuarioDto); 
                return Ok("Usuário cadastrado com sucesso.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [Authorize]
        [HttpPut("EditarTelefone")]
        public IActionResult Editar( [FromBody] EditarTelefoneUsuarioDto dto)
        {
            try
            {   // o metódo (ObterIdUsuarioLogado) obtém o ID do usuário pelo o tokem Jtw e, insere automaticamente ao IdUsuario junto ao dto
                var idUsuario = UserHelper.ObterIdUsuarioLogado(HttpContext);
                if (idUsuario <= 0)
                {
                    return Unauthorized(new { mensagem = "Usuário não autenticado." });
                }

                _usuarioServiços.EditarTelefone(idUsuario, dto);
                return Ok(new { mensagem = "Telefone atualizado com sucesso!" });
            }
            catch (Exception ex)
            {

                return BadRequest(new { mensagem = ex.Message });
            }
        }




        [Authorize]
        [HttpGet("Buscar-imagem")]
        public IActionResult ObterImagem()
        {
            try
            {   // o metódo (ObterIdUsuarioLogado) obtém o ID do usuário pelo o tokem Jtw e, insere automaticamente ao IdUsuario
                var idUsuario = Helpers.UserHelper.ObterIdUsuarioLogado(HttpContext);
                if (idUsuario <= 0)
                {
                    return Unauthorized(new { mensagem = "Usuário não autenticado." });
                }

                var imagemBytes = _usuarioServiços.BuscarImagemUsuario(idUsuario);
                return File(imagemBytes, "image/jpeg");
            }
            catch (Exception ex)
            {

                return BadRequest(new { mensagem = ex.Message });
            }
            

        }



        [Authorize]
        [HttpPut("SalvarOuAtualizarImagem")]
        public IActionResult UpsertImagem([FromForm] AtualizarImgUsuarioDto dto)
        {
            try
            {   // o metódo (ObterIdUsuarioLogado) obtém o ID do usuário pelo o tokem Jtw e, insere automaticamente ao IdUsuario.
                var idUsuario = Helpers.UserHelper.ObterIdUsuarioLogado(HttpContext);
                if (idUsuario <= 0)
                {
                    return Unauthorized(new { mensagem = "Usuário não autenticado." });
                }

                _usuarioServiços.SalvarOuAtualizarImagem(idUsuario, dto.Imagem);
                return Ok(new { mensagem = "Imagem Atualizada sucesso!" });

            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete("Deletar-imagem")]
        public IActionResult DeletarImagem()
        {
            try
            {   // o metódo (ObterIdUsuarioLogado) obtém o ID do usuário pelo o tokem Jtw e, insere automaticamente ao IdUsuario. 
                var IdUsuario = Helpers.UserHelper.ObterIdUsuarioLogado(HttpContext);
                if (IdUsuario <= 0)
                {
                    return Unauthorized(new { mensagem = "Usuário não autenticado." });
                }

                _usuarioServiços.DeletarImagemUsuario(IdUsuario);
                return Ok(new { mensagem = "imagem deletada com sucesso!" });
            }
            catch (Exception ex)
            {

                return BadRequest(new { mensagem = ex.Message });
            }

        }
        


    }
}

