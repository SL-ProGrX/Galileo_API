using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.Security;
using Galileo.Models.ERROR;
using System.Diagnostics;
using System.Data;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB
{
    public class CcProcesoMensualRecepcionDb
    {
        private const int CommandTimeoutSeconds = 0;
        private readonly PortalDB _portalDb;
        private readonly MCobroDb _mCobroDb;
        private readonly CcProcesoMensualGeneralDb _mGeneral;
        private readonly ILogger<CcProcesoMensualRecepcionDb> _logger;

        /// <summary>
        /// Inicializa una nueva instancia para gestionar la recepción y desglose del proceso mensual.
        /// </summary>
        /// <param name="config">Configuración general de la aplicación.</param>
        /// <param name="logger">Logger estructurado para el seguimiento del proceso.</param>
        public CcProcesoMensualRecepcionDb(
            IConfiguration config,
            ILogger<CcProcesoMensualRecepcionDb> logger)
        {
            _portalDb = new PortalDB(config);
            _mCobroDb = new MCobroDb(config);
            _mGeneral = new CcProcesoMensualGeneralDb(config);
            _logger = logger;
        }

        #region sbCrDesgloce
        /// <summary>
        /// Ejecuta el desglose de créditos para una institución en el proceso indicado.
        /// </summary>
        /// <param name="connection">Conexión activa a base de datos.</param>
        /// <param name="request">Parámetros de solicitud del desglose.</param>
        /// <param name="correlacionId">Identificador generado por el servidor para correlacionar los logs.</param>
        /// <param name="reportarProgreso">Acción que informa total, procesados y mensaje de avance.</param>
        /// <param name="reportarLog">Acción que publica el detalle de las etapas en Galileo.</param>
        /// <returns>Resultado del último lote procesado.</returns>
        public CcProcesoMensualCreditoDesgloseResultadoDbModel CrDesgloce(
            IDbConnection connection,
            CcProcesoMensualDesgloseRequest request,
            Guid correlacionId,
            Action<int, int, string>? reportarProgreso = null,
            Action<string>? reportarLog = null)
        {
            var configuracion = ObtenerConfiguracionCreditoDesglose(connection, request.CodInstitucion);

            var aplicaIncon = configuracion.Aplica;
            var historicoCobroEnvio = configuracion.HistoricoCobroEnvio;
            var fechaSistema = configuracion.FechaServer;

            decimal fechaAnterior = request.FechaProceso;

            RegistrarAvance(
                correlacionId,
                "Configuración de créditos obtenida",
                $"Histórico: {historicoCobroEnvio}; aplica inconsistencias: {aplicaIncon}",
                reportarLog);

            for (var i = 1; i <= historicoCobroEnvio; i++)
            {
                fechaAnterior = _mCobroDb.fxFechaProcesoAnterior(request.CodEmpresa, fechaAnterior);
            }

            EliminarCreditosProceso(connection, request.CodInstitucion, request.FechaProceso, fechaAnterior);

            RegistrarAvance(correlacionId, "Registros anteriores de créditos eliminados", reportarLog: reportarLog);
            reportarProgreso?.Invoke(0, 0, "Detallando los Abonos a créditos...");

            var resultado = EjecutarCreditoDesglose(
                connection,
                 new CcProcesoMensualCreditoDesgloseDbRequest
                 {
                     CodInstitucion = request.CodInstitucion,
                     FechaProceso = request.FechaProceso,
                     FechaSistema = fechaSistema,
                     AplicaIncon = aplicaIncon,
                     PrimeraVez = 1,
                     Cantidad = 50
                 });


             
            var total = resultado.Total;
            var pendientes = total == 0 ? 0 : resultado.Pendientes;
            var numeroLote = 1;

            ReportarProgresoDesglose(resultado, reportarProgreso);

            RegistrarAvance(
                correlacionId,
                "Lote de créditos finalizado",
                $"Lote: {numeroLote}; total: {total}; pendientes: {pendientes}",
                reportarLog);

            while (pendientes > 0)
            {
                numeroLote++;
                resultado = EjecutarCreditoDesglose(
                    connection,
                  new CcProcesoMensualCreditoDesgloseDbRequest
                  {
                      CodInstitucion = request.CodInstitucion,
                      FechaProceso = request.FechaProceso,
                      FechaSistema = fechaSistema,
                      AplicaIncon = aplicaIncon,
                      PrimeraVez = 0,
                      Cantidad = 350
                  });
                 
                total = resultado.Total;
                pendientes = total == 0 ? 0 : resultado.Pendientes;

                ReportarProgresoDesglose(resultado, reportarProgreso);

                RegistrarAvance(
                    correlacionId,
                    "Lote de créditos finalizado",
                    $"Lote: {numeroLote}; total: {total}; pendientes: {pendientes}",
                    reportarLog);
            }

            return resultado;
        }

        /// <summary>
        /// Informa el avance real retornado por el procedimiento de desglose.
        /// </summary>
        /// <param name="resultado">Resultado del lote procesado.</param>
        /// <param name="reportarProgreso">Acción que recibe el avance.</param>
        private static void ReportarProgresoDesglose(
            CcProcesoMensualCreditoDesgloseResultadoDbModel resultado,
            Action<int, int, string>? reportarProgreso)
        {
            reportarProgreso?.Invoke(
                resultado.Total,
                resultado.Procesados,
                "Detallando los Abonos a créditos...");
        }

        /// <summary>
        /// Obtiene la configuración necesaria para ejecutar el desglose de créditos.
        /// </summary>
        /// <param name="connection">Conexión activa a base de datos.</param>
        /// <param name="codInstitucion">Código de la institución.</param>
        /// <returns>Configuración del desglose de créditos.</returns>
        private static CcProcesoMensualCreditoDesgloseConfigDbModel ObtenerConfiguracionCreditoDesglose(IDbConnection connection, int codInstitucion)
        {
            const string query = @"
                SELECT 
                    ISNULL(pr_cr_aplica_incon, 0) AS Aplica,
                    ISNULL(historico_cobro_envio, 0) AS HistoricoCobroEnvio,
                    dbo.MyGetdate() AS FechaServer
                FROM instituciones
                WHERE cod_institucion = @CodInstitucion";

            return connection.QueryFirstOrDefault<CcProcesoMensualCreditoDesgloseConfigDbModel>(
                query,
                new { CodInstitucion = codInstitucion },
                commandTimeout: CommandTimeoutSeconds)
                ?? new CcProcesoMensualCreditoDesgloseConfigDbModel();
        }

        /// <summary>
        /// Elimina registros de créditos del proceso según institución y fechas de corte.
        /// </summary>
        /// <param name="connection">Conexión activa a base de datos.</param>
        /// <param name="codInstitucion">Código de la institución.</param>
        /// <param name="fechaProceso">Fecha del proceso actual.</param>
        /// <param name="fechaAnterior">Fecha límite histórica a considerar.</param>
        private static void EliminarCreditosProceso(IDbConnection connection, int codInstitucion, decimal fechaProceso, decimal fechaAnterior)
        {
            const string query = @"
                DELETE FROM prm_creditos
                WHERE cod_institucion = @CodInstitucion
                  AND (
                        Fecha_Proceso <= @FechaAnterior
                        OR Fecha_Proceso = @FechaProceso
                      )";

            connection.Execute(
                query,
                new
                {
                    CodInstitucion = codInstitucion,
                    FechaProceso = fechaProceso,
                    FechaAnterior = fechaAnterior
                },
                commandTimeout: CommandTimeoutSeconds);
        }

        /// <summary>
        /// Ejecuta el procedimiento de desglose de créditos y retorna su resultado.
        /// </summary>
        /// <param name="connection">Conexión activa a base de datos.</param>
        /// <param name="request">Parámetros requeridos para el procedimiento.</param>
        /// <returns>Resultado del desglose de créditos.</returns>
        private static CcProcesoMensualCreditoDesgloseResultadoDbModel EjecutarCreditoDesglose(IDbConnection connection, CcProcesoMensualCreditoDesgloseDbRequest request)
        {
            const string query = @"
        EXEC spPrmCreditoDesgloseNew
            @CodInstitucion,
            @FechaProceso,
            @FechaSistema,
            @AplicaIncon,
            @PrimeraVez,
            @Cantidad";

            return connection.QueryFirstOrDefault<CcProcesoMensualCreditoDesgloseResultadoDbModel>(
            query,
            new
            {
                request.CodInstitucion,
                request.FechaProceso,
                FechaSistema = request.FechaSistema.Date,
                request.AplicaIncon,
                request.PrimeraVez,
                request.Cantidad
            },
            commandTimeout: CommandTimeoutSeconds)
            ?? new CcProcesoMensualCreditoDesgloseResultadoDbModel();
        }
        #endregion 

        #region  sbDesglocePlanilla
        /// <summary>
        /// Ejecuta el proceso completo de desglose de planilla para una institución.
        /// </summary>
        /// <param name="request">Solicitud con parámetros del proceso mensual.</param>
        /// <param name="reportarProgreso">Acción que informa total, procesados y mensaje de avance.</param>
        /// <param name="reportarLog">Acción que publica el detalle de las etapas en Galileo.</param>
        /// <returns>Resultado del desglose de planilla.</returns>
        public ErrorDto<CcProcesoMensualDesglosePlanillaResponse> CcProcesoMensual_DesglosarPlanilla_Ejecutar(
            CcProcesoMensualDesgloseRequest request,
            Action<int, int, string>? reportarProgreso = null,
            Action<string>? reportarLog = null)
        {
            if (request is null)
            {
                return DbHelper.CreateErrorResponse<CcProcesoMensualDesglosePlanillaResponse>(
               "La solicitud es requerida.",
                -1,
              new CcProcesoMensualDesglosePlanillaResponse());

            }

            var duracionTotal = Stopwatch.StartNew();
            var correlacionId = Guid.NewGuid();

            RegistrarAvance(correlacionId, "Inicio del desglose de planilla", reportarLog: reportarLog);

            using var connection = DbHelper.OpenConnection(_portalDb, request.CodEmpresa);
            _mGeneral.CcProcesoMensual_ProcesosAdd_Ejecutar(connection, request.CodEmpresa, "04", "PRE", request.Usuario, request.CodInstitucion, request.FechaProceso);

            connection.Open();

            try
            {
                var duracionEtapa = Stopwatch.StartNew();
                RegistrarAvance(correlacionId, "Iniciando detalle de aportes", reportarLog: reportarLog);
                reportarProgreso?.Invoke(0, 0, "Detallando Aportes...");
                DetallarAportes(connection, request.FechaProceso, request.CodInstitucion);
                RegistrarAvance(
                    correlacionId,
                    "Detalle de aportes finalizado",
                    $"Duración: {duracionEtapa.ElapsedMilliseconds} ms",
                    reportarLog);

                duracionEtapa.Restart();
                RegistrarAvance(correlacionId, "Iniciando desglose de créditos", reportarLog: reportarLog);
                reportarProgreso?.Invoke(0, 0, "Limpiando y Actualizando...");
                var resultadoDesglose = CrDesgloce(
                    connection,
                    request,
                    correlacionId,
                    reportarProgreso,
                    reportarLog);
                RegistrarAvance(
                    correlacionId,
                    "Desglose de créditos finalizado",
                    $"Duración: {duracionEtapa.ElapsedMilliseconds} ms",
                    reportarLog);

                duracionEtapa.Restart();
                RegistrarAvance(correlacionId, "Marcando institución como desglosada", reportarLog: reportarLog);
                MarcarInstitucionComoDesglosada(connection, request.CodInstitucion);
                RegistrarAvance(
                    correlacionId,
                    "Institución actualizada",
                    $"Duración: {duracionEtapa.ElapsedMilliseconds} ms",
                    reportarLog);

                RegistrarAvance(correlacionId, "Registrando bitácora del proceso finalizado", reportarLog: reportarLog);
                MProcesoMensualDb.SbBitacoraPlanilla(connection,
                                                  new CcProcesoMensualBitacoraPlanillaDto
                                                  {
                                                      Transaccion = "04",
                                                      CodInstitucion = request.CodInstitucion,
                                                      Proceso = request.FechaProceso,
                                                      Gestion = "R",
                                                      Usuario = request.Usuario
                                                  });

                _mGeneral.CcProcesoMensual_ProcesosAdd_Ejecutar(connection, request.CodEmpresa, "04", "POS", request.Usuario, request.CodInstitucion, request.FechaProceso);

                RegistrarAvance(
                    correlacionId,
                    "Desglose de planilla finalizado correctamente",
                    $"Duración total: {duracionTotal.ElapsedMilliseconds} ms",
                    reportarLog);

                reportarProgreso?.Invoke(
                    resultadoDesglose.Total,
                    resultadoDesglose.Procesados,
                    "Detalle de Aportes y Abonos finalizado.");

                return DbHelper.CreateOkResponse(
                   new CcProcesoMensualDesglosePlanillaResponse
                   {
                       Desglosado = true,
                       Mensaje = "- Detalle de Aportes y Abonos realizado satisfactoriamente...- Puede Proceder a las Aplicaciones...\""
                   });


            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error en el desglose de planilla. Correlación {CorrelacionId}; duración {DuracionMs} ms",
                    correlacionId,
                    duracionTotal.ElapsedMilliseconds);

                return DbHelper.CreateErrorResponse<CcProcesoMensualDesglosePlanillaResponse>(
                    ex.Message,
                    -1,
                    new CcProcesoMensualDesglosePlanillaResponse());
            }
        }

        /// <summary>
        /// Ejecuta el detalle de aportes para la institución y fecha de proceso indicada.
        /// </summary>
        /// <param name="connection">Conexión activa a base de datos.</param>
        /// <param name="fechaProceso">Fecha del proceso.</param>
        /// <param name="codInstitucion">Código de la institución.</param>
        private static void DetallarAportes(IDbConnection connection, decimal fechaProceso, int codInstitucion)
        {
            const string query = @"
                EXEC spPrmAporteDetalla
                    @FechaProceso,
                    @CodInstitucion";

            connection.Execute(
                query,
                new
                {
                    FechaProceso = fechaProceso,
                    CodInstitucion = codInstitucion
                },
                commandTimeout: CommandTimeoutSeconds);
        }

        /// <summary>
        /// Marca la institución como desglosada en la tabla de instituciones.
        /// </summary>
        /// <param name="connection">Conexión activa a base de datos.</param>
        /// <param name="codInstitucion">Código de la institución.</param>
        private static void MarcarInstitucionComoDesglosada(IDbConnection connection, int codInstitucion)
        {
            const string query = @"
                UPDATE instituciones
                SET pr_desgloza = 1
                WHERE cod_institucion = @CodInstitucion";

            connection.Execute(
                query,
                new
                {
                    CodInstitucion = codInstitucion
                },
                commandTimeout: CommandTimeoutSeconds);
        }

        /// <summary>
        /// Registra el avance de una etapa del desglose con su contexto de ejecución.
        /// </summary>
        /// <param name="correlacionId">Identificador generado por el servidor para agrupar la ejecución.</param>
        /// <param name="etapa">Nombre de la etapa que se está ejecutando.</param>
        /// <param name="detalle">Información adicional de diagnóstico.</param>
        /// <param name="reportarLog">Acción que publica la etapa en Galileo.</param>
        private void RegistrarAvance(
            Guid correlacionId,
            string etapa,
            string detalle = "",
            Action<string>? reportarLog = null)
        {
            var mensaje = string.IsNullOrWhiteSpace(detalle)
                ? etapa
                : $"{etapa}. {detalle}";

            reportarLog?.Invoke(mensaje);

            _logger.LogInformation(
                "Proceso mensual: {Etapa}. Correlación {CorrelacionId}. {Detalle}",
                etapa,
                correlacionId,
                detalle);
        }
        #endregion 
    }
}
