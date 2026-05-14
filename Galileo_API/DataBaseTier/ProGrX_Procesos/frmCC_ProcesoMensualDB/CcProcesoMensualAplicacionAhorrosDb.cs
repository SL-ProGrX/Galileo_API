using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security; 
using System.Data; 
using static Galileo_API.Models.MProcesoMensualModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB
{
    public class CcProcesoMensualAplicacionAhorrosDb
    {
        private readonly PortalDB _portalDb;
        private readonly MProGrxMain _mProGrx;
        private readonly int vModulo = 3;
        private readonly MSecurityMainDb _Security_MainDB;
        public CcProcesoMensualAplicacionAhorrosDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _mProGrx = new MProGrxMain(config);
            _Security_MainDB = new MSecurityMainDb(config);
        }
 

        public ErrorDto<bool> CcProcesoMensual_Ahorros_Aplicar(  int codEmpresa,int codInstitucion,decimal fechaProceso,string usuario)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
            DateTime vFecha = _mProGrx.fxFechaServidor(codEmpresa, 0);
            try
            {
                EjecutarAplicacionAportes(connection, codInstitucion, fechaProceso, usuario);
                ProcesarDevolucionesAhorros(connection, codInstitucion, fechaProceso, vFecha, usuario);
                ActualizarEstadoAplicacionAhorros(connection, codInstitucion);
                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"PRM - Aplicación de Aportes Inst: {codInstitucion}",
                    Movimiento = "Aplica - WEB",
                    Modulo = vModulo
                });
                MProcesoMensualDb.SbBitacoraPlanilla(  connection,  "05",codInstitucion,fechaProceso,"R", usuario);   
                

                return DbHelper.CreateOkResponse(true);
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse<bool>(
                    "Error al aplicar los aportes del proceso mensual.",
                    -1,
                    false);
            }
        }

        public ErrorDto<bool> CcProcesoMensual_AhorrosInconsistencias_Marcar(int codEmpresa, int codInstitucion)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                ActualizarEstadoInconsistenciasAhorros(connection, codInstitucion);

                // Pendiente: Bitacora("Aplica", ...)
                // Pendiente: sbBitacoraPlanilla("06", ...)

                return DbHelper.CreateOkResponse(true);
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse<bool>(
                    "Error al marcar las inconsistencias de ahorros.",
                    -1,
                    false);
            }
        }

        public ErrorDto<bool> CcProcesoMensual_AhorrosDevoluciones_Marcar(int codEmpresa, int codInstitucion)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                ActualizarEstadoDevolucionesAhorros(connection, codInstitucion);

                // Pendiente: Bitacora("Aplica", ...)
                // Pendiente: sbBitacoraPlanilla("07", ...)

                return DbHelper.CreateOkResponse(true);
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse<bool>(
                    "Error al marcar las devoluciones de ahorros.",
                    -1,
                    false);
            }
        }

        private static void EjecutarAplicacionAportes( IDbConnection connection, int codInstitucion,decimal fechaProceso,string usuario)
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

        private static void EjecutarPasoAplicacionAportes(IDbConnection connection,string query,decimal fechaProceso,int codInstitucion,string usuario,int paso)
        {
            connection.Execute(query, new
            {
                FechaProceso = fechaProceso,
                CodInstitucion = codInstitucion,
                Usuario = usuario,
                Paso = paso
            });
        }

        private static void ProcesarDevolucionesAhorros( IDbConnection connection,int codInstitucion,decimal fechaProceso,DateTime fecha,string usuario)
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

        private static List<CcProcesoMensualSocioDevolucionDbModel> ObtenerSociosDevolucion(IDbConnection connection,int codInstitucion,decimal fechaProceso)
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

        private static void EjecutarDevolucionSiAplica( IDbConnection connection,CcProcesoMensualAhorroParametrosDbModel parametros,CcProcesoMensualSocioDevolucionDbModel socio,int codInstitucion,decimal fechaProceso,string documento, DateTime fecha)
        {
            if (socio.Monto > 0)
            {
                EjecutarDevolucionFondo(
                    connection,
                    parametros,
                    socio.Cedula,
                    socio.Monto,
                    codInstitucion,
                    fechaProceso,
                    documento,
                    parametros.FndApPlan,
                    "A",
                    fecha);
            }

            if (socio.Aporte > 0)
            {
                EjecutarDevolucionFondo(
                    connection,
                    parametros,
                    socio.Cedula,
                    socio.Aporte,
                    codInstitucion,
                    fechaProceso,
                    documento,
                    parametros.FndApPlanP,
                    "P",
                    fecha);
            }
        }

        private static void EjecutarDevolucionFondo( IDbConnection connection,CcProcesoMensualAhorroParametrosDbModel parametros,string cedula, decimal monto,int codInstitucion,decimal fechaProceso,string documento,string plan,string tipo, DateTime fecha)
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
                CodInstitucion = codInstitucion,
                FechaProceso = fechaProceso,
                Operadora = parametros.FndApOperadora,
                Plan = plan.Trim(),
                Cedula = cedula.Trim(),
                Monto = monto,
                Documento = documento,
                CuentaInconsistencia = parametros.CtaInconsistencia.Trim(),
                Tipo = tipo,
                Fecha = fecha
            });
        }

        private static void EliminarSociosTempDevolucion(IDbConnection connection,int codInstitucion,decimal fechaProceso)
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

        private static void ActualizarEstadoAplicacionAhorros(IDbConnection connection,int codInstitucion)
        {
            const string query = @"
                UPDATE instituciones
                SET pr_apAplica = 1
                WHERE cod_institucion = @CodInstitucion";

            connection.Execute(query, new { CodInstitucion = codInstitucion });
        }

        private static void ActualizarEstadoInconsistenciasAhorros(
            IDbConnection connection,
            int codInstitucion)
        {
            const string query = @"
                UPDATE instituciones
                SET pr_apInco = 1
                WHERE cod_institucion = @CodInstitucion";

            connection.Execute(query, new { CodInstitucion = codInstitucion });
        }

        private static void ActualizarEstadoDevolucionesAhorros(
            IDbConnection connection,
            int codInstitucion)
        {
            const string query = @"
                UPDATE instituciones
                SET pr_apDev = 1
                WHERE cod_institucion = @CodInstitucion";

            connection.Execute(query, new { CodInstitucion = codInstitucion });
        }

        private sealed class CcProcesoMensualAhorroParametrosDbModel
        {
            public int FndApAplica { get; set; }
            public int FndApOperadora { get; set; }
            public string FndApPlan { get; set; } = string.Empty;
            public string FndApPlanP { get; set; } = string.Empty;
            public string CtaInconsistencia { get; set; } = string.Empty;
        }

        private sealed class CcProcesoMensualSocioDevolucionDbModel
        {
            public string Cedula { get; set; } = string.Empty;
            public decimal Monto { get; set; }
            public decimal Aporte { get; set; }
        }
    }
}
 