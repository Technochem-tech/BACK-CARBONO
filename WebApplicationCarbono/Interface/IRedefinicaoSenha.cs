using WebApplicationCarbono.Dtos;

namespace WebApplicationCarbono.Interface
{
    public interface IRedefinicaoSenha
    {
        void EnviarEmailRedefinicao(EditarSenhaRequestDto dto);
        bool ValidarToken (string token);
        void AtualizarSenha (string token, string senha);

    }
}
