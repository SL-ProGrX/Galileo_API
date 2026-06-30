using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using System.Data;
using static Galileo_API.Models.MProcesoMensualModels;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB
{
    public class CcProcesoMensualAplicacionAhorrosDb
    {
        private readonly PortalDB _portalDb;
        private readonly MProGrxMain _mProGrx;
        private readonly int vModulo = 3;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly CcProcesoMensualGeneralDb _mGeneral;
        private readonly string movimientoBitacora = "Aplica - WEB";

        /// <summary>
        /// Inicializa una nueva instancia para gestionar la aplicación de ahorros del proceso mensual.
        /// </summary>
        /// <param name="config">Configuración general de la aplicación.</param>
        public CcProcesoMensualAplicacionAhorrosDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _mProGrx = new MProGrxMain(config);
            _Security_MainDB = new MSecurityMainDb(config);
            _mGeneral = new CcProcesoMensualGeneralDb(config);

        }

        /// <summary>
        /// Ejecuta la aplicación de aportes para una institución y proceso mensual.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codInstitucion">Código de la institución.</param>
        /// <param name="fechaProceso">Fecha de proceso.</param>
        /// <param name="usuario">Usuario que ejecuta el proceso.</param>
        /// <returns>Resultado de la ejecución de aplicación de aportes.</returns>
        public ErrorDto<CcProcesoMensualAhorros> CcProcesoMensual_Ahorros_Aplicar(int codEmpresa, int codInstitucion, decimal fechaProceso, string usuario)
        {
            return EjecutarAplicacionAhorro(
                codEmpresa,
                codInstitucion,
                fechaProceso,
                usuario,
                new AplicacionAhorroConfig
                {
                    Transaccion = "05",
                    DetalleMovimiento = $"PRM - Aplicación de Aportes Inst: {codInstitucion}",
                    EjecutarProceso = context =>
                    {
                        EjecutarAplicacionAportes(context);
                        ProcesarDevolucionesAhorros(context);
                        ActualizarEstadoInstitucion(context, TipoAplicacionAhorro.Aportes);
                    }
                });
        }

        /// <summary>
        /// Ejecuta la aplicación del proceso de inconsistencias de ahorros.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codInstitucion">Código de la institución.</param>
        /// <param name="fechaProceso">Fecha de proceso.</param>
        /// <param name="usuario">Usuario que ejecuta el proceso.</param>
        /// <returns>Resultado de la ejecución de inconsistencias.</returns>
        public ErrorDto<CcProcesoMensualAhorros> CcProcesoMensual_AhorrosInconsistencias_Aplicar(int codEmpresa, int codInstitucion, decimal fechaProceso, string usuario)
        {
            return EjecutarAplicacionAhorro(
                codEmpresa,
                codInstitucion,
                fechaProceso,
                usuario,
                new AplicacionAhorroConfig
                {
                    Transaccion = "06",
                    DetalleMovimiento = $"PRM-AHORRO Reporte Inconsistencias Inst: {codInstitucion}",
                    EjecutarProceso = context => ActualizarEstadoInstitucion(context, TipoAplicacionAhorro.Inconsistencias)
                });
        }

        /// <summary>
        /// Ejecuta la aplicación del proceso de devoluciones de ahorros.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codInstitucion">Código de la institución.</param>
        /// <param name="fechaProceso">Fecha de proceso.</param>
        /// <param name="usuario">Usuario que ejecuta el proceso.</param>
        /// <returns>Resultado de la ejecución de devoluciones.</returns>
        public ErrorDto<CcProcesoMensualAhorros> CcProcesoMensual_AhorrosDevoluciones_Aplicar(int codEmpresa, int codInstitucion, decimal fechaProceso, string usuario)
        {
            return EjecutarAplicacionAhorro(
                codEmpresa,
                codInstitucion,
                fechaProceso,
                usuario,
                new AplicacionAhorroConfig
                {
                    Transaccion = "07",
                    DetalleMovimiento = $"PRM-AHORRO Reporte Devoluciones Inst: {codInstitucion}",
                    EjecutarProceso = context => ActualizarEstadoInstitucion(context, TipoAplicacionAhorro.Devoluciones)
                });
        }

        /// <summary>
        /// Ejecuta el flujo común de aplicación de ahorros según la configuración recibida.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codInstitucion">Código de la institución.</param>
        /// <param name="fechaProceso">Fecha de proceso.</param>
        /// <param name="usuario">Usuario que ejecuta el proceso.</param>
        /// <param name="config">Configuración específica del tipo de aplicación.</param>
        /// <returns>Resultado del proceso de aplicación.</returns>
        private ErrorDto<CcProcesoMensualAhorros> EjecutarAplicacionAhorro(int codEmpresa, int codInstitucion, decimal fechaProceso, string usuario, AplicacionAhorroConfig config)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            var context = new AplicacionAhorroContext
            {
                Connection = connection,
                CodEmpresa = codEmpresa,
                CodInstitucion = codInstitucion,
                FechaProceso = fechaProceso,
                Usuario = usuario,
                FechaServidor = _mProGrx.fxFechaServidor(codEmpresa, 0)
            };

            try
            {
                EjecutarProcesoMensual(context, config.Transaccion, "PRE");

                config.EjecutarProceso(context);

                RegistrarBitacora(context, config.DetalleMovimiento);
                RegistrarBitacoraPlanilla(context, config.Transaccion);

                var datosReporte = ObtenerParametrosAhorroReporte(
                    context.Connection,
                    context.CodInstitucion);

                EjecutarProcesoMensual(context, config.Transaccion, "POS");

                return DbHelper.CreateOkResponse(
                    new CcProcesoMensualAhorros
                    {
                        Aplicado = true,
                        ParametrosReporte = datosReporte
                    });
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<CcProcesoMensualAhorros>(
                    ex.Message,
                    -1,
                    new CcProcesoMensualAhorros());
            }
        }

        /// <summary>
        /// Registra en bitácora de planilla la transacción ejecutada.
        /// </summary>
        /// <param name="context">Contexto de ejecución del proceso.</param>
        /// <param name="transaccion">Código de transacción.</param>
        private static void RegistrarBitacoraPlanilla(AplicacionAhorroContext context, string transaccion)
        {
            MProcesoMensualDb.SbBitacoraPlanilla(
                context.Connection,
                new CcProcesoMensualBitacoraPlanillaDto
                {
                    Transaccion = transaccion,
                    CodInstitucion = context.CodInstitucion,
                    Proceso = context.FechaProceso,
                    Gestion = "R",
                    Usuario = context.Usuario
                });
        }

        /// <summary>
        /// Registra en bitácora de seguridad el movimiento ejecutado.
        /// </summary>
        /// <param name="context">Contexto de ejecución del proceso.</param>
        /// <param name="detalleMovimiento">Detalle del movimiento a registrar.</param>
        private void RegistrarBitacora(AplicacionAhorroContext context, string detalleMovimiento)
        {
            _Security_MainDB.Bitacora(
                new BitacoraInsertarDto
                {
                    EmpresaId = context.CodEmpresa,
                    Usuario = context.Usuario,
                    DetalleMovimiento = detalleMovimiento,
                    Movimiento = movimientoBitacora,
                    Modulo = vModulo
                });
        }

        /// <summary>
        /// Registra el estado del proceso mensual en PRE o POS.
        /// </summary>
        /// <param name="context">Contexto de ejecución del proceso.</param>
        /// <param name="transaccion">Código de transacción.</param>
        /// <param name="estado">Estado del proceso (PRE/POS).</param>
        private void EjecutarProcesoMensual(AplicacionAhorroContext context, string transaccion, string estado)
        {
            _mGeneral.CcProcesoMensual_ProcesosAdd_Ejecutar(
                context.Connection,
                context.CodEmpresa,
                transaccion,
                estado,
                context.Usuario,
                context.CodInstitucion,
                context.FechaProceso);
        }

        /// <summary>
        /// Ejecuta los pasos de aplicación de aportes.
        /// </summary>
        /// <param name="context">Contexto de ejecución del proceso.</param>
        private static void EjecutarAplicacionAportes(AplicacionAhorroContext context)
        {
            const string query = @"
                EXEC spPrmAporteAplica
                    @FechaProceso,
                    @CodInstitucion,
                    @Usuario,
                    @Paso";

            EjecutarPasoAplicacionAportes(context, query, 1);
            EjecutarPasoAplicacionAportes(context, query, 2);
        }

        /// <summary>
        /// Ejecuta un paso específico del procedimiento de aplicación de aportes.
        /// </summary>
        /// <param name="context">Contexto de ejecución del proceso.</param>
        /// <param name="query">Consulta o procedimiento a ejecutar.</param>
        /// <param name="paso">Número de paso a ejecutar.</param>
        private static void EjecutarPasoAplicacionAportes(AplicacionAhorroContext context, string query, int paso)
        {
            context.Connection.Execute(
                query,
                new
                {
                    context.FechaProceso,
                    context.CodInstitucion,
                    context.Usuario,
                    Paso = paso
                });
        }

        /// <summary>
        /// Procesa devoluciones de ahorros cuando la configuración institucional lo permite.
        /// </summary>
        /// <param name="context">Contexto de ejecución del proceso.</param>
        private static void ProcesarDevolucionesAhorros(AplicacionAhorroContext context)
        {
            var parametros = ObtenerParametrosAhorros(
                context.Connection,
                context.CodInstitucion);

            if (parametros is null || parametros.FndApAplica != 1)
            {
                return;
            }

            var documento = $"{context.FechaProceso}.{context.CodInstitucion}.PAT.01";

            var socios = ObtenerSociosDevolucion(
                context.Connection,
                context.CodInstitucion,
                context.FechaProceso);

            foreach (var socio in socios)
            {
                EjecutarDevolucionSiAplica(
                    context,
                    parametros,
                    socio,
                    documento);
            }

            EliminarSociosTempDevolucion(
                context.Connection,
                context.CodInstitucion,
                context.FechaProceso);

            MProcesoMensualDb.SbFndAsiento(
                context.Connection,
                new ProcesoMensualFndAsientoRequest
                {
                    Proceso = context.FechaProceso,
                    CodInstitucion = context.CodInstitucion,
                    Operadora = parametros.FndApOperadora,
                    Plan = parametros.FndApPlan,
                    Cuenta = parametros.CtaInconsistencia,
                    Usuario = context.Usuario,
                    NumeroDocumento = documento
                });
        }

        /// <summary>
        /// Obtiene parámetros de configuración de ahorros de la institución.
        /// </summary>
        /// <param name="connection">Conexión activa a base de datos.</param>
        /// <param name="codInstitucion">Código de la institución.</param>
        /// <returns>Parámetros de ahorro o <c>null</c> si no existen.</returns>
        private static CcProcesoMensualAhorroParametrosDbModel? ObtenerParametrosAhorros(IDbConnection connection, int codInstitucion)
        {
            const string query = @"
                SELECT
                    ISNULL(fnd_ap_Aplica, 0) AS FndApAplica,
                    ISNULL(fnd_ap_operadora, 0) AS FndApOperadora,
                    ISNULL(fnd_ap_plan, '') AS FndApPlan,
                    ISNULL(fnd_ap_planP, '') AS FndApPlanP,
                    ISNULL(cta_inconsistencia, '') AS CtaInconsistencia
                FROM instituciones
                WHERE cod_institucion = @CodInstitucion";

            return connection.QueryFirstOrDefault<CcProcesoMensualAhorroParametrosDbModel>(
                query,
                new { CodInstitucion = codInstitucion });
        }

        /// <summary>
        /// Obtiene socios marcados para devolución en el proceso.
        /// </summary>
        /// <param name="connection">Conexión activa a base de datos.</param>
        /// <param name="codInstitucion">Código de la institución.</param>
        /// <param name="fechaProceso">Fecha de proceso.</param>
        /// <returns>Lista de socios con devolución.</returns>
        private static List<CcProcesoMensualSocioDevolucionDbModel> ObtenerSociosDevolucion(IDbConnection connection, int codInstitucion, decimal fechaProceso)
        {
            const string query = @"
                SELECT
                    Cedula,
                    Monto,
                    Aporte
                FROM sociostemp
                WHERE existe = 'D'
                  AND cod_institucion = @CodInstitucion
                  AND fechaproc = @FechaProceso
                  AND (monto + aporte) > 0";

            return [.. connection.Query<CcProcesoMensualSocioDevolucionDbModel>(
                query,
                new
                {
                    CodInstitucion = codInstitucion,
                    FechaProceso = fechaProceso
                })];
        }

        /// <summary>
        /// Ejecuta devoluciones para monto y aporte del socio cuando corresponda.
        /// </summary>
        /// <param name="context">Contexto de ejecución del proceso.</param>
        /// <param name="parametros">Parámetros de ahorro institucional.</param>
        /// <param name="socio">Datos del socio a procesar.</param>
        /// <param name="documento">Número de documento asociado.</param>
        private static void EjecutarDevolucionSiAplica(AplicacionAhorroContext context, CcProcesoMensualAhorroParametrosDbModel parametros, CcProcesoMensualSocioDevolucionDbModel socio, string documento)
        {
            if (socio.Monto > 0)
            {
                EjecutarDevolucionFondo(
                    context.Connection,
                    new CcProcesoMensualDevolucionFondoRequest
                    {
                        CodInstitucion = context.CodInstitucion,
                        FechaProceso = context.FechaProceso,
                        Operadora = parametros.FndApOperadora,
                        Plan = parametros.FndApPlan,
                        Cedula = socio.Cedula,
                        Monto = socio.Monto,
                        Documento = documento,
                        CuentaInconsistencia = parametros.CtaInconsistencia,
                        Tipo = "A",
                        Fecha = context.FechaServidor
                    });
            }

            if (socio.Aporte > 0)
            {
                EjecutarDevolucionFondo(
                    context.Connection,
                    new CcProcesoMensualDevolucionFondoRequest
                    {
                        CodInstitucion = context.CodInstitucion,
                        FechaProceso = context.FechaProceso,
                        Operadora = parametros.FndApOperadora,
                        Plan = parametros.FndApPlanP,
                        Cedula = socio.Cedula,
                        Monto = socio.Aporte,
                        Documento = documento,
                        CuentaInconsistencia = parametros.CtaInconsistencia,
                        Tipo = "P",
                        Fecha = context.FechaServidor
                    });
            }
        }

        /// <summary>
        /// Ejecuta el procedimiento de devolución de fondos.
        /// </summary>
        /// <param name="connection">Conexión activa a base de datos.</param>
        /// <param name="request">Parámetros de la devolución.</param>
        private static void EjecutarDevolucionFondo(IDbConnection connection, CcProcesoMensualDevolucionFondoRequest request)
        {
            const string query = @"
        EXEC spPrmDevFondos
            @CodInstitucion,
            @FechaProceso,
            @Operadora,
            @Plan,
            @Cedula,
            @Monto,
            @Documento,
            @CuentaInconsistencia,
            @Tipo,
            @Fecha";

            connection.Execute(query, new
            {
                request.CodInstitucion,
                request.FechaProceso,
                request.Operadora,
                Plan = request.Plan.Trim(),
                Cedula = request.Cedula.Trim(),
                request.Monto,
                Documento = request.Documento.Trim(),
                CuentaInconsistencia = request.CuentaInconsistencia.Trim(),
                Tipo = request.Tipo.Trim(),
                request.Fecha
            });
        }

        /// <summary>
        /// Elimina registros temporales de socios procesados para devolución.
        /// </summary>
        /// <param name="connection">Conexión activa a base de datos.</param>
        /// <param name="codInstitucion">Código de la institución.</param>
        /// <param name="fechaProceso">Fecha de proceso.</param>
        private static void EliminarSociosTempDevolucion(IDbConnection connection, int codInstitucion, decimal fechaProceso)
        {
            const string query = @"
                DELETE sociosTemp
                WHERE existe = 'D'
                  AND fechaProc = @FechaProceso
                  AND cod_institucion = @CodInstitucion";

            connection.Execute(query, new
            {
                FechaProceso = fechaProceso,
                CodInstitucion = codInstitucion
            });
        }

        /// <summary>
        /// Actualiza el indicador de estado de aplicación en la institución según tipo de proceso.
        /// </summary>
        /// <param name="context">Contexto de ejecución del proceso.</param>
        /// <param name="tipoAplicacion">Tipo de aplicación de ahorro.</param>
        private static void ActualizarEstadoInstitucion(  AplicacionAhorroContext context,   TipoAplicacionAhorro tipoAplicacion)
        {
            var query = tipoAplicacion switch
            {
                TipoAplicacionAhorro.Aportes => """
            UPDATE instituciones
            SET pr_apAplica = 1
            WHERE cod_institucion = @CodInstitucion
            """,

                TipoAplicacionAhorro.Inconsistencias => """
            UPDATE instituciones
            SET pr_apInco = 1
            WHERE cod_institucion = @CodInstitucion
            """,

                TipoAplicacionAhorro.Devoluciones => """
            UPDATE instituciones
            SET pr_apDev = 1
            WHERE cod_institucion = @CodInstitucion
            """,

                _ => throw new ArgumentOutOfRangeException(
                    nameof(tipoAplicacion),
                    tipoAplicacion,
                    "Tipo de aplicación no soportado.")
            };

            context.Connection.Execute(
                query,
                new { context.CodInstitucion });
        }

        /// <summary>
        /// Obtiene los parámetros para el reporte de ahorros de una institución.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codInstitucion">Código de la institución.</param>
        /// <returns>Resultado con los parámetros del reporte.</returns>
        public ErrorDto<CcProcesoMensualAhorroReporteModel> CcProcesoMensual_ParametrosAhorroReporte_Obtener(int codEmpresa, int codInstitucion)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

                const string query = @"
            SELECT
                ISNULL(porc_aporte, 0) / 100.0 AS Porcentaje,
                ISNULL(porc_ahorro, 0) / 100.0 AS PorcAhorro
            FROM instituciones
            WHERE cod_institucion = @CodInstitucion";

                var result = connection.QueryFirstOrDefault<CcProcesoMensualAhorroReporteModel>(
                    query,
                    new { CodInstitucion = codInstitucion }) ?? new CcProcesoMensualAhorroReporteModel();

                return new ErrorDto<CcProcesoMensualAhorroReporteModel>
                {
                    Code = 0,
                    Description = "Consulta realizada correctamente.",
                    Result = result
                };
            }
            catch (Exception ex)
            {
                return new ErrorDto<CcProcesoMensualAhorroReporteModel>
                {
                    Code = -1,
                    Description = ex.Message,
                    Result = new CcProcesoMensualAhorroReporteModel()
                };
            }
        }

        /// <summary>
        /// Obtiene los parámetros del reporte de ahorros directamente desde la base de datos.
        /// </summary>
        /// <param name="connection">Conexión activa a base de datos.</param>
        /// <param name="codInstitucion">Código de la institución.</param>
        /// <returns>Modelo con porcentaje de aporte y ahorro.</returns>
        private static CcProcesoMensualAhorroReporteModel ObtenerParametrosAhorroReporte(IDbConnection connection, int codInstitucion)
        {
            const string query = @"
                SELECT
                    ISNULL(porc_aporte, 0) / 100 AS Porcentaje,
                    ISNULL(porc_ahorro, 0) / 100 AS PorcAhorro
                FROM instituciones
                WHERE cod_institucion = @CodInstitucion";

            return connection.QueryFirstOrDefault<CcProcesoMensualAhorroReporteModel>(
                query,
                new { CodInstitucion = codInstitucion }) ?? new CcProcesoMensualAhorroReporteModel();
        }

        private sealed class AplicacionAhorroContext
        {
            public required IDbConnection Connection { get; init; }
            public required int CodEmpresa { get; init; }
            public required int CodInstitucion { get; init; }
            public required decimal FechaProceso { get; init; }
            public required string Usuario { get; init; }
            public DateTime FechaServidor { get; init; }
        }
        private sealed class AplicacionAhorroConfig
        {
            public required string Transaccion { get; init; }
            public required string DetalleMovimiento { get; init; }
            public required Action<AplicacionAhorroContext> EjecutarProceso { get; init; }
        }
        private sealed class CcProcesoMensualAhorroParametrosDbModel
        {
            public int FndApAplica { get; set; } = 0;
            public int FndApOperadora { get; set; } = 0;
            public string FndApPlan { get; set; } = string.Empty;
            public string FndApPlanP { get; set; } = string.Empty;
            public string CtaInconsistencia { get; set; } = string.Empty;
        }

        private sealed class CcProcesoMensualSocioDevolucionDbModel
        {
            public string Cedula { get; set; } = string.Empty;
            public decimal Monto { get; set; } = 0;
            public decimal Aporte { get; set; } = 0;
        }
        private enum TipoAplicacionAhorro
        {
            Aportes,
            Inconsistencias,
            Devoluciones
        }
    }
}
