public class EditarProjetoDto
{
    public string? titulo { get; set; }
    public string? descriçao { get; set; }
    public decimal? valor { get; set; }
    public decimal? creditosDisponivel { get; set; }

    public IFormFile? img_projetos { get; set; }  // Vem do form
    public byte[]? imagemBytes { get; set; }      // Vai pro banco
}

