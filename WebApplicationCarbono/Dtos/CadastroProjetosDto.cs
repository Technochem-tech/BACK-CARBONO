using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;

namespace WebApplicationCarbono.Dtos
{
    public class CadastroProjetosDto
    {
        [Required]
        public string titulo { get; set; } = null!;
        [Required]
        public string descriçao { get; set; } = null!;
        [Required]
        public decimal valor { get; set; }
        [Required]
        public IFormFile img_projetos { get; set; } = null!;
        [Required]
        public Decimal creditosDisponivel{ get; set; }
        
    }
}
