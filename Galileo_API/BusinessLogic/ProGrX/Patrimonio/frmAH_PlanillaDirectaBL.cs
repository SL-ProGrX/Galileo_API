using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Patrimonio;
using Galileo_API.Models.ProGrX;

namespace Galileo_API.BusinessLogic.ProGrX.Patrimonio
{
    public class FrmAhPlanillaDirectaBL
    {
        private readonly FrmAhPlanillaDirectaDB _db;

        public FrmAhPlanillaDirectaBL(IConfiguration config)
        {
            _db = new FrmAhPlanillaDirectaDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Ah_PlanillaDirecta_Instituciones_Obtener(
            int codEmpresa)
        {
            return _db.Ah_PlanillaDirecta_Instituciones_Obtener(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Ah_PlanillaDirecta_Periodos_Obtener(
            int codEmpresa)
        {
            return _db.Ah_PlanillaDirecta_Periodos_Obtener(codEmpresa);
        }

        public ErrorDto<string> Ah_PlanillaDirecta_Comprobante_Obtener(
            int codEmpresa,
            int codInstitucion,
            int proceso,
            string tipoAporte)
        {
            return _db.Ah_PlanillaDirecta_Comprobante_Obtener(
                codEmpresa,
                codInstitucion,
                proceso,
                tipoAporte);
        }

        public ErrorDto<List<FrmAhPlanillaDirectaCargadoDto>> Ah_PlanillaDirecta_Cargado(
            int codEmpresa,
            FrmAhPlanillaDirectaCargadoRequest request)
        {
            return _db.Ah_PlanillaDirecta_Cargado(codEmpresa, request);
        }

        public ErrorDto<List<FrmAhPlanillaDirectaInconsistenciaDto>> Ah_PlanillaDirecta_Inconsistencias_Obtener(
            int codEmpresa,
            string numDoc)
        {
            return _db.Ah_PlanillaDirecta_Inconsistencias_Obtener(codEmpresa, numDoc);
        }

        public ErrorDto<FrmAhPlanillaDirectaProcesarResponse> Ah_PlanillaDirecta_Procesar(
            int codEmpresa,
            FrmAhPlanillaDirectaProcesarRequest request)
        {
            return _db.Ah_PlanillaDirecta_Procesar(codEmpresa, request);
        }
    }
}
