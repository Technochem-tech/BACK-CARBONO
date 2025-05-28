using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplicationCarbono.Dtos;
using WebApplicationCarbono.Interface;
using WebApplicationCarbono.Serviços;

namespace WebApplicationCarbono.controler
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProjetosController : ControllerBase
    {
        private readonly IProjetos _projetoServiços;

        public ProjetosController (IProjetos ProjetosServiço)
        {
            _projetoServiços = ProjetosServiço;
        }

        [HttpPost("CadastrarProjetos")]
        public ActionResult CadastrarProjetos([FromBody] CadastroProjetosDto dto)
        {
            try
            {
                _projetoServiços.CadastrarProjetos(dto);
                return Ok("Projeto cadastrado com sucesso.");
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }

        [HttpPut("EditarProjeto")]
        public IActionResult Editar([FromQuery] int id, [FromBody] EditarProjetoDto dto)
        {
            _projetoServiços.EditarProjeto(id, dto);
            return Ok(new { mensagem = "Projeto atualizado com sucesso!" });
        }

        [HttpDelete("deletarProjeto")]
        public IActionResult Deletar([FromQuery] int id)
        {
            _projetoServiços.DeletarProjeto(id);
            return Ok(new { mensagem = "Projeto deletado com sucesso!" });
        }


        [HttpGet("ListarProjetos")]
        public IActionResult ListarProjetos()
        {
            try
            {
                var projetos = _projetoServiços.ListarProjetos();
                return Ok(new { projetossustentaveis = projetos });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
