namespace WebApplicationCarbono.Interface
{
    public interface IVerificacaoEmail
    {
        void EnviarCodigoVerificacao(string email);
        bool ConfirmarCodigo(string email, string codigo);
        bool EstaConfirmado(string email);
    }
}
