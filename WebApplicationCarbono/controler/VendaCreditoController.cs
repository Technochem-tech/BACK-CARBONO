using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApplicationCarbono.Dtos;
using WebApplicationCarbono.Interface;

[ApiController]
[Route("api/[controller]")]
public class VendaCreditoController : ControllerBase
{
    private readonly IVendaCredito _vendaCreditoServico;

    public VendaCreditoController(IVendaCredito vendaCreditoServico)
    {
        _vendaCreditoServico = vendaCreditoServico;
    }

    [HttpPost("vender")]
    [Authorize] // Garante que só usuários autenticados possam vender
    public async Task<IActionResult> VenderCreditos([FromBody] VendaCreditoDto dto)
    {
        // Obtém o ID do usuário logado a partir do JWT
        var idUsuarioClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(idUsuarioClaim) || !int.TryParse(idUsuarioClaim, out int idUsuario))
            return Unauthorized("Usuário não autenticado.");

        var resultado = await _vendaCreditoServico.RealizarVendaAsync(idUsuario, dto.QuantidadeCreditos);
        return Ok(new { mensagem = resultado });
    }
}
