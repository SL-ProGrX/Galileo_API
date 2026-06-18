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
        public CcProcesoMensualAplicacionAhorrosDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _mProGrx = new MProGrxMain(config);
            _Security_MainDB = new MSecurityMainDb(config);
            _mGeneral = new CcProcesoMensualGeneralDb(config);

        }

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
                        ActualizarEstadoInstitucion(context, "pr_apAplica");
                    }
                });
        }
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
                    EjecutarProceso = context => ActualizarEstadoInstitucion(context, "pr_apInco")
                });
        }
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
                    EjecutarProceso = context => ActualizarEstadoInstitucion(context, "pr_apDev")
                });
        }

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

        private static void ActualizarEstadoInstitucion(AplicacionAhorroContext context, string campo)
        {
            var camposPermitidos = new Dictionary<string, string>
            {
                ["pr_apAplica"] = "pr_apAplica",
                ["pr_apInco"] = "pr_apInco",
                ["pr_apDev"] = "pr_apDev"
            };

            if (!camposPermitidos.TryGetValue(campo, out var campoSql))
            {
                throw new ArgumentException("Campo de actualización inválido.", nameof(campo));
            }

            var query = $"""
                UPDATE instituciones
                SET {campoSql} = 1
                WHERE cod_institucion = @CodInstitucion
                """;

            context.Connection.Execute(
                query,
                new { context.CodInstitucion });
        }
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
    }
}
