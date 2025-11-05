using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace PgxAPI.DataBaseTier
{
    public class PerfilUsuarioDB
    {
        private readonly IConfiguration _config;

        public PerfilUsuarioDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDTO<PerfilUsuarioDTO> UsuarioPerfilConsultar(string usuario)
        {
            var response = new ErrorDTO<PerfilUsuarioDTO>();

            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var values = new
                    {
                        Usuario = usuario
                    };

                    response.Result = connection.QueryFirstOrDefault<PerfilUsuarioDTO>("spSEG_W_Logon_Info", values, commandType: CommandType.StoredProcedure);

                    if (response.Result == null)
                    {
                        response.Code = -1;
                        response.Description = "Usuario no encontrado";
                        return response;
                    }
                    else
                    {

                        var claims = new List<Claim>
                        {
                            new Claim(JwtRegisteredClaimNames.Sub, _config["JwtConfig:Subject"]!),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                            new Claim("UserId", response.Result.UserId.ToString()),
                            new Claim(JwtRegisteredClaimNames.UniqueName, response.Result.Nombre)
                        };

                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtConfig:Key"]!));
                        var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                        var token = new JwtSecurityToken(
                            _config["JwtConfig:Issuer"],
                            _config["JwtConfig:Audience"],
                            claims,
                            expires: DateTime.UtcNow.AddMinutes(int.Parse(_config["JwtConfig:ExpirationInMinutes"]!)), // Expira en una hora desde el tiempo actual
                            signingCredentials: signIn
                        );

                        response.Result.token = new JwtSecurityTokenHandler().WriteToken(token);
                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        public ErrorDTO PerfilUsuario_Actualizar(PerfilUsuarioDTO request)
        {
            ErrorDTO resp = new ErrorDTO();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
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