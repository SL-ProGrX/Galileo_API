using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.Security;
using Galileo.Models.ERROR; 
using System.Data;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB
{
    public class CcProcesoMensualEnvioDb
    {
        private readonly PortalDB _portalDb;
        private readonly MProGrxMain _mProGrx;
        private readonly MCobroDb _mCobroDb;
        private readonly CcProcesoMensualGeneralDb _mGeneral;
        private readonly CcProcesoMensualEstadoDB _mEstado;
        private readonly int vModulo = 3;
        private readonly MSecurityMainDb _Security_MainDB;
        public const string PlanillaEnvioAya = "08";
        public const string PlanillaEnvioSpa = "09";
        public const string PlanillaEnvioIna = "13";

        public CcProcesoMensualEnvioDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _mProGrx = new MProGrxMain(config);
            _mCobroDb = new MCobroDb(config);
            _mGeneral = new CcProcesoMensualGeneralDb(config);           
            _mEstado = new CcProcesoMensualEstadoDB(config);
            _Security_MainDB = new MSecurityMainDb(config);
        }

        public ErrorDto<bool> CcProcesoMensual_GeneraDeducciones_Ejecutar(int codEmpresa,  CcProcesoMensualGeneraDeduccionesRequest request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
         
            try
            {
                var globalesDto = _mProGrx.sbSifParametrosInicializa(codEmpresa, request.Usuario); //codContabilidad
                var glngFechaCR = globalesDto?.Result?.GlngFechaCR ?? 0;

                _mGeneral.CcProcesoMensual_ProcesosAdd_Ejecutar(connection, codEmpresa, "02", "PRE", request.Usuario, request.CodInstitucion, request.FechaProceso);

                var config = ObtenerConfiguracionGeneracion(connection, request.CodInstitucion);

                Helpers.CcProcesoMensualCreditosHelperDb.SbCrCalculaSaldoMes(connection, request.CodInstitucion, 0);

                var fechaAnterior = ObtenerFechaAnteriorHistorica(request.FechaProceso, codEmpresa, config.HistoricoCobroEnvio);

                if (DebeProcesar(config.CodigoCreditosEnv))
                {
                    ProcesarCreditosEnvio(connection, request, glngFechaCR, fechaAnterior);
                }

                CodificacionEnvio_Ejecutar(connection, request);
                CreditoCambioDeducciones_Ejecutar(connection, request);
                ExclusionPoliticaGeneral_Ejecutar(connection, request);

                var planillaConfig = ObtenerConfigPlanillaEnvio(connection, request.CodInstitucion);

                ComparacionSiAplica_Ejecutar(connection, request,planillaConfig);

                AjustesPorPlanilla_Ejecutar(connection, request,planillaConfig);

                CreditoCambioDeducciones_Ejecutar(connection, request);

                ActualizarEstadoGeneracion(connection, request.CodInstitucion);

                RegistrarBitacoraGeneracion( connection, codEmpresa,request);

             
                _mEstado.CcProcesoMensual_EstadoActualProceso_Obtener( codEmpresa, request.CodInstitucion);
                _mGeneral.CcProcesoMensual_ProcesosAdd_Ejecutar(connection, codEmpresa, "02", "POS", request.Usuario, request.CodInstitucion, request.FechaProceso);

                return DbHelper.CreateOkResponse(true);
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse<bool>(
                    "Error al generar las deducciones del proceso mensual.",
                    -1,
                    false);
            }
        }

        private static CcProcesoMensualGeneraConfigDbModel ObtenerConfiguracionGeneracion(IDbConnection connection, int codInstitucion)
        {
            const string query = @"
                SELECT
                    ISNULL(codigo_aportes_env, '') AS CodigoAportesEnv,
                    ISNULL(codigo_creditos_env, '') AS CodigoCreditosEnv,
                    ISNULL(historico_cobro_envio, 0) AS HistoricoCobroEnvio
                FROM instituciones
                WHERE cod_institucion = @CodInstitucion";

            return connection.QueryFirstOrDefault<CcProcesoMensualGeneraConfigDbModel>(
                query,
                new { CodInstitucion = codInstitucion }) ?? new CcProcesoMensualGeneraConfigDbModel();
        }

        private static bool DebeProcesar(string codigo)
        {
            return !string.Equals(codigo?.Trim(), "NO", StringComparison.OrdinalIgnoreCase);
        }

        private decimal ObtenerFechaAnteriorHistorica(decimal fechaProceso, int CodEmpresa, int historicoCobroEnvio)
        {
            decimal fechaAnterior = fechaProceso;

            for (var i = 1; i <= historicoCobroEnvio + 1; i++)
            {
                fechaAnterior = _mCobroDb.fxFechaProcesoAnterior(CodEmpresa, fechaAnterior);
            }

            return fechaAnterior;
        }

        private static void ProcesarCreditosEnvio(IDbConnection connection, CcProcesoMensualGeneraDeduccionesRequest request, decimal glngFechaCR, decimal fechaAnterior)
        {
            EliminarDetalleEnvioAnterior(connection, request.CodInstitucion, request.FechaProceso, fechaAnterior);

            if (request.UsaPlanillaTransito)
            {
                MProcesoMensualDb.SbCrEnviaConPlanillaTransito(connection, request.CodInstitucion, glngFechaCR, request.FechaProceso);
            }
            else
            {
                EjecutarEnvioCreditos(connection, request);
            }

            InsertarDobleDeduccion(connection, request);
        }

        private static void EliminarDetalleEnvioAnterior(IDbConnection connection, int codInstitucion, decimal fechaProceso, decimal fechaAnterior)
        {
            const string query = @"
                DELETE PRM_ENVIADO_DETALLE
                WHERE (fecPro <= @FechaAnterior OR fecPro = @FechaProceso)
                  AND cod_institucion = @CodInstitucion";

            connection.Execute(query, new
            {
                FechaAnterior = fechaAnterior,
                FechaProceso = fechaProceso,
                CodInstitucion = codInstitucion
            });
        }

        private static void EjecutarEnvioCreditos(IDbConnection connection, CcProcesoMensualGeneraDeduccionesRequest request)
        {
            ActivarCuotas(connection, request);
            EnviarCuotaOrdinaria(connection, request, 0);
            EnviarCuotaOrdinaria(connection, request, 1);
            EnviarCuotaOrdinaria(connection, request, 2);
            EnviaMora(connection, request);
        }

        private static void ActivarCuotas(IDbConnection connection, CcProcesoMensualGeneraDeduccionesRequest request)
        {
            const string query = @"  EXEC spPrmCredito_Activa_Cuotas  @FechaProceso, @CodInstitucion";

            connection.Execute(query, new
            {
                request.FechaProceso,
                request.CodInstitucion
            });
        }

        private static void EnviarCuotaOrdinaria(IDbConnection connection, CcProcesoMensualGeneraDeduccionesRequest request, int paso)
        {
            const string query = @"
                EXEC spPrmCreditoEnviaCuotaOrdinaria  @FechaProceso, @CodInstitucion, @Paso";

            connection.Execute(query, new
            {
                request.FechaProceso,
                request.CodInstitucion,
                Paso = paso
            });
        }

        private static void EnviaMora(IDbConnection connection, CcProcesoMensualGeneraDeduccionesRequest request)
        {
            const string query = @"
                EXEC spPrmCreditoEnviaMora
                    @FechaProceso,
                    @CodInstitucion";

            connection.Execute(query, new
            {
                request.FechaProceso,
                request.CodInstitucion
            });
        }

        private static void InsertarDobleDeduccion(IDbConnection connection, CcProcesoMensualGeneraDeduccionesRequest request)
        {
            const string query = @"
                INSERT INTO PRM_ENVIADO_DETALLE(
                    id_solicitud,
                    codigo,
                    fecpro,
                    cedula,
                    cuota,
                    morosidad,
                    cod_institucion,
                    cargo,
                    poliza,
                    cod_deduccion,
                    Cod_Divisa,
                    IMPORTE)
                SELECT
                    id_solicitud,
                    codigo,
                    fecpro,
                    cedula,
                    cuota,
                    morosidad,
                    cod_institucion,
                    cargo,
                    poliza,
                    cod_deduccion,
                    Cod_Divisa,
                    IMPORTE
                FROM PRM_ENVIADO_DETALLE
                WHERE fecpro = @FechaProceso
                  AND cod_institucion = @CodInstitucion
                  AND cedula IN (
                      SELECT cedula
                      FROM socios
                      WHERE ind_doble_deduccion = 1
                  )";

            connection.Execute(query, new
            {
                request.FechaProceso,
                request.CodInstitucion
            });
        }

        private static void CodificacionEnvio_Ejecutar(IDbConnection connection, CcProcesoMensualGeneraDeduccionesRequest request)
        {
            CodigosSeparacion_Ejecutar(connection, request);
            DeduccionCodificaEnvio_Ejecutar(connection, request);
        }

        private static void CodigosSeparacion_Ejecutar(IDbConnection connection, CcProcesoMensualGeneraDeduccionesRequest request)
        {
            const string query = @" EXEC spPrmProcCodigosSeparacion @CodInstitucion, @FechaProceso";

            connection.Execute(query, new
            {
                request.CodInstitucion,
                request.FechaProceso
            });
        }

        private static void DeduccionCodificaEnvio_Ejecutar(IDbConnection connection, CcProcesoMensualGeneraDeduccionesRequest request)
        {
            const string query = @"EXEC spPrmDeduccionCodifica_Envio @CodInstitucion, @FechaProceso,@Redondeo";

            connection.Execute(query, new
            {
                request.CodInstitucion,
                request.FechaProceso,
                request.Redondeo
            });
        }

        private static void CreditoCambioDeducciones_Ejecutar(IDbConnection connection, CcProcesoMensualGeneraDeduccionesRequest request)
        {
            if (!request.AplicaCambioDeducciones)
            {
                return;
            }
            const string query = @"EXEC spPrm_CreditoCambioDeducciones @CodInstitucion, @FechaProceso, @Usuario";

            connection.Execute(query, new
            {
                request.CodInstitucion,
                request.FechaProceso,
                request.Usuario
            });
        }

        private static void ExclusionPoliticaGeneral_Ejecutar(IDbConnection connection,CcProcesoMensualGeneraDeduccionesRequest request)
        {
            const string query = @"  EXEC spPrm_Credito_Excluye_Casos @CodInstitucion, @FechaProceso";

            connection.Execute(query, new
            {
                request.CodInstitucion,
                request.FechaProceso
            });
        }

        private static CcProcesoMensualPlanillaEnvioConfigDbModel ObtenerConfigPlanillaEnvio( IDbConnection connection, int codInstitucion)
        {
            const string query = @"
                SELECT
                    ISNULL(planilla, '') AS Planilla,
                    ISNULL(Planilla_envio, '') AS PlanillaEnvio,
                    ISNULL(compara_indicador, 0) AS ComparaIndicador,
                    ISNULL(compara_valor, '') AS ComparaValor
                FROM instituciones
                WHERE cod_institucion = @CodInstitucion";

            return connection.QueryFirstOrDefault<CcProcesoMensualPlanillaEnvioConfigDbModel>(
                query,
                new { CodInstitucion = codInstitucion }) ?? new CcProcesoMensualPlanillaEnvioConfigDbModel();
        }

        private static void ComparacionSiAplica_Ejecutar( IDbConnection connection,CcProcesoMensualGeneraDeduccionesRequest request,CcProcesoMensualPlanillaEnvioConfigDbModel config)
        {
            if (config.ComparaIndicador != 1)
            {
                return;
            }

            GeneraPlanillaComp_Ejecutar(
                connection,
                request.CodInstitucion,
                request.FechaProceso);
        }

        private static void GeneraPlanillaComp_Ejecutar(IDbConnection connection, int codInstitucion,decimal fechaProceso)
        {
            const string query = @"EXEC spPrm_Planilla_Compra @CodInstitucion, @FechaProceso";

            connection.Execute(query, new
            {
                CodInstitucion = codInstitucion,
                FechaProceso = fechaProceso
            });
        }

        private static void AjustesPorPlanilla_Ejecutar(IDbConnection connection, CcProcesoMensualGeneraDeduccionesRequest request, CcProcesoMensualPlanillaEnvioConfigDbModel config)
        {
            switch (config.PlanillaEnvio)
            {
                case PlanillaEnvioAya:
                case PlanillaEnvioIna:
                    RedondearPlanilla(connection, request);
                    break;

                case PlanillaEnvioSpa:
                    RedondearPlanillaSpaSiAplica(connection, request, config);
                    break;
            }
        }

        private static void RedondearPlanilla( IDbConnection connection,CcProcesoMensualGeneraDeduccionesRequest request)
        {
            const string query = @"
                UPDATE prm_planilla
                SET monto_actual = ROUND(monto_actual, 0),
                    monto_anterior = ROUND(monto_anterior, 0)
                WHERE proceso = @FechaProceso
                  AND cod_institucion = @CodInstitucion";

            connection.Execute(query, new
            {
                request.FechaProceso,
                request.CodInstitucion
            });
        }

        private static void RedondearPlanillaSpaSiAplica(IDbConnection connection,CcProcesoMensualGeneraDeduccionesRequest request,CcProcesoMensualPlanillaEnvioConfigDbModel config)
        {
            if (config.ComparaIndicador != 0)
            {
                return;
            }

            const string query = @"
                UPDATE prm_planilla
                SET monto_anterior = 0,
                    monto_actual = ROUND(monto_actual, 0),
                    Movimiento = 'I'
                WHERE proceso = @FechaProceso
                  AND cod_institucion = @CodInstitucion";

            connection.Execute(query, new
            {
                request.FechaProceso,
                request.CodInstitucion
            });
        }

        private static void ActualizarEstadoGeneracion( IDbConnection connection, int codInstitucion)
        {
            const string query = @"
                UPDATE instituciones
                SET pr_genera = 1
                WHERE cod_institucion = @CodInstitucion";

            connection.Execute(query, new { CodInstitucion = codInstitucion });
        }

        private void RegistrarBitacoraGeneracion( IDbConnection connection,int codEmpresa,  CcProcesoMensualGeneraDeduccionesRequest request)
        {
            var documento = request.UsaPlanillaTransito
                ? "Transito.: Si"
                : "Transito.: No";

            if (request.AplicaCambioDeducciones)
            {
                documento = $"{documento}  - Cambios: Sí";
            }

            
            MProcesoMensualDb.SbBitacoraPlanilla(connection,
                                                    new CcProcesoMensualBitacoraPlanillaDto
                                                    {
                                                        Transaccion = "02",
                                                        CodInstitucion = request.CodInstitucion,
                                                        Proceso = request.FechaProceso,
                                                        Gestion = "E",
                                                        Usuario = request.Usuario,
                                                        Documento = documento
                                                    });

            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = request.Usuario,
                DetalleMovimiento = $"Planilla Genera Deducciones Inst: {request.CodInstitucion}",
                Movimiento = "Aplica - WEB",
                Modulo = vModulo
            });   
        }
       
        private sealed class CcProcesoMensualGeneraConfigDbModel
        {
            public string CodigoAportesEnv { get; set; } = string.Empty;
            public string CodigoCreditosEnv { get; set; } = string.Empty;
            public int HistoricoCobroEnvio { get; set; } = 0;
        }

        private sealed class CcProcesoMensualPlanillaEnvioConfigDbModel
        {
            public string Planilla { get; set; } = string.Empty;
            public string PlanillaEnvio { get; set; } = string.Empty;
            public int ComparaIndicador { get; set; } = 0;
            public string ComparaValor { get; set; } = string.Empty;
        }

    }
}
