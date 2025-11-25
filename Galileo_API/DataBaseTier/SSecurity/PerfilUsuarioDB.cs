using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier
{
    public class PerfilUsuarioDB
    {
        private readonly IConfiguration _config;

        public PerfilUsuarioDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<PerfilUsuarioDto> UsuarioPerfilConsultar(string usuario)
        {
            var response = new ErrorDto<PerfilUsuarioDto>();

            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString"));

                var values = new { Usuario = usuario };
                response.Result = connection.QueryFirstOrDefault<PerfilUsuarioDto>(
                    "spSEG_W_Logon_Info", values, commandType: CommandType.StoredProcedure);

                if (response.Result == null)
                {
                    response.Code = -1;
                    response.Description = "Usuario no encontrado";
                    return response;
                }

                // === MISMOS valores que Program.cs (sección 'Jwt') ===
                var jwt = _config.GetSection("Jwt");
                var issuer   = jwt["Issuer"];
                var audience = jwt["Audience"];
                var secret   = jwt["Secret"];
                var minutes  = int.TryParse(jwt["AccessTokenMinutes"], out var m) ? m : 60;

                if (string.IsNullOrWhiteSpace(issuer) ||
                    string.IsNullOrWhiteSpace(audience) ||
                    string.IsNullOrWhiteSpace(secret))
                {
                    response.Code = -1;
                    response.Description = "Configuración JWT incompleta (Jwt:Issuer/Audience/Secret).";
                    return response;
                }

                var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                // Claims alineados con Program.cs
                var claims = new List<Claim>
                {
                    // Identidad principal
                    new Claim(JwtRegisteredClaimNames.Sub, response.Result.UserId?.ToString() ?? string.Empty),
                    new Claim(ClaimTypes.NameIdentifier, response.Result.UserId?.ToString() ?? string.Empty),
                    new Claim(ClaimTypes.Name, response.Result.Usuario ?? usuario),

                    // Metadatos estándar
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat,
                              DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                              ClaimValueTypes.Integer64),

                    // Opcionales
                    // new Claim(ClaimTypes.Email, response.Result.Email ?? string.Empty),
                    // new Claim(ClaimTypes.Role, response.Result.Rol ?? "User"),
                };

                var token = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claims,
                    notBefore: DateTime.UtcNow,
                    expires: DateTime.UtcNow.AddMinutes(minutes),
                    signingCredentials: creds
                );

                response.Result.token = new JwtSecurityTokenHandler().WriteToken(token);
                response.Code = 1;
                response.Description = "Ok";
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        public ErrorDto PerfilUsuario_Actualizar(PerfilUsuarioDto request)
        {
            var resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString"));
                var procedure = "[spSEG_W_PerfilUsuario_Actualizar]";
                var values = new
                {
                    USERID = request.UserId,
                    USUARIO = request.Usuario,
                    NOMBRE = request.Nombre,
                    TEL_CELL = request.Tel_Cell,
                    TEL_TRABAJO = request.Tel_Trabajo,
                    EMAIL = request.Email,
                };

                resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                resp.Description = "Ok";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }
    }
}