using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class PerfilUsuarioDB
    {
        private readonly IConfiguration _config;

        public PerfilUsuarioDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<PerfilUsuarioDto> UsuarioPerfilConsultar(string usuario, bool propagarError = false)
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

                response.Code = 0;
                response.Description = "Ok";
                return response;
            }
            catch
            {
                if (propagarError)
                {
                    throw;
                }

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

                var resultCode = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure)
                                           .FirstOrDefault();
                resp.Code = resultCode < 0 ? resultCode : 0;
                resp.Description = resultCode < 0 ? $"El procedimiento rechazó la actualización (código {resultCode})." : "OK";
                return resp;
            }
            catch
            {
                resp.Code = -1;
                resp.Description = "Error interno";
                return resp;
            }
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
