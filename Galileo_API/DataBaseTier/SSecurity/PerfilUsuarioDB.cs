using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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

            if (string.IsNullOrWhiteSpace(usuario))
            {
                response.Code = -1;
                response.Description = "Usuario requerido.";
                return response;
            }

            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString"));

                var values = new { Usuario = usuario.Trim() };

                response.Result = connection.QueryFirstOrDefault<PerfilUsuarioDto>(
                    "spSEG_W_Logon_Info",
                    values,
                    commandType: CommandType.StoredProcedure
                );

                if (response.Result == null)
                {
                    response.Code = -1;
                    response.Description = "Usuario no encontrado";
                    return response;
                }

                // Asegurar UserId válido
                if (response.Result.UserId <= 0)
                {
                    response.Code = -1;
                    response.Description = "Usuario inválido (sin UserId).";
                    return response;
                }

                // Leer configuración JWT
                var jwtSection = _config.GetSection("Jwt");
                var issuer = jwtSection["Issuer"];
                var audience = jwtSection["Audience"];
                var secret = jwtSection["Secret"];
                var minutes = int.TryParse(jwtSection["AccessTokenMinutes"], out var m) ? m : 60;

                if (string.IsNullOrWhiteSpace(issuer) ||
                    string.IsNullOrWhiteSpace(audience) ||
                    string.IsNullOrWhiteSpace(secret))
                {
                    response.Code = -1;
                    response.Description = "Configuración JWT incompleta (Jwt:Issuer/Audience/Secret).";
                    return response;
                }

                if (!response.Result.UserId.HasValue || response.Result.UserId.Value <= 0)
                {
                    response.Code = -1;
                    response.Description = "Usuario inválido (sin UserId).";
                    return response;
                }

                // Generar token
                response.Result.token = GenerateJwt(
                    userId: response.Result.UserId.Value,
                    username: response.Result.Usuario ?? usuario,
                    issuer: issuer,
                    audience: audience,
                    secret: secret,
                    minutes: minutes
                // Si quieres, aquí puedes pasar email/rol para agregarlos como claims
                );

                response.Code = 1;
                response.Description = "Ok";
                return response;
            }
            catch
            {
                // No devuelvas ex.Message en producción
                response.Code = -1;
                response.Description = "Error interno";
                return response;
            }
        }

        public ErrorDto PerfilUsuario_Actualizar(PerfilUsuarioDto request)
        {
            var resp = new ErrorDto();

            if (request == null || request.UserId <= 0)
            {
                resp.Code = -1;
                resp.Description = "Datos inválidos.";
                return resp;
            }

            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString"));

                const string procedure = "spSEG_W_PerfilUsuario_Actualizar";

                var values = new
                {
                    USERID = request.UserId,
                    USUARIO = request.Usuario,
                    NOMBRE = request.Nombre,
                    TEL_CELL = request.Tel_Cell,
                    TEL_TRABAJO = request.Tel_Trabajo,
                    EMAIL = request.Email,
                };

                resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure)
                                      .FirstOrDefault();

                resp.Description = "Ok";
                return resp;
            }
            catch
            {
                resp.Code = -1;
                resp.Description = "Error interno";
                return resp;
            }
        }

        private static string GenerateJwt(int userId, string username, string issuer, string audience, string secret, int minutes)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var now = DateTime.UtcNow;

            var claims = new List<Claim>
            {
                // Identidad
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username ?? string.Empty),

                // Metadatos
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: now,
                expires: now.AddMinutes(minutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public bool UsuarioTieneAccesoAEmpresa(int userId, int codEmpresa)
        {
            using var cn = new SqlConnection(_config.GetConnectionString("DefaultConnString"));
            var ok = cn.QueryFirstOrDefault<int>(
                "spSEG_Usuario_TieneAcceso_Empresa",
                new { UserId = userId, CodEmpresa = codEmpresa },
                commandType: CommandType.StoredProcedure
            );
            return ok == 1;
        }
    }
}