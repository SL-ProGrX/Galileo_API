using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using Galileo.DataBaseTier.ProGrX.Fondos;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndBancosXBl
    {
        private readonly FrmFndBancosXDb _db;

        public FrmFndBancosXBl(IConfiguration config)
        {
            _db = new FrmFndBancosXDb(config);
        }

        public ErrorDto<List<FndBancosXModel>> BancosX_Obtener(int codEmpresa)
        {
            return _db.BancosX_Obtener(codEmpresa);
        }

        public ErrorDto BancosX_Insertar(int codEmpresa)
        {
            return _db.BancosX_Insertar(codEmpresa);
        }

        public ErrorDto BancosX_Actualizar(int codEmpresa, FndBancosXUpdateParam param)
        {
            return _db.BancosX_Actualizar(codEmpresa, param);
        }
    }
}