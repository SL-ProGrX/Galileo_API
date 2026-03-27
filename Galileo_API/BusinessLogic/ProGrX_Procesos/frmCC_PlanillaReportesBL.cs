using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Procesos;
using Galileo.Models.ProGrX_Procesos;

namespace Galileo_API.BusinessLogic.ProGrX_Procesos
{
    public class FrmCCPlanillaReportesBL
    {
        private readonly FrmCCPlanillaReportesDB Db;

        public FrmCCPlanillaReportesBL(IConfiguration config)
        {
            Db = new FrmCCPlanillaReportesDB(config);
        }
        public ErrorDto<List<CcPlanillaReporteCatalogoDto>> CC_PlanillaReportes_Catalogo_Obtener(int CodEmpresa)
        {
            return Db.CC_PlanillaReportes_Catalogo_Obtener(CodEmpresa);
        }
        public static ErrorDto<List<CcPlanillaReporteTipoDto>> CC_PlanillaReportes_TiposReporte_Obtener(int CodEmpresa, string? codigoOpcion)
        {
            return FrmCCPlanillaReportesDB.CC_PlanillaReportes_TiposReporte_Obtener(CodEmpresa, codigoOpcion);
        }
        public ErrorDto<CcPlanillaReportesParametrosInicialesDto> CC_PlanillaReportes_ParametrosIniciales_Obtener(int CodEmpresa,int codInstitucion)
        {
            return Db.CC_PlanillaReportes_ParametrosIniciales_Obtener(CodEmpresa, codInstitucion);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> CC_Instituciones_Dropdown_Obtener(int CodEmpresa)
        {
            return Db.CC_Instituciones_Dropdown_Obtener(CodEmpresa);
        }
        public ErrorDto<CcPlanillaInstitucionInfoDto> CC_PlanillaReportes_Institucion_Obtener(int CodEmpresa, int codInstitucion)
        {
            return Db.CC_PlanillaReportes_Institucion_Obtener(CodEmpresa, codInstitucion);
        }
        public ErrorDto<CcPlanillaProcesoScrollDto> CC_PlanillaReportes_Proceso_Scroll_Obtener(int CodEmpresa, int scrollCode, decimal procesoActual)
        {
            return Db.CC_PlanillaReportes_Proceso_Scroll_Obtener(CodEmpresa, scrollCode, procesoActual);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> CC_PlanillaReportes_Lineas_Dropdown_Obtener(int CodEmpresa,decimal proceso,int? codInstitucion,bool todasInstituciones)
        {
            return Db.CC_PlanillaReportes_Lineas_Dropdown_Obtener(CodEmpresa, proceso, codInstitucion, todasInstituciones);
        }
        public static ErrorDto<List<CcPlanillaTipoCobroDto>> CC_PlanillaReportes_TiposCobro_Obtener(int CodEmpresa)
        {
            return FrmCCPlanillaReportesDB.CC_PlanillaReportes_TiposCobro_Obtener(CodEmpresa);
        }
    }
}