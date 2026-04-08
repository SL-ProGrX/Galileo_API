using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Procesos;
using Galileo_API.Models.ProGrX_Procesos;

namespace Galileo_API.BusinessLogic.ProGrX_Procesos
{
    public class FrmCcPlanillaCtaCorreccionBL
    {
        private readonly FrmCcPlanillaCtaCorreccionDB Db;

        public FrmCcPlanillaCtaCorreccionBL(IConfiguration config)
        {
            Db = new FrmCcPlanillaCtaCorreccionDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CC_PlanillaCtaCorreccion_Instituciones_Dropdown_Obtener(int CodEmpresa)
        {
            return Db.CC_PlanillaCtaCorreccion_Instituciones_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto<CcPlanillaCtaCorreccionProcesoScrollDto> CC_PlanillaCtaCorreccion_Proceso_Scroll_Obtener(int CodEmpresa, int scrollCode, decimal procesoActual)
        {
            return Db.CC_PlanillaCtaCorreccion_Proceso_Scroll_Obtener(CodEmpresa, scrollCode, procesoActual);
        }

        public ErrorDto<List<CcPlanillaCtaCorreccionPersonaF4Dto>> CC_PlanillaCtaCorreccion_Personas_F4_Obtener(int CodEmpresa, string? texto)
        {
            return Db.CC_PlanillaCtaCorreccion_Personas_F4_Obtener(CodEmpresa, texto);
        }

        public ErrorDto<CcPlanillaCtaCorreccionListaResult> CC_PlanillaCtaCorreccion_Lista_Obtener(int CodEmpresa, CcPlanillaCtaCorreccionListaRequest parametros)
        {
            return Db.CC_PlanillaCtaCorreccion_Lista_Obtener(CodEmpresa, parametros);
        }

        public ErrorDto<CcPlanillaCtaCorreccionListaResult> CC_PlanillaCtaCorreccion_Lista_Export(int CodEmpresa, CcPlanillaCtaCorreccionListaRequest parametros)
        {
            parametros ??= new CcPlanillaCtaCorreccionListaRequest();
            parametros.filtros ??= new FiltrosLazyLoadData();
            parametros.filtros.pagina = 0;
            parametros.filtros.paginacion = 0;

            return Db.CC_PlanillaCtaCorreccion_Lista_Export(CodEmpresa, parametros);
        }

        public ErrorDto CC_PlanillaCtaCorreccion_Cuota_Actualizar(int CodEmpresa, CcPlanillaCtaCorreccionActualizarCuotaRequest parametros)
        {
            return Db.CC_PlanillaCtaCorreccion_Cuota_Actualizar(CodEmpresa, parametros);
        }
    }
}