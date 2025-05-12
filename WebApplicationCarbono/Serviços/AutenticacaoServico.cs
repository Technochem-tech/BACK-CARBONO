using Npgsql;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WebApplicationCarbono.Dtos;
using WebApplicationCarbono.Interface;
using Microsoft.Extensions.Configuration;
using BCrypt.Net;

namespace WebApplicationCarbono.Serviços
{
    public class AutenticacaoServico : IAutenticacao
    {
        private readonly string _stringConexao;
        private readonly JwtSettings _jwtConfig;

        public AutenticacaoServico(IConfiguration configuracao, IOptions<JwtSettings> jwtOptions)
        {
            _stringConexao = configuracao.GetConnectionString("DefaultConnection");
            _jwtConfig = jwtOptions.Value;
        }

        public string Logar(LoginUsuarioDto loginDto)
        {
            int idUsuario = 0;
            string senhaHashBanco = string.Empty;

            using (var conexao = new NpgsqlConnection(_stringConexao))
            {
                conexao.Open();
                var query = "SELECT id, senha FROM usuarios WHERE email = @Email";
                using (var comando = new NpgsqlCommand(query, conexao))
                {
                    comando.Parameters.AddWithValue("@Email", loginDto.Email);

                    using (var leitor = comando.ExecuteReader())
                    {
                        if (leitor.Read())
                        {
                            senhaHashBanco = leitor.GetString(leitor.GetOrdinal("senha"));
                            idUsuario = leitor.GetInt32(leitor.GetOrdinal("id"));
                        }
                        else
                        {
                            throw new Exception("Usuário não encontrado.");
                        }
                    }
                }
            }

            // Verifica a senha usando o mesmo algoritmo BCrypt
            if (!BCrypt.Net.BCrypt.Verify(loginDto.Senha, senhaHashBanco))
            {
                throw new Exception("Senha inválida.");
            }

            // Geração do token JWT
            var manipuladorToken = new JwtSecurityTokenHandler();
            var chave = Encoding.ASCII.GetBytes(_jwtConfig.SecretKey);
            var descricaoToken = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, idUsuario.ToString()),
                    new Claim(ClaimTypes.Email, loginDto.Email)
                }),
                Expires = DateTime.UtcNow.AddMinutes(30),
                Issuer = _jwtConfig.Issuer,
                Audience = _jwtConfig.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(chave), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = manipuladorToken.CreateToken(descricaoToken);
            return manipuladorToken.WriteToken(token);
        }
    }
}
