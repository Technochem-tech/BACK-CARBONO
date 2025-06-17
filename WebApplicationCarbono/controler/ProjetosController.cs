using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplicationCarbono.Dtos;
using WebApplicationCarbono.Interface;
using WebApplicationCarbono.Serviços;

namespace WebApplicationCarbono.controler
{
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
        public async Task<ActionResult> CadastrarProjetos([FromForm] CadastroProjetosDto dto)
        {
            try
            {
                // Lê a imagem enviada e converte para byte[]
                byte[] bytesImagem;

                using (var ms = new MemoryStream())
                {
                    await dto.img_projetos.CopyToAsync(ms);
                    bytesImagem = ms.ToArray();
                }

                _projetoServiços.CadastrarProjetos(dto, bytesImagem);

                return Ok("Projeto cadastrado com sucesso.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("EditarProjeto")]
        public async Task<IActionResult> Editar([FromQuery] int id, [FromForm] EditarProjetoDto dto)
        {
            try
            {
                // Lê a imagem, se tiver sido enviada
                if (dto.img_projetos != null)
                {
                    using var ms = new MemoryStream();
                    await dto.img_projetos.CopyToAsync(ms);
                    dto.imagemBytes = ms.ToArray();
                }

                // Chama o serviço que já verifica o que veio preenchido
                _projetoServiços.EditarProjeto(id, dto);

                return Ok(new { mensagem = "Projeto atualizado com sucesso!" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
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
