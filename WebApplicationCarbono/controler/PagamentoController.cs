using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class PagamentoController : ControllerBase
{
    private readonly IPagamentoService _pagamentoService;

    public PagamentoController(IPagamentoService pagamentoService)
    {
        _pagamentoService = pagamentoService;
    }

    [HttpPost("pix")]
    public async Task<IActionResult> GerarPagamentoPix([FromBody] PixRequestModel request)
    {
        var pagamento = await _pagamentoService.CriarPagamentoPixAsync(request.Valor, request.EmailCliente);

        return Ok(new
        {
            Status = pagamento.Status,
            QrCode = pagamento.PointOfInteraction.TransactionData.QrCode,
            QrCodeBase64 = pagamento.PointOfInteraction.TransactionData.QrCodeBase64
        });
    }

    // Rota que retorna a imagem PNG do QR Code diretamente no response
    [HttpPost("pix-imagem")]
    public async Task<IActionResult> GerarPagamentoPixImagem([FromBody] PixRequestModel request)
    {
        var pagamento = await _pagamentoService.CriarPagamentoPixAsync(request.Valor, request.EmailCliente);

        var base64 = pagamento.PointOfInteraction.TransactionData.QrCodeBase64;

        // Converter o Base64 para bytes da imagem
        var bytes = Convert.FromBase64String(base64);

        // Retornar a imagem PNG diretamente no response
        return File(bytes, "image/png");
    }
}

public class PixRequestModel
{
    public decimal Valor { get; set; }
    public string EmailCliente { get; set; }
}
