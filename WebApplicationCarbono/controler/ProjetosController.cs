using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
