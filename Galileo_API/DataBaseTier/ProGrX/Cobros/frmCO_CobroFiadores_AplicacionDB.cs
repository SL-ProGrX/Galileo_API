using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Cobros;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCoCobroFiadoresAplicacionDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;
        private readonly int vModulo = 4;

        public FrmCoCobroFiadoresAplicacionDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Inserta en bitácora un movimiento del módulo.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return _securityMainDb.Bitacora(data);
        }
        /// <summary>
        /// Obtiene el total de casos pendientes de acreditar a deudores.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<CoCobroFiadoresAplicacionPendientesDto> CO_CobroFiadores_Aplicacion_Pendientes_Obtener(int CodEmpresa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            var response = new ErrorDto<CoCobroFiadoresAplicacionPendientesDto>
            {
                Code = 0,
                Description = "Ok",
                Result = new CoCobroFiadoresAplicacionPendientesDto()
            };

            response.Result ??= new CoCobroFiadoresAplicacionPendientesDto();

            try
            {
                const string sp = "spCBR_Cobro_Fiador_Aplica_Abonos";

                var pendientes = conn.QueryFirstOrDefault<int>(
                    sp,
                    new
                    {
                        Top = 0,
                        Usuario = string.Empty,
                        Paso = 0
                    },
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 5200);

                response.Result.pendientes = pendientes;
                return response;
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CoCobroFiadoresAplicacionPendientesDto>(ex.Message);
            }
        }

        /// <summary>
        /// Ejecuta el proceso masivo de aplicación de abonos desde Cobro a Fiadores.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto<CoCobroFiadoresAplicacionProcesarResponse> CO_CobroFiadores_Aplicacion_Procesar(int CodEmpresa,CoCobroFiadoresAplicacionProcesarRequest data)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            var response = new ErrorDto<CoCobroFiadoresAplicacionProcesarResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new CoCobroFiadoresAplicacionProcesarResponse()
            };

            response.Result ??= new CoCobroFiadoresAplicacionProcesarResponse();

            try
            {
                var usuario = (data.usuario_sesion ?? string.Empty).Trim().ToUpperInvariant();

                if (string.IsNullOrWhiteSpace(usuario))
                    return DbHelper.CreateErrorResponse<CoCobroFiadoresAplicacionProcesarResponse>("Usuario requerido.");

                const string sp = "spCBR_Cobro_Fiador_Aplica_Abonos";
                const int topLote = 20;
                const int pasoConsulta = 0;
                const int pasoProcesa = 1;
                const int timeoutSegundos = 5200;
                const int maxIteraciones = 10000;

                var pendientesIniciales = conn.QueryFirstOrDefault<int>(
                    sp,
                    new
                    {
                        Top = 0,
                        Usuario = string.Empty,
                        Paso = pasoConsulta
                    },
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: timeoutSegundos);

                response.Result.pendientes_iniciales = pendientesIniciales;
                response.Result.pendientes_finales = pendientesIniciales;
                response.Result.iteraciones = 0;
                response.Result.completado = true;
                response.Result.mensaje = "Sin casos pendientes.";

                if (pendientesIniciales <= 0)
                {
                    return response;
                }

                var pendientesActuales = pendientesIniciales;
                var iteraciones = 0;

                while (pendientesActuales > 0)
                {
                    iteraciones++;
                    if (iteraciones > maxIteraciones)
                    {
                        return DbHelper.CreateErrorResponse<CoCobroFiadoresAplicacionProcesarResponse>(
                            "El proceso excedió el máximo de iteraciones permitido.");
                    }

                    var pendientesPrevios = pendientesActuales;

                    pendientesActuales = conn.QueryFirstOrDefault<int>(
                        sp,
                        new
                        {
                            Top = topLote,
                            Usuario = usuario,
                            Paso = pasoProcesa
                        },
                        commandType: CommandType.StoredProcedure,
                        commandTimeout: timeoutSegundos);

                    if (pendientesActuales < 0)
                    {
                        return DbHelper.CreateErrorResponse<CoCobroFiadoresAplicacionProcesarResponse>(
                            "El procedimiento devolvió un conteo inválido de pendientes.");
                    }

                    if (pendientesActuales >= pendientesPrevios)
                    {
                        return DbHelper.CreateErrorResponse<CoCobroFiadoresAplicacionProcesarResponse>(
                            "El proceso no mostró avance en la aplicación de abonos.");
                    }
                }

                response.Result.pendientes_finales = pendientesActuales;
                response.Result.iteraciones = iteraciones;
                response.Result.completado = true;
                response.Result.mensaje = "Proceso de aplicación de Abonos desde cobro a Fiadores realizado satisfactoriamente!";

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"CobroFiadores Aplicación Abonos: Inicial={pendientesIniciales}, Final={pendientesActuales}, Iteraciones={iteraciones}",
                    Movimiento = "PROCESA-WEB",
                    Modulo = vModulo
                });

                return response;
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CoCobroFiadoresAplicacionProcesarResponse>(ex.Message);
            }
        }
        /// <summary>
        /// Ejecuta la cancelación masiva de cobro a fiadores para casos normalizados.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto CO_CobroFiadores_Aplicacion_Cancelar(int CodEmpresa,CoCobroFiadoresAplicacionCancelarRequest data)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var usuario = (data.usuario_sesion ?? string.Empty).Trim().ToUpperInvariant();

                if (string.IsNullOrWhiteSpace(usuario))
                    return DbHelper.ErrorResponse("Usuario requerido.");

                conn.Execute(
                    "spCbr_Cobro_Fiadores_Cancela_Masivo",
                    new { Usuario = usuario },
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 5200);

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = "CobroFiadores Cancelación Masiva: cancelación automática por normalización de pago.",
                    Movimiento = "PROCESA-WEB",
                    Modulo = vModulo
                });

                return DbHelper.OkResponse("El proceso se ejecutó satisfactoriamente!");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
    }
}