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
        /// <param name="transaction">Transacción activa.</param>
        /// <param name="request">Parámetros de solicitud del desglose.</param>
        public void CrDesgloce(IDbConnection connection, IDbTransaction transaction, CcProcesoMensualDesgloseRequest request)
        {
            var configuracion = ObtenerConfiguracionCreditoDesglose(connection, transaction, request.CodInstitucion);

            var aplicaIncon = configuracion.Aplica;
            var historicoCobroEnvio = configuracion.HistoricoCobroEnvio;
            var fechaSistema = configuracion.FechaServer;

            decimal fechaAnterior = request.FechaProceso;

            _logger.LogInformation(
                "Desglose de créditos: configuración obtenida. Institución {CodInstitucion}, período {FechaProceso}, histórico {HistoricoCobroEnvio}, aplica inconsistencias {AplicaIncon}",
                request.CodInstitucion,
                request.FechaProceso,
                historicoCobroEnvio,
                aplicaIncon);

            for (var i = 1; i <= historicoCobroEnvio; i++)
            {
                fechaAnterior = _mCobroDb.fxFechaProcesoAnterior(request.CodEmpresa, fechaAnterior);
            }

            EliminarCreditosProceso(connection, transaction, request.CodInstitucion, request.FechaProceso, fechaAnterior);

            _logger.LogInformation(
                "Desglose de créditos: registros anteriores eliminados. Institución {CodInstitucion}, período {FechaProceso}",
                request.CodInstitucion,
                request.FechaProceso);

            var resultado = EjecutarCreditoDesglose(
                connection, transaction,
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

            _logger.LogInformation(
                "Desglose de créditos: lote {NumeroLote} finalizado. Total {Total}, pendientes {Pendientes}",
                numeroLote,
                total,
                pendientes);

            while (pendientes > 0)
            {
                numeroLote++;
                resultado = EjecutarCreditoDesglose(
                    connection, transaction,
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

                _logger.LogInformation(
                    "Desglose de créditos: lote {NumeroLote} finalizado. Total {Total}, pendientes {Pendientes}",
                    numeroLote,
                    total,
                    pendientes);
            }
        }

        /// <summary>
        /// Obtiene la configuración necesaria para ejecutar el desglose de créditos.
        /// </summary>
        /// <param name="connection">Conexión activa a base de datos.</param>
        /// <param name="transaction">Transacción activa.</param>
        /// <param name="codInstitucion">Código de la institución.</param>
        /// <returns>Configuración del desglose de créditos.</returns>
        private static CcProcesoMensualCreditoDesgloseConfigDbModel ObtenerConfiguracionCreditoDesglose(IDbConnection connection, IDbTransaction transaction, int codInstitucion)
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
                transaction,
                commandTimeout: CommandTimeoutSeconds)
                ?? new CcProcesoMensualCreditoDesgloseConfigDbModel();
        }

        /// <summary>
        /// Elimina registros de créditos del proceso según institución y fechas de corte.
        /// </summary>
        /// <param name="connection">Conexión activa a base de datos.</param>
        /// <param name="transaction">Transacción activa.</param>
        /// <param name="codInstitucion">Código de la institución.</param>
        /// <param name="fechaProceso">Fecha del proceso actual.</param>
        /// <param name="fechaAnterior">Fecha límite histórica a considerar.</param>
        private static void EliminarCreditosProceso(IDbConnection connection, IDbTransaction transaction, int codInstitucion, decimal fechaProceso, decimal fechaAnterior)
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
                transaction,
                commandTimeout: CommandTimeoutSeconds);
        }

        /// <summary>
        /// Ejecuta el procedimiento de desglose de créditos y retorna su resultado.
        /// </summary>
        /// <param name="connection">Conexión activa a base de datos.</param>
        /// <param name="transaction">Transacción activa.</param>
        /// <param name="request">Parámetros requeridos para el procedimiento.</param>
        /// <returns>Resultado del desglose de créditos.</returns>
        private static CcProcesoMensualCreditoDesgloseResultadoDbModel EjecutarCreditoDesglose(IDbConnection connection, IDbTransaction transaction, CcProcesoMensualCreditoDesgloseDbRequest request)
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
            transaction,
            commandTimeout: CommandTimeoutSeconds)
            ?? new CcProcesoMensualCreditoDesgloseResultadoDbModel();
        }
        #endregion 

        #region  sbDesglocePlanilla
        /// <summary>
        /// Ejecuta el proceso completo de desglose de planilla para una institución.
        /// </summary>
        /// <param name="request">Solicitud con parámetros del proceso mensual.</param>
        /// <returns>Resultado del desglose de planilla.</returns>
        public ErrorDto<CcProcesoMensualDesglosePlanillaResponse> CcProcesoMensual_DesglosarPlanilla_Ejecutar(CcProcesoMensualDesgloseRequest request)
        {
            if (request is null)
            {
                return DbHelper.CreateErrorResponse<CcProcesoMensualDesglosePlanillaResponse>(
               "La solicitud es requerida.",
                -1,
              new CcProcesoMensualDesglosePlanillaResponse());

            }

            var duracionTotal = Stopwatch.StartNew();

            _logger.LogInformation(
                "Inicio del desglose de planilla. Empresa {CodEmpresa}, institución {CodInstitucion}, período {FechaProceso}, usuario {Usuario}",
                request.CodEmpresa,
                request.CodInstitucion,
                request.FechaProceso,
                request.Usuario);

            using var connection = DbHelper.OpenConnection(_portalDb, request.CodEmpresa);
            _mGeneral.CcProcesoMensual_ProcesosAdd_Ejecutar(connection, request.CodEmpresa, "04", "PRE", request.Usuario, request.CodInstitucion, request.FechaProceso);

            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var duracionEtapa = Stopwatch.StartNew();
                _logger.LogInformation("Desglose de planilla: iniciando detalle de aportes");
                DetallarAportes(connection, transaction, request.FechaProceso, request.CodInstitucion);
                _logger.LogInformation(
                    "Desglose de planilla: detalle de aportes finalizado en {DuracionMs} ms",
                    duracionEtapa.ElapsedMilliseconds);

                duracionEtapa.Restart();
                _logger.LogInformation("Desglose de planilla: iniciando desglose de créditos");
                CrDesgloce(connection, transaction, request);
                _logger.LogInformation(
                    "Desglose de planilla: desglose de créditos finalizado en {DuracionMs} ms",
                    duracionEtapa.ElapsedMilliseconds);

                duracionEtapa.Restart();
                _logger.LogInformation("Desglose de planilla: marcando institución como desglosada");
                MarcarInstitucionComoDesglosada(connection, transaction, request.CodInstitucion);
                _logger.LogInformation(
                    "Desglose de planilla: institución actualizada en {DuracionMs} ms",
                    duracionEtapa.ElapsedMilliseconds);

                _logger.LogInformation("Desglose de planilla: registrando bitácora y confirmando transacción");
                MProcesoMensualDb.SbBitacoraPlanilla(connection,
                                                  new CcProcesoMensualBitacoraPlanillaDto
                                                  {
                                                      Transaccion = "04",
                                                      CodInstitucion = request.CodInstitucion,
                                                      Proceso = request.FechaProceso,
                                                      Gestion = "R",
                                                      Usuario = request.Usuario
                                                  }, transaction);
                transaction.Commit();

                _mGeneral.CcProcesoMensual_ProcesosAdd_Ejecutar(connection, request.CodEmpresa, "04", "POS", request.Usuario, request.CodInstitucion, request.FechaProceso);

                _logger.LogInformation(
                    "Desglose de planilla finalizado correctamente en {DuracionMs} ms. Empresa {CodEmpresa}, institución {CodInstitucion}, período {FechaProceso}",
                    duracionTotal.ElapsedMilliseconds,
                    request.CodEmpresa,
                    request.CodInstitucion,
                    request.FechaProceso);

                return DbHelper.CreateOkResponse(
                   new CcProcesoMensualDesglosePlanillaResponse
                   {
                       Desglosado = true,
                       Mensaje = "- Detalle de Aportes y Abonos realizado satisfactoriamente...- Puede Proceder a las Aplicaciones...\""
                   });


            }
            catch (Exception ex)
            {
                transaction.Rollback();

                _logger.LogError(
                    ex,
                    "Error en el desglose de planilla después de {DuracionMs} ms. Empresa {CodEmpresa}, institución {CodInstitucion}, período {FechaProceso}",
                    duracionTotal.ElapsedMilliseconds,
                    request.CodEmpresa,
                    request.CodInstitucion,
                    request.FechaProceso);

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
        /// <param name="transaction">Transacción activa.</param>
        /// <param name="fechaProceso">Fecha del proceso.</param>
        /// <param name="codInstitucion">Código de la institución.</param>
        private static void DetallarAportes(IDbConnection connection, IDbTransaction transaction, decimal fechaProceso, int codInstitucion)
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
                transaction: transaction,
                commandTimeout: CommandTimeoutSeconds);
        }

        /// <summary>
        /// Marca la institución como desglosada en la tabla de instituciones.
        /// </summary>
        /// <param name="connection">Conexión activa a base de datos.</param>
        /// <param name="transaction">Transacción activa.</param>
        /// <param name="codInstitucion">Código de la institución.</param>
        private static void MarcarInstitucionComoDesglosada(IDbConnection connection, IDbTransaction transaction, int codInstitucion)
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
                transaction,
                commandTimeout: CommandTimeoutSeconds);
        }
        #endregion 
    }
}
