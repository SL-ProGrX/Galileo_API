using Galileo.DataBaseTier.ProGrX.Clientes;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmAfActaAfiliacionBL
    {
        private readonly FrmAfActaAfiliacionDB _db;
        public FrmAfActaAfiliacionBL(IConfiguration config)
        {
            _db = new FrmAfActaAfiliacionDB(config);
        }

        public ErrorDto<long> AF_ActaAfiliacio_Obtener(int CodEmpresa, string usuario)
        {
            return _db.AF_ActaAfiliacio_Obtener(CodEmpresa, usuario);
        }
    }
}