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
                // 404 para destinatário não encontrado
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

                // Se mensagem começar com "Erro", lançar erro para ser capturado no catch
                if (mensagem.StartsWith("Erro"))
                {
                    if (mensagem.Contains("para si mesmo") ||
                        mensagem.Contains("maior que zero") ||
                        mensagem.Contains("Saldo insuficiente"))
                    {
                        return BadRequest(new { mensagem });
                    }

                    if (mensagem.Contains("Destinatário não encontrado"))
                    {
                        return NotFound(new { mensagem });
                    }

                    // Outros erros tratados como bad request
                    return BadRequest(new { mensagem = "Erro inesperado: " + mensagem });
                }

                return Ok(new { mensagem });
            }
            catch (Exception ex)
            {
                // Erro inesperado retorna BadRequest
                return BadRequest(new { mensagem = "Erro inesperado: " + ex.Message });
            }
        }
    }
}
