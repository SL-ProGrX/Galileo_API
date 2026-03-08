using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Procesos;

namespace Galileo_API.BusinessLogic.ProGrX_Procesos
{
    public class FrmCcProcesoMensualPlanillaBL
    {
        private readonly FrmCcProcesoMensualPlanillaDB _db;

        public FrmCcProcesoMensualPlanillaBL(IConfiguration config)
        {
            _db = new FrmCcProcesoMensualPlanillaDB(config);
        }
        public ErrorDto<TablasListaGenericaModel> CC_ProcesoMensualPlanilla_Lista_Obtener(int CodEmpresa, string Parametros)
        {
            return _db.CC_ProcesoMensualPlanilla_Lista_Obtener(CodEmpresa, Parametros);
        }
        public ErrorDto<TablasListaGenericaModel> CC_ProcesoMensualPlanilla_Lista_Export(int CodEmpresa, string Parametros)
        {
            return _db.CC_ProcesoMensualPlanilla_Lista_Export(CodEmpresa, Parametros);
        }
    }
}