using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;
using Galileo.Models;

namespace Galileo.DataBaseTier.ProGrX_Personas
{
    public class FrmAFCrLiquidacionMasivaDB
    {
        private readonly IConfiguration _config;

        public FrmAFCrLiquidacionMasivaDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Consulta liquidaciones masivas pendientes usando el SP y los parámetros del objeto Filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<AfLiquidacionMasiva>> AF_LiquidacionMasiva_Obtener(int CodEmpresa, AfLiquidacionMasivaFiltros Filtro)
        {
            if (Filtro is null)
            {
                return DbHelper.CreateErrorResponse("Los filtros de liquidación masiva son requeridos.", -2, new List<AfLiquidacionMasiva>());
            }

            return EjecutarStoredProcedureList<AfLiquidacionMasiva>(
                CodEmpresa,
                "spAFI_Renuncia_Liquidacion_Pendiente",
                new
                {
                    Inicio = Filtro.Inicio,
                    Corte = Filtro.Corte,
                    Tipo = Filtro.Tipo,
                    Institucion = Filtro.Institucion,
                    Causa = Filtro.Causa,
                    Cedula = Filtro.Cedula ?? string.Empty,
                    Nombre = Filtro.Nombre ?? string.Empty,
                    Ejecutivo = Filtro.Ejecutivo ?? string.Empty,
                    Usuario = Filtro.Usuario ?? string.Empty
                });
        }

        /// <summary>
        /// Consulta las causas de renuncia para dropdown, con variantes según los parámetros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipoApl"></param>
        /// <param name="inicio"></param>
        /// <param name="corte"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_LiquidacionMasiva_Obtener_Causas(int CodEmpresa, string? tipoApl = null, DateTime? inicio = null, DateTime? corte = null)
        {
            string query;
            object? parameters = null;

            if (tipoApl == null && inicio == null && corte == null)
            {
                query = @"SELECT id_Causa AS item, Descripcion AS descripcion
                          FROM causas_renuncias
                          WHERE ACTIVO = 1";
            }
            else if (inicio == null && corte == null)
            {
                query = @"SELECT id_Causa AS item, Descripcion AS descripcion
                          FROM causas_renuncias
                          WHERE ACTIVO = 1
                            AND Tipo_Apl IN ('A', @TipoApl)";
                parameters = new { TipoApl = tipoApl };
            }
            else
            {
                query = @"SELECT id_Causa AS item, Descripcion AS descripcion
                          FROM causas_renuncias
                          WHERE ACTIVO = 1
                            AND id_Causa IN (
                                SELECT ID_CAUSA
                                FROM AFI_CR_RENUNCIAS
                                WHERE registro_Fecha BETWEEN @Inicio AND @Corte
                                  AND Tipo IN ('A', @Tipo)
                                  AND Estado = 'P'
                                  AND LIQ IS NULL
                                GROUP BY ID_CAUSA
                            )";
                parameters = new
                {
                    Inicio = inicio?.Date.ToString("yyyy-MM-dd") + " 00:00:00",
                    Corte = corte?.Date.ToString("yyyy-MM-dd") + " 23:59:59",
                    Tipo = tipoApl ?? "P"
                };
            }

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                query,
                parameters);
        }

        /// <summary>
        /// Consulta las instituciones activas para dropdown.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_LiquidacionMasiva_Obtener_Instituciones(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT cod_Institucion AS item, Descripcion AS descripcion
                  FROM Instituciones
                  WHERE ACTIVA = 1");
        }

        /// <summary>
        /// Ejecuta el proceso de liquidación masiva para una renuncia.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="RenunciaId"></param>
        /// <param name="Usuario"></param>
        /// <param name="S06"></param>
        /// <returns></returns>
        public ErrorDto AF_LiquidacionMasiva(int CodEmpresa, int RenunciaId, string Usuario, short S06 = 1)
        {
            return EjecutarStoredProcedure(
                CodEmpresa,
                "spAFI_Renuncia_Liquidacion_Procesa",
                new
                {
                    RenunciaId,
                    Usuario,
                    S06
                },
                "Error al ejecutar liquidación masiva.");
        }

        // ============================================================
        //  Proceso de liquidación masiva por lotes (reanudable)
        //  Modulo_Formulario_Accion: AF_LiquidacionMasiva_Proceso_*
        //  El encabezado y el detalle permiten mostrar avance y reanudar
        //  si el usuario cierra el navegador. El procesamiento es
        //  SECUENCIAL (una renuncia a la vez) para no arriesgar los
        //  consecutivos/contabilidad del SP spAFI_Renuncia_Liquidacion_Procesa.
        // ============================================================

        private const string SqlProcesoActivo = @"
SELECT TOP(1) PROCESO_ID, ESTADO, TOTAL, PROCESADAS, EXITOSAS, ERRORES, MENSAJE
FROM AFI_CR_LIQ_MASIVA_PROCESO
WHERE COD_EMPRESA = @CodEmpresa AND USUARIO = @Usuario AND ESTADO = 'Procesando'
ORDER BY FECHA_INICIO DESC;";

        private const string SqlProcesoPorId = @"
SELECT PROCESO_ID, ESTADO, TOTAL, PROCESADAS, EXITOSAS, ERRORES, MENSAJE
FROM AFI_CR_LIQ_MASIVA_PROCESO WHERE PROCESO_ID = @ProcesoId;";

        private const string SqlProcesoUsuario = @"
SELECT USUARIO FROM AFI_CR_LIQ_MASIVA_PROCESO WHERE PROCESO_ID = @ProcesoId;";

        private const string SqlInsertProceso = @"
INSERT INTO AFI_CR_LIQ_MASIVA_PROCESO (PROCESO_ID, COD_EMPRESA, USUARIO, S06, ESTADO, TOTAL)
VALUES (@ProcesoId, @CodEmpresa, @Usuario, @S06, 'Procesando', @Total);";

        private const string SqlInsertDetalle = @"
INSERT INTO AFI_CR_LIQ_MASIVA_DETALLE (PROCESO_ID, COD_RENUNCIA, CEDULA, S06, ESTADO)
VALUES (@ProcesoId, @CodRenuncia, @Cedula, @S06, 'Pendiente');";

        private const string SqlPendientesLote = @"
SELECT TOP(@Tamano) DETALLE_ID AS Detalle_Id, COD_RENUNCIA AS Cod_Renuncia, S06
FROM AFI_CR_LIQ_MASIVA_DETALLE
WHERE PROCESO_ID = @ProcesoId AND ESTADO = 'Pendiente'
ORDER BY DETALLE_ID;";

        private const string SqlActualizarDetalle = @"
UPDATE AFI_CR_LIQ_MASIVA_DETALLE
SET ESTADO = @Estado, LIQ = @Liq, MENSAJE = @Mensaje, FECHA_PROCESO = SYSDATETIME()
WHERE DETALLE_ID = @DetalleId;";

        private const string SqlRecalcularProceso = @"
UPDATE p SET
    PROCESADAS = d.Procesadas,
    EXITOSAS   = d.Exitosas,
    ERRORES    = d.Errores,
    ESTADO     = CASE WHEN d.Pendientes = 0 THEN 'Completado' ELSE 'Procesando' END,
    ULTIMA_ACTIVIDAD = SYSDATETIME()
FROM AFI_CR_LIQ_MASIVA_PROCESO p
CROSS APPLY (
    SELECT
        SUM(CASE WHEN ESTADO <> 'Pendiente' THEN 1 ELSE 0 END) AS Procesadas,
        SUM(CASE WHEN ESTADO =  'Procesada' THEN 1 ELSE 0 END) AS Exitosas,
        SUM(CASE WHEN ESTADO =  'Error'     THEN 1 ELSE 0 END) AS Errores,
        SUM(CASE WHEN ESTADO =  'Pendiente' THEN 1 ELSE 0 END) AS Pendientes
    FROM AFI_CR_LIQ_MASIVA_DETALLE WHERE PROCESO_ID = p.PROCESO_ID
) d
WHERE p.PROCESO_ID = @ProcesoId;";

        private const string SqlLiqRenuncia = @"
SELECT LIQ FROM AFI_CR_RENUNCIAS WHERE COD_RENUNCIA = @Renuncia;";

        /// <summary>
        /// Inicia un proceso de liquidación masiva: crea el encabezado y una fila de
        /// detalle 'Pendiente' por cada renuncia seleccionada. Si el usuario ya tiene un
        /// proceso activo (por ejemplo cerró el navegador a medio proceso), devuelve ese
        /// proceso marcándolo como Reanudado, sin volver a insertar el detalle.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="request">Usuario y renuncias seleccionadas.</param>
        /// <returns>Progreso del proceso (nuevo o reanudado).</returns>
        public ErrorDto<AfLiqMasivaProgreso> AF_LiquidacionMasiva_Proceso_Iniciar(
            int CodEmpresa, AfLiqMasivaIniciarRequest request)
        {
            if (request?.Renuncias is not { Count: > 0 })
            {
                return DbHelper.CreateErrorResponse<AfLiqMasivaProgreso>(
                    "Debe seleccionar al menos una renuncia para liquidar.");
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, conn =>
            {
                var activo = conn.QueryFirstOrDefault<AfLiqMasivaProgreso>(
                    SqlProcesoActivo, new { CodEmpresa, request.Usuario });
                if (activo != null)
                {
                    activo.Reanudado = true;
                    return activo;
                }

                var procesoId = Guid.NewGuid();
                conn.Execute(SqlInsertProceso, new
                {
                    ProcesoId = procesoId,
                    CodEmpresa,
                    request.Usuario,
                    S06 = (short)1,
                    Total = request.Renuncias.Count
                });

                foreach (var r in request.Renuncias)
                {
                    conn.Execute(SqlInsertDetalle, new
                    {
                        ProcesoId = procesoId,
                        CodRenuncia = r.Cod_Renuncia,
                        r.Cedula,
                        r.S06
                    });
                }

                return conn.QueryFirstOrDefault<AfLiqMasivaProgreso>(SqlProcesoPorId, new { ProcesoId = procesoId })
                       ?? new AfLiqMasivaProgreso { Proceso_Id = procesoId, Estado = "Procesando", Total = request.Renuncias.Count };
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new AfLiqMasivaProgreso())
                : DbHelper.CreateErrorResponse<AfLiqMasivaProgreso>(
                    result.Description ?? "Error al iniciar la liquidación masiva.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Procesa el siguiente lote de renuncias pendientes del proceso, ejecutando el SP
        /// spAFI_Renuncia_Liquidacion_Procesa una por una (secuencial). Cada renuncia se marca
        /// como 'Procesada' o 'Error' de forma independiente para que un fallo no aborte el lote,
        /// y luego se recalculan los contadores del encabezado. Devuelve el avance acumulado.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="ProcesoId">Identificador del proceso.</param>
        /// <param name="Tamano">Cantidad de renuncias a procesar en esta llamada.</param>
        /// <returns>Progreso acumulado del proceso.</returns>
        public ErrorDto<AfLiqMasivaProgreso> AF_LiquidacionMasiva_Proceso_ProcesarLote(
            int CodEmpresa, Guid ProcesoId, int Tamano)
        {
            int tam = Tamano <= 0 ? 25 : Tamano;

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, conn =>
            {
                var usuario = conn.QueryFirstOrDefault<string>(SqlProcesoUsuario, new { ProcesoId })
                    ?? throw new InvalidOperationException("El proceso de liquidación no existe.");

                var pendientes = conn.Query<AfLiqMasivaDetalleRow>(
                    SqlPendientesLote, new { ProcesoId, Tamano = tam }).ToList();

                foreach (var row in pendientes)
                {
                    ProcesarRenunciaDetalle(conn, usuario, row);
                }

                conn.Execute(SqlRecalcularProceso, new { ProcesoId });

                return conn.QueryFirstOrDefault<AfLiqMasivaProgreso>(SqlProcesoPorId, new { ProcesoId })
                       ?? new AfLiqMasivaProgreso { Proceso_Id = ProcesoId };
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new AfLiqMasivaProgreso())
                : DbHelper.CreateErrorResponse<AfLiqMasivaProgreso>(
                    result.Description ?? "Error al procesar el lote de liquidación.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Ejecuta la liquidación de una renuncia y actualiza su fila de detalle.
        /// Aísla el fallo por renuncia (try/catch) para no interrumpir el resto del lote.
        /// </summary>
        private static void ProcesarRenunciaDetalle(SqlConnection conn, string usuario, AfLiqMasivaDetalleRow row)
        {
            string estado = "Procesada";
            string? mensaje = null;
            int? liq = null;

            try
            {
                conn.Execute(
                    "spAFI_Renuncia_Liquidacion_Procesa",
                    new { RenunciaId = row.Cod_Renuncia, Usuario = usuario, S06 = row.S06 },
                    commandType: System.Data.CommandType.StoredProcedure,
                    commandTimeout: 0);

                liq = conn.QueryFirstOrDefault<int?>(SqlLiqRenuncia, new { Renuncia = row.Cod_Renuncia });
            }
            catch (Exception ex)
            {
                estado = "Error";
                mensaje = ex.Message.Length > 500 ? ex.Message.Substring(0, 500) : ex.Message;
            }

            conn.Execute(SqlActualizarDetalle, new
            {
                DetalleId = row.Detalle_Id,
                Estado = estado,
                Liq = liq,
                Mensaje = mensaje
            });
        }

        /// <summary>
        /// Consulta el avance actual de un proceso (para el polling del Swal en el front).
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="ProcesoId">Identificador del proceso.</param>
        /// <returns>Progreso del proceso.</returns>
        public ErrorDto<AfLiqMasivaProgreso> AF_LiquidacionMasiva_Proceso_Estado_Obtener(int CodEmpresa, Guid ProcesoId)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, conn =>
                conn.QueryFirstOrDefault<AfLiqMasivaProgreso>(SqlProcesoPorId, new { ProcesoId }));

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new AfLiqMasivaProgreso())
                : DbHelper.CreateErrorResponse<AfLiqMasivaProgreso>(
                    result.Description ?? "Error al consultar el estado del proceso.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Busca un proceso activo (ESTADO='Procesando') del usuario, para ofrecer reanudar
        /// cuando vuelve a entrar a la pantalla tras cerrar el navegador. Si no hay, el
        /// Result trae Proceso_Id vacío (Guid.Empty).
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="Usuario">Usuario de la sesión.</param>
        /// <returns>Progreso del proceso activo o vacío.</returns>
        public ErrorDto<AfLiqMasivaProgreso> AF_LiquidacionMasiva_Proceso_Activo_Obtener(int CodEmpresa, string Usuario)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, conn =>
                conn.QueryFirstOrDefault<AfLiqMasivaProgreso>(SqlProcesoActivo, new { CodEmpresa, Usuario }));

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new AfLiqMasivaProgreso())
                : DbHelper.CreateErrorResponse<AfLiqMasivaProgreso>(
                    result.Description ?? "Error al consultar el proceso activo.", result.Code.GetValueOrDefault(-1));
        }

        private ErrorDto<List<T>> EjecutarStoredProcedureList<T>(int codEmpresa, string storedProcedure, object parameters)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), codEmpresa, connection =>
                connection.Query<T>(storedProcedure, parameters, commandType: System.Data.CommandType.StoredProcedure).ToList());

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<T>())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al ejecutar procedimiento almacenado.", result.Code.GetValueOrDefault(-1), new List<T>());
        }

        private ErrorDto EjecutarStoredProcedure(int codEmpresa, string storedProcedure, object parameters, string errorMessage)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), codEmpresa, connection =>
            {
                connection.Execute(storedProcedure, parameters, commandType: System.Data.CommandType.StoredProcedure);
                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? errorMessage, result.Code.GetValueOrDefault(-1));
        }

        private PortalDB CreatePortalDb() => new(_config);
    }
}
