using Microsoft.IdentityModel.Tokens;
using Galileo.DataBaseTier;
using Galileo.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Galileo.Models.Security;

namespace Galileo.BusinessLogic
{
    public class AdminBL
    {

        private readonly IConfiguration _config;

        public AdminBL(IConfiguration config)
        {
            _config = config;
        }

        public LoginResult Login(string username, string passw)
        {
            LoginResult resultado = new LoginResult();
            try
            {
                var logindbResultado = new AdminDb(_config).Login(username, passw);

                if (logindbResultado != null && logindbResultado.Any())
                {
                    resultado.UserId = logindbResultado[0].UserId;
                    resultado.UserName = logindbResultado[0].UserName;

                    var jwt = _config.GetSection("Jwt").Get<JwtDto>();

                    if (jwt == null)
                    {
                        throw new InvalidOperationException("JWT configuration is missing or invalid.");
                    }

                    var claims = new List<Claim>
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, jwt.Subject),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64), // Se utiliza ToUnixTimeSeconds para la propiedad Iat
                        new Claim("UserId", resultado.UserId.ToString()),
                        new Claim("UserName", resultado.UserName.ToString()),
                    };

                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key));
                    var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                    var token = new JwtSecurityToken(
                        issuer: jwt.Issuer,
                        audience: jwt.Audience,
                        claims: claims,
                        expires: DateTime.UtcNow.AddMinutes(60), // Expira en una hora desde el tiempo actual
                        signingCredentials: signIn
                    );

                    resultado.Token = new JwtSecurityTokenHandler().WriteToken(token);
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }

            return resultado;

        }
    }
}