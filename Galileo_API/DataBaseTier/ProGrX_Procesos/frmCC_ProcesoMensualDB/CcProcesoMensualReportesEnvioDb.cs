using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB
{
    public class CcProcesoMensualReportesEnvioDb
    {

        private readonly MProGrxMain _mProGrx;
        public CcProcesoMensualReportesEnvioDb(IConfiguration config)
        {
            _mProGrx = new MProGrxMain(config);
        }
        public ErrorDto<CcProcesoMensualReporteModel> CcProcesoMensual_ReporteGeneracionF02_Obtener(int codEmpresa, string usuario, decimal vFecha)
        {
            try
            {
                var globalesResp = _mProGrx.sbSifParametrosInicializa(codEmpresa, usuario);
                var fechaTexto = MCobroDb.fxFechaProcesoFormat(vFecha);

                var response = new CcProcesoMensualReporteModel
                {
                    NombreReporte = "Sys_Planilla_Generada",
                    Titulo = "Planillas - Información Generada",
                    Formulas = new Dictionary<string, object>
                    {
                        { "Empresa", globalesResp?.Result?.GstrNombreEmpresa ?? string.Empty },
                        { "Fecha", fechaTexto },
                        { "usuario", usuario },
                        { "institucion", globalesResp?.Result?.GNombreInstitucion ?? string.Empty }
                    },
                    SelectionFormula =
                        "PRM_PLANILLA.PROCESO = " + vFecha +
                        " AND PRM_PLANILLA.COD_INSTITUCION = " + globalesResp?.Result?.GInstitucion
                };

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse<CcProcesoMensualReporteModel>(
                    "Error al obtener los parámetros del reporte de planillas generadas.",
                    -1,
                    new CcProcesoMensualReporteModel());
            }
        }
    
    }
}
