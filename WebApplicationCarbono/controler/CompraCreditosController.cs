using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplicationCarbono.Dtos;
using WebApplicationCarbono.Helpers;
using WebApplicationCarbono.Interface;
using WebApplicationCarbono.Modelos;

namespace WebApplicationCarbono.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompraCreditosController : ControllerBase
    {
        private readonly ICompraCreditos _servico;

        public CompraCreditosController(ICompraCreditos servico)
        {
            _servico = servico;
        }

        [HttpPost("iniciar-compra")]
        [Authorize]
        public IActionResult IniciarCompra([FromBody] CompraCreditosDto dto)
        {
            try
            {
                var idUsuario = UserHelper.ObterIdUsuarioLogado(HttpContext);
                var emailUsuario = UserHelper.ObterEmailUsuarioLogado(HttpContext);

                var compra = new ComprarCredito
                {
                    idUsuario = idUsuario,
                    emailUsuario = emailUsuario,
                    quantidadeCredito = dto.quantidadeCredito
                };

                var resultado = _servico.IniciarCompraCredito(compra);

                return Ok(new
                {
                    qrCode = resultado.qrCode,
                    pagamentoId = resultado.pagamentoId
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = "Erro ao iniciar a compra: " + ex.Message });
            }
        }

        [HttpPost("webhook/mercadopago")]
        [AllowAnonymous]
        public async Task<IActionResult> WebhookMercadoPago([FromBody] MercadoPagoNotification notificacao)
        {
            try
            {

                var resultado = await _servico.ConfirmarCompraWebhookAsync(notificacao);
                return Ok(new { mensagem = resultado });

            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = "Erro no webhook: " + ex.Message });
            }
        }


    }


}
