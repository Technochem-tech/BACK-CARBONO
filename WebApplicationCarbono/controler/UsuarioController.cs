using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplicationCarbono.Dtos;
using WebApplicationCarbono.Interface;
using WebApplicationCarbono.Modelos;
using WebApplicationCarbono.Serviços;

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
        [HttpGet("ConsultarUsuario{id}")]
        public ActionResult<BuscarUsuarioModelo> GetUsuario(int id)
        {
            var usuario = _usuarioServiços.GetUsuario(id);

            if (usuario == null)
            {
                return NotFound("Usuário não encontrado.");
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
        public IActionResult Editar([FromQuery] int id, [FromBody] EditarTelefoneUsuarioDto dto)
        {
            try
            {
                _usuarioServiços.EditarTelefone(id, dto);
                return Ok(new { mensagem = "Telefone atualizado com sucesso!" });
            }
            catch (Exception ex)
            {

                return BadRequest(new { mensagem = ex.Message });
            }
        }




        [Authorize]
        [HttpGet("Buscar-imagem/{idUsuario}")]
        public IActionResult ObterImagem( int idUsuario)
        {
            try
            {
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
        public IActionResult UpsertImagem(int idUsuario, [FromForm] AtualizarImgUsuarioDto dto)
        {
            try
            {
                _usuarioServiços.SalvarOuAtualizarImagem(idUsuario, dto.Imagem);
                return Ok(new { mensagem = "Imagem Atualizada sucesso!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete("Deletar-imagem/{idUsuario}")]
        public IActionResult DeletarImagem(int idUsuario)
        {
            try
            {
                _usuarioServiços.DeletarImagemUsuario(idUsuario);
                return Ok(new { mensagem = "imagem deletada com sucesso!" });
            }
            catch (Exception ex)
            {

                return BadRequest(new { mensagem = ex.Message });
            }

        }
        


    }
}

