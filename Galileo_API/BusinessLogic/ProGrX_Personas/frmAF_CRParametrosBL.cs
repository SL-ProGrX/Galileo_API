using Galileo.DataBaseTier.ProGrX_Personas;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;

namespace Galileo.BusinessLogic.ProGrX_Personas
{
    public class FrmAFCrParametrosBL
    {
        private readonly FrmAFCrParametrosDB _db;

        public FrmAFCrParametrosBL(IConfiguration config)
        {
            _db = new FrmAFCrParametrosDB(config);
        }

        public ErrorDto<List<AfCrParametrosData>> AF_CRParametros_Obtener(int CodEmpresa)
        {
            return _db.AF_CRParametros_Obtener(CodEmpresa);
        }

        public ErrorDto AF_CRParametros_Guardar(int CodEmpresa, string usuario, AfCrParametrosData parametros)
        {
            return _db.AF_CRParametros_Guardar(CodEmpresa, usuario, parametros);
        }
    }
}
