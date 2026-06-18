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
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
            DateTime vFecha = _mProGrx.fxFechaServidor(codEmpresa, 0);
            try
            {
                _mGeneral.CcProcesoMensual_ProcesosAdd_Ejecutar(connection, codEmpresa, "05", "PRE", usuario, codInstitucion, fechaProceso);


                EjecutarAplicacionAportes(connection, codInstitucion, fechaProceso, usuario);
                ProcesarDevolucionesAhorros(connection, codInstitucion, fechaProceso, vFecha, usuario);
                ActualizarEstadoAplicacionAhorros(connection, codInstitucion);
                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"PRM - Aplicación de Aportes Inst: {codInstitucion}",
                    Movimiento = movimientoBitacora,
                    Modulo = vModulo
                });

                MProcesoMensualDb.SbBitacoraPlanilla(connection,
                                                    new CcProcesoMensualBitacoraPlanillaDto
                                                    {
                                                        Transaccion = "05",
                                                        CodInstitucion = codInstitucion,
                                                        Proceso = fechaProceso,
                                                        Gestion = "R",
                                                        Usuario = usuario
                                                    });

                var datosReporte = ObtenerParametrosAhorroReporte(connection, codInstitucion);

                _mGeneral.CcProcesoMensual_ProcesosAdd_Ejecutar(connection, codEmpresa, "05", "POS", usuario, codInstitucion, fechaProceso);

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
        public ErrorDto<CcProcesoMensualAhorros> CcProcesoMensual_AhorrosInconsistencias_Aplicar(int codEmpresa, int codInstitucion, decimal fechaProceso, string usuario)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
          
            try
            {
                _mGeneral.CcProcesoMensual_ProcesosAdd_Ejecutar(connection, codEmpresa, "06", "PRE", usuario, codInstitucion, fechaProceso);

                ActualizarEstadoAplicacionAhorrosInconsistencias(connection, codInstitucion);
                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"PRM-AHORRO Reporte Inconsistencias Inst: {codInstitucion}",
                    Movimiento = movimientoBitacora,
                    Modulo = vModulo
                });

                MProcesoMensualDb.SbBitacoraPlanilla(connection, new CcProcesoMensualBitacoraPlanillaDto
                { Transaccion = "06", CodInstitucion = codInstitucion, Proceso = fechaProceso, Gestion = "R", Usuario = usuario });

                var datosReporte = ObtenerParametrosAhorroReporte(connection, codInstitucion);
                _mGeneral.CcProcesoMensual_ProcesosAdd_Ejecutar(connection, codEmpresa, "06", "POS", usuario, codInstitucion, fechaProceso);

                return DbHelper.CreateOkResponse(new CcProcesoMensualAhorros { Aplicado = true, ParametrosReporte = datosReporte });

            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<CcProcesoMensualAhorros>(
                 ex.Message,
                 -1,
                 new CcProcesoMensualAhorros());

            }
        }
        public ErrorDto<CcProcesoMensualAhorros> CcProcesoMensual_AhorrosDevoluciones_Aplicar(int codEmpresa, int codInstitucion, decimal fechaProceso, string usuario)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
         
            try
            {
                _mGeneral.CcProcesoMensual_ProcesosAdd_Ejecutar(connection, codEmpresa, "07", "PRE", usuario, codInstitucion, fechaProceso);

                ActualizarEstadoDevolucionesAhorros(connection, codInstitucion);
                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"PRM-AHORRO Reporte Devoluciones Inst: {codInstitucion}",
                    Movimiento = movimientoBitacora,
                    Modulo = vModulo
                });

                MProcesoMensualDb.SbBitacoraPlanilla(connection, new CcProcesoMensualBitacoraPlanillaDto
                { Transaccion = "07", CodInstitucion = codInstitucion, Proceso = fechaProceso, Gestion = "R", Usuario = usuario });

                var datosReporte = ObtenerParametrosAhorroReporte(connection, codInstitucion);
                _mGeneral.CcProcesoMensual_ProcesosAdd_Ejecutar(connection, codEmpresa, "07", "POS", usuario, codInstitucion, fechaProceso);

                return DbHelper.CreateOkResponse(new CcProcesoMensualAhorros { Aplicado = true, ParametrosReporte = datosReporte });

            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<CcProcesoMensualAhorros>(
                 ex.Message,
                 -1,
                 new CcProcesoMensualAhorros());

            }
        }
        private static void EjecutarAplicacionAportes(IDbConnection connection, int codInstitucion, decimal fechaProceso, string usuario)
        {
            const string query = @"
                EXEC spPrmAporteAplica
                    @FechaProceso,
                    @CodInstitucion,
                    @Usuario,
                    @Paso";

            EjecutarPasoAplicacionAportes(connection, query, fechaProceso, codInstitucion, usuario, 1);
            EjecutarPasoAplicacionAportes(connection, query, fechaProceso, codInstitucion, usuario, 2);
        }

        private static void EjecutarPasoAplicacionAportes(IDbConnection connection, string query, decimal fechaProceso, int codInstitucion, string usuario, int paso)
        {
            connection.Execute(query, new
            {
                FechaProceso = fechaProceso,
                CodInstitucion = codInstitucion,
                Usuario = usuario,
                Paso = paso
            });
        }

        private static void ProcesarDevolucionesAhorros(IDbConnection connection, int codInstitucion, decimal fechaProceso, DateTime fecha, string usuario)
        {
            var parametros = ObtenerParametrosAhorros(connection, codInstitucion);

            if (parametros is null || parametros.FndApAplica != 1)
            {
                return;
            }

            var documento = $"{fechaProceso}.{codInstitucion}.PAT.01";
            var socios = ObtenerSociosDevolucion(connection, codInstitucion, fechaProceso);

            foreach (var socio in socios)
            {
                EjecutarDevolucionSiAplica(
                    connection,
                    parametros,
                    socio,
                    codInstitucion,
                    fechaProceso,
                    documento,
                    fecha);
            }

            EliminarSociosTempDevolucion(connection, codInstitucion, fechaProceso);


            MProcesoMensualDb.SbFndAsiento(connection, new ProcesoMensualFndAsientoRequest
            {
                Proceso = fechaProceso,
                CodInstitucion = codInstitucion,
                Operadora = parametros.FndApOperadora,
                Plan = parametros.FndApPlan,
                Cuenta = parametros.CtaInconsistencia,
                Usuario = usuario,
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

        private static void EjecutarDevolucionSiAplica(IDbConnection connection, CcProcesoMensualAhorroParametrosDbModel parametros, CcProcesoMensualSocioDevolucionDbModel socio, int codInstitucion, decimal fechaProceso, string documento, DateTime fecha)
        {
            if (socio.Monto > 0)
            {
                EjecutarDevolucionFondo(
                    connection,
                    new CcProcesoMensualDevolucionFondoRequest
                    {
                        CodInstitucion = codInstitucion,
                        FechaProceso = fechaProceso,
                        Operadora = parametros.FndApOperadora,
                        Plan = parametros.FndApPlan,
                        Cedula = socio.Cedula,
                        Monto = socio.Monto,
                        Documento = documento,
                        CuentaInconsistencia = parametros.CtaInconsistencia,
                        Tipo = "A",
                        Fecha = fecha
                    });
            }
            if (socio.Aporte > 0)
            {
                EjecutarDevolucionFondo(
                    connection,
                    new CcProcesoMensualDevolucionFondoRequest
                    {
                        CodInstitucion = codInstitucion,
                        FechaProceso = fechaProceso,
                        Operadora = parametros.FndApOperadora,
                        Plan = parametros.FndApPlanP,
                        Cedula = socio.Cedula,
                        Monto = socio.Aporte,
                        Documento = documento,
                        CuentaInconsistencia = parametros.CtaInconsistencia,
                        Tipo = "P",
                        Fecha = fecha
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

        private static void ActualizarEstadoAplicacionAhorros(IDbConnection connection, int codInstitucion)
        {
            const string query = @"  UPDATE instituciones  SET pr_apAplica = 1  WHERE cod_institucion = @CodInstitucion";

            connection.Execute(query, new { CodInstitucion = codInstitucion });
        }
        private static void ActualizarEstadoAplicacionAhorrosInconsistencias(IDbConnection connection, int codInstitucion)
        {
            const string query = @" UPDATE instituciones  SET pr_apInco = 1 WHERE cod_institucion = @CodInstitucion";

            connection.Execute(query, new { CodInstitucion = codInstitucion });
        }
        
        private static void ActualizarEstadoDevolucionesAhorros(IDbConnection connection, int codInstitucion)
        {
            const string query = @"
                UPDATE instituciones
                SET pr_apDev = 1
                WHERE cod_institucion = @CodInstitucion";

            connection.Execute(query, new { CodInstitucion = codInstitucion });
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
