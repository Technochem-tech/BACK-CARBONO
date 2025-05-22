using WebApplicationCarbono.Dtos;

namespace WebApplicationCarbono.Interface
{
    public interface IRedefinicaoSenha
    {
        void EnviarEmailRedefinicao(ResetSenhaRequestDto dto);
        bool ValidarToken (string token);
        void AtualizarSenha (string token, string senha);

    }
}
