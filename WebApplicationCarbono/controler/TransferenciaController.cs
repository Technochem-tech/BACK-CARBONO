using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplicationCarbono.Dtos;
using WebApplicationCarbono.Interface;
using WebApplicationCarbono.Modelos;

namespace WebApplicationCarbono.controler
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransferenciaController : ControllerBase
    {

        private readonly ITransferencia _serviço;

        public TransferenciaController(ITransferencia serviço)
        {
            _serviço = serviço;
        }

        [Authorize]
        [HttpGet("verificar-destinatario")]
        public IActionResult VerificarDestinatario([FromQuery] string emailOuCnpj)
        {
            try
            {
                var resultado = _serviço.VerificarDestinatario(emailOuCnpj);
                return Ok(resultado);
            }
            catch (Exception ex)
            {

               return NotFound(new {mensagem = ex.Message});
            }
            
        }

        [Authorize]
        [HttpPost("confirmar")]
        public IActionResult ConfirmarTransferencia([FromBody] TransferenciaDto dto)
        {
            try
            {
                var infoUsuarioToken = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (infoUsuarioToken == null)
                {
                    return Unauthorized(new { erro = "Usuário não autenticado corretamente." });
                }

                var remetenteId = int.Parse(infoUsuarioToken.Value);

                var transferencia = new TransferenciaModelo
                {
                    RemetenteId = remetenteId,
                    DestinatarioEmailOuCnpj = dto.DestinatarioEmailOuCnpj,
                    QuantidadeCredito = dto.QuantidadeCredito

                };

                var mensagem = _serviço.RealizarTransferencia(transferencia);
                return Ok(new { mensagem });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
           
            
        }
    }
}
