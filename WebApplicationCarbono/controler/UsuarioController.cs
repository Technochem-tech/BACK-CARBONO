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
        public ActionResult<BuscarUsuario> GetUsuario(int id)
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
                _usuarioServiços.CadastrarUsuario(cadastroUsuarioDto); // nome correto aqui
                return Ok("Usuário cadastrado com sucesso.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}

