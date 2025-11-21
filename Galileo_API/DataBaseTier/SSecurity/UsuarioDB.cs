using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class UsuarioDB
    {

        private readonly IConfiguration _config;
        private const string connectionStringName = "DefaultConnString";

        public UsuarioDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto UsuarioCuentaRevisar(UsuarioCuentaRevisarDto cuentaUsuarioRevisarDto)
        {
            ErrorDto resultado = new ErrorDto();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_W_Cuenta_Revisar]";
                    var datosCtaRevisar = new
                    {
                        Usuario = cuentaUsuarioRevisarDto.Usuario,
                        Bloqueo = cuentaUsuarioRevisarDto.Bloqueo ? 1 : 0,
                        BloqueoIndef = cuentaUsuarioRevisarDto.BloqueoI ? 1 : 0,
                        Admin = cuentaUsuarioRevisarDto.Admin ? 1 : 0,
                        CuentaCaduca = cuentaUsuarioRevisarDto.CuentaCaduca ? 1 : 0,
                        CambioLogon = cuentaUsuarioRevisarDto.CambioLogon ? 1 : 0,

                        // CambioContrasena = cuentaUsuarioRevisarDto.CambioContrasena,
                        Notas = cuentaUsuarioRevisarDto.Notas,
                        AppName = "SSECURITY- WEB",
                        AppVersion = cuentaUsuarioRevisarDto.AppVersion,
                        UsuarioMovimiento = cuentaUsuarioRevisarDto.UsuarioMovimiento,
                        Maquina = cuentaUsuarioRevisarDto.Maquina,
                    };

                    resultado.Code = connection.Execute(procedure, datosCtaRevisar, commandType: CommandType.StoredProcedure);
                    resultado.Description = "Ok";
                }
            }
            catch (Exception ex)
            {
                resultado.Code = -1;
                resultado.Description = ex.Message;
            }
            return resultado;
        }

        public UsuarioCuentaRevisarDto UsuarioCuentaObtener(string nombreUsuario)
        {
            UsuarioCuentaRevisarDto result = null!;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_Cuenta_Obtener]";
                    var values = new
                    {
                        NombreUsuario = nombreUsuario,
                    };
                    result = connection.QueryFirstOrDefault<UsuarioCuentaRevisarDto>(procedure, values, commandType: CommandType.StoredProcedure)!;
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return result;
        }

        public List<UsuarioCuentaMovimientoResultDto> UsuarioCuentaMovimientosObtener(UsuarioCuentaMovimientoRequestDto usuarioCuentaMovimientoRequestDto)
        {
            List<UsuarioCuentaMovimientoResultDto> resultado = new List<UsuarioCuentaMovimientoResultDto>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_Cuenta_Log_Movimientos_Obtener]";
                    var values = new
                    {
                        Usuario = usuarioCuentaMovimientoRequestDto.Usuario.Trim(),
                        FechaInicio = usuarioCuentaMovimientoRequestDto.FechaInicio,
                        FechaCorte = usuarioCuentaMovimientoRequestDto.FechaCorte,
                        Estacion = usuarioCuentaMovimientoRequestDto.Estacion != null ? usuarioCuentaMovimientoRequestDto.Estacion.Trim() : string.Empty,
                        ListaCodTransacciones = usuarioCuentaMovimientoRequestDto.ListaCodTransacciones.Trim(),
                        AppName = usuarioCuentaMovimientoRequestDto.AppName != null ? usuarioCuentaMovimientoRequestDto.AppName.Trim() : string.Empty,
                        AppVersion = usuarioCuentaMovimientoRequestDto.AppVersion,
                        UsuarioBusqueda = usuarioCuentaMovimientoRequestDto.UsuarioBusqueda,
                        Revision = usuarioCuentaMovimientoRequestDto.Revision.Trim(),
                        RevisionInd = usuarioCuentaMovimientoRequestDto.RevisionInd
                    };
                    resultado = connection.Query<UsuarioCuentaMovimientoResultDto>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resultado;
        }

        public List<LoginDbResult> ObtenerInformacionUsuario(string username, string userId)
        {
            List<LoginDbResult> resp = [];
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spGA_ObtenerUsuario]";
                    var values = new
                    {
                        UserName = username,
                        UserId = userId
                    };
                    resp = connection.Query<LoginDbResult>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

    }

}//end namespace
