using System.Security.Claims;

namespace WebApplicationCarbono.Helpers
{
    public static class UserHelper
    {
        public static int ObterIdUsuarioLogado(HttpContext httpContext)
        {
            var claim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null)
                throw new UnauthorizedAccessException("Usuário não autenticado.");

            return int.Parse(claim.Value);
        }
    }
}