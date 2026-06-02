using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmAFReingresosBL
    {
        private readonly FrmAFReingresosDB _db;

        public FrmAFReingresosBL(IConfiguration config)
        {
            _db = new FrmAFReingresosDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_PromotoresReingreso_Obtener(int CodEmpresa)
        {
            return _db.AF_PromotoresReingreso_Obtener(CodEmpresa);
        }

        public ErrorDto AF_Persona_ActivarYVincular(int CodEmpresa, string request)
        {
            return _db.AF_Persona_ActivarYVincular(CodEmpresa, request);

        }
    }
}