using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels;


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

        public ErrorDto<CcProcesoMensualReporteModel> CcProcesoMensual_AhorrosAplicaAhorroRep_Obtener( int codEmpresa,string usuario , decimal vFecha)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
            var globalesResp = _mProGrx.sbSifParametrosInicializa(codEmpresa, usuario); 
            try
            {
                const string query = @"
                    SELECT
                        ISNULL(porc_aporte, 0) / 100 AS Porcentaje,
                        ISNULL(porc_ahorro, 0) / 100 AS PorcAhorro
                    FROM instituciones
                    WHERE cod_institucion = @CodInstitucion";

                var parametros = connection.QueryFirstOrDefault<CcProcesoMensualAhorroReporteDbModel>(
                    query,
                    new { CodInstitucion = globalesResp?.Result?.GInstitucion ?? 0 }) ?? new CcProcesoMensualAhorroReporteDbModel();

                var response = new CcProcesoMensualReporteModel
                {
                    NombreReporte = "Sys_Planilla_PatAplicados",
                    Titulo = "Reportes Módulo de Ahorros",
                    Formulas = new Dictionary<string, object>
                    {
                        { "Fecha", MCobroDb.fxFechaProcesoFormat(vFecha) },
                        { "Empresa", globalesResp?.Result?.GstrNombreEmpresa ?? string.Empty },
                        { "usuario", usuario },
                        { "Porcentaje", parametros.Porcentaje },
                        { "PorcAhorro", parametros.PorcAhorro },
                        { "institucion", globalesResp?.Result?.GNombreInstitucion ?? string.Empty }
                    },
                    SelectionFormula =
                        "SOCIOSTEMP.EXISTE = 'S'" +
                        " AND SOCIOSTEMP.FECHAPROC = " + vFecha +
                        " AND SOCIOSTEMP.COD_INSTITUCION = " + (globalesResp?.Result?.GInstitucion ?? 0)
                };

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse<CcProcesoMensualReporteModel>(
                    "Error al obtener los parámetros del reporte de aportes aplicados.",
                    -1,
                    new CcProcesoMensualReporteModel());
            }
        }
         
        private sealed class CcProcesoMensualAhorroReporteDbModel
        {
            public decimal Porcentaje { get; set; } = 0;
            public decimal PorcAhorro { get; set; } = 0;
        }
    }
}
