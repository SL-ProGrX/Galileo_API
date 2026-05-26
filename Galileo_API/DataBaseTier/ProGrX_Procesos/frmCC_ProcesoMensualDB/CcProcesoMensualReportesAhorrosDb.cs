using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB
{
    public class CcProcesoMensualReportesAhorrosDb
    {
        private readonly PortalDB _portalDb;
        private readonly MProGrxMain _mProGrx;
        public CcProcesoMensualReportesAhorrosDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _mProGrx = new MProGrxMain(config);
        }

        public ErrorDto<CcProcesoMensualReporteModel> CcProcesoMensual_AhorrosAplicaAhorroRep_Obtener(int codEmpresa, string usuario, decimal vFecha)
        {
            return ObtenerReporteAhorro(
                new CcProcesoMensualAhorroReporteDefinicion
                {
                    CodEmpresa = codEmpresa,
                    Usuario = usuario,
                    FechaProceso = vFecha,
                    NombreReporte = "Sys_Planilla_PatAplicados",
                    Titulo = "Reportes Módulo de Ahorros",
                    EstadoExiste = "S",
                    MensajeError = "Error al obtener los parámetros del reporte de aportes aplicados."
                });
        }
        public ErrorDto<CcProcesoMensualReporteModel> CcProcesoMensual_AhorrosDevolucionesRep_Obtener(int codEmpresa, string usuario, decimal vFecha)
        {
            return ObtenerReporteAhorro(
                new CcProcesoMensualAhorroReporteDefinicion
                {
                    CodEmpresa = codEmpresa,
                    Usuario = usuario,
                    FechaProceso = vFecha,
                    NombreReporte = "Sys_Planilla_PatListados",
                    Titulo = "DEVOLUCIONES (CASOS EX-SOCIOS)",
                    EstadoExiste = "D",
                    MensajeError = "Error al obtener los parámetros del reporte de devoluciones."
                });
        }
        public ErrorDto<CcProcesoMensualReporteModel> CcProcesoMensual_AhorrosInconsistenciasRep_Obtener(int codEmpresa, string usuario, decimal vFecha)
        {
            return ObtenerReporteAhorro(
                new CcProcesoMensualAhorroReporteDefinicion
                {
                    CodEmpresa = codEmpresa,
                    Usuario = usuario,
                    FechaProceso = vFecha,
                    NombreReporte = "Sys_Planilla_PatListados",
                    Titulo = "INCONSISTENCIAS DE APORTES",
                    EstadoExiste = "N",
                    MensajeError = "Error al obtener los parámetros del reporte de inconsistencias de aportes."
                });
        }
        private ErrorDto<CcProcesoMensualReporteModel> ObtenerReporteAhorro(CcProcesoMensualAhorroReporteDefinicion definicion)
        {
            using var connection = DbHelper.OpenConnection(
                _portalDb,
                definicion.CodEmpresa);

            var contexto = CrearContextoReporteAhorro(
                connection,
                definicion);

            try
            {
                var response = CrearReporteAhorro(
                    definicion,
                    contexto);

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception)
            {
                return CrearErrorReporte(definicion.MensajeError);
            }
        }
        private CcProcesoMensualAhorroReporteContexto CrearContextoReporteAhorro(IDbConnection connection, CcProcesoMensualAhorroReporteDefinicion definicion)
        {
            var globalesResp = _mProGrx.sbSifParametrosInicializa(definicion.CodEmpresa, definicion.Usuario);

            var codInstitucion = globalesResp?.Result?.GInstitucion ?? 0;

            return new CcProcesoMensualAhorroReporteContexto
            {
                CodInstitucion = codInstitucion,
                NombreEmpresa = globalesResp?.Result?.GstrNombreEmpresa ?? string.Empty,
                NombreInstitucion = globalesResp?.Result?.GNombreInstitucion ?? string.Empty,
                Parametros = ObtenerParametrosAhorroReporte(connection, codInstitucion)
            };
        }
        private static CcProcesoMensualReporteModel CrearReporteAhorro(CcProcesoMensualAhorroReporteDefinicion definicion, CcProcesoMensualAhorroReporteContexto contexto)
        {
            return new CcProcesoMensualReporteModel
            {
                NombreReporte = definicion.NombreReporte,
                Titulo = definicion.Titulo,
                Fecha = MCobroDb.fxFechaProcesoFormat(definicion.FechaProceso),
                Empresa = contexto.NombreEmpresa,
                Usuario = definicion.Usuario,
                Porcentaje = contexto.Parametros.Porcentaje,
                PorcAhorro = contexto.Parametros.PorcAhorro,
                Institucion = contexto.NombreInstitucion,
                Filtros = CrearFiltroSociosTemp(
                    definicion,
                    contexto.CodInstitucion)
            };
        }
        private static string CrearFiltroSociosTemp(CcProcesoMensualAhorroReporteDefinicion definicion, int codInstitucion)
        {
            return $"SOCIOSTEMP.EXISTE = '{definicion.EstadoExiste}'" +
                $" AND SOCIOSTEMP.FECHAPROC = {definicion.FechaProceso}" +
                $" AND SOCIOSTEMP.COD_INSTITUCION = {codInstitucion}";
        }
        private static CcProcesoMensualAhorroReporteDbModel ObtenerParametrosAhorroReporte(IDbConnection connection, int codInstitucion)
        {
            const string query = @"
        SELECT
            ISNULL(porc_aporte, 0) / 100 AS Porcentaje,
            ISNULL(porc_ahorro, 0) / 100 AS PorcAhorro
        FROM instituciones
        WHERE cod_institucion = @CodInstitucion";

            return connection.QueryFirstOrDefault<CcProcesoMensualAhorroReporteDbModel>(
                query,
                new { CodInstitucion = codInstitucion }) ?? new CcProcesoMensualAhorroReporteDbModel();
        }
        private static ErrorDto<CcProcesoMensualReporteModel> CrearErrorReporte(string mensaje)
        {
            return DbHelper.CreateErrorResponse<CcProcesoMensualReporteModel>(
                mensaje,
                -1,
                new CcProcesoMensualReporteModel());
        }

        private sealed class CcProcesoMensualAhorroReporteDefinicion
        {
            public int CodEmpresa { get; init; } = 0;
            public string Usuario { get; init; } = string.Empty;
            public decimal FechaProceso { get; init; } = 0;
            public string NombreReporte { get; init; } = string.Empty;
            public string Titulo { get; init; } = string.Empty;
            public string EstadoExiste { get; init; } = string.Empty;
            public string MensajeError { get; init; } = string.Empty;
        }

        private sealed class CcProcesoMensualAhorroReporteContexto
        {
            public int CodInstitucion { get; init; } = 0;
            public string NombreEmpresa { get; init; } = string.Empty;
            public string NombreInstitucion { get; init; } = string.Empty;
            public CcProcesoMensualAhorroReporteDbModel Parametros { get; init; } = new();
        }

        private sealed class CcProcesoMensualAhorroReporteDbModel
        {
            public decimal Porcentaje { get; set; } = 0;
            public decimal PorcAhorro { get; set; } = 0;
        }
    }
}
