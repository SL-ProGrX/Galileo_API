
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB
{
    public class CcProcesoMensualReportesRecepcionDb
    {

        private readonly PortalDB _portalDb;
        private readonly MProGrxMain _mProGrx;
        public CcProcesoMensualReportesRecepcionDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _mProGrx = new MProGrxMain(config);
        }
        public ErrorDto<CcProcesoMensualReporteModel> CcProcesoMensual_CargadoNoLocalizadoReporte_Obtener(int codEmpresa, string usuario, decimal vFecha)
        {
            try
            {
                var globalesResp = _mProGrx.sbSifParametrosInicializa(codEmpresa, usuario);
                var fechaTexto = MCobroDb.fxFechaProcesoFormat(vFecha);

                var response = new CcProcesoMensualReporteModel
                {
                    NombreReporte = "Sys_Planilla_Cargada_NoLocalizado",
                    Titulo = "PROCESO MENSUAL - CARGADO DE INFORMACION",
                    Formulas = new Dictionary<string, object>
                    {
                        { "Empresa", globalesResp?.Result?.GstrNombreEmpresa ?? string.Empty },
                        { "Fecha", fechaTexto },
                        { "usuario", usuario },
                        { "institucion", globalesResp?.Result?.GNombreInstitucion ?? string.Empty }
                    },
                    SelectionFormula =
                        "vPrmCargadoPersonasNoEncontradas.FECHA_PROCESO = " + vFecha +
                        " AND vPrmCargadoPersonasNoEncontradas.COD_INSTITUCION = " + globalesResp?.Result?.GInstitucion
                };

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse<CcProcesoMensualReporteModel>(
                    "Error al obtener los parámetros del reporte de personas no localizadas.",
                    -1,
                    new CcProcesoMensualReporteModel());
            }
        }
    }
}

