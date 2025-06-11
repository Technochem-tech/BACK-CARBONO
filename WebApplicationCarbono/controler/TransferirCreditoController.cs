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
    public class TransferirCreditoController : ControllerBase
    {

        private readonly ITransferirCredito _serviço;

        public TransferirCreditoController(ITransferirCredito serviço)
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

                return NotFound(new { mensagem = ex.Message });
            }

        }

        [Authorize]
        [HttpPost("confirmarTransferenciaCredito")]
        public IActionResult ConfirmarTransferenciaSaldo([FromBody] TransferenciaCreditoDto dto)
        {
            try
            {
   
                var remetenteId = Helpers.UserHelper.ObterIdUsuarioLogado(HttpContext);

                var transferencia = new TransferenciaModelo
                {
                    RemetenteId = remetenteId,
                    DestinatarioEmailOuCnpj = dto.DestinatarioEmailOuCnpj,
                    QuantidadeCredito = dto.QuantidadeCredito,
                    Descricao = dto.Descricao

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
