using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Comites;
using Galileo_API.Models.ProGrX_Comites;

namespace Galileo_API.BusinessLogic.ProGrX_Comites
{
    public class FrmAfCdLiquidacionesBl
    {
        private readonly FrmAfCdLiquidacionesDb _db;

        public FrmAfCdLiquidacionesBl(IConfiguration config)
        {
            _db = new FrmAfCdLiquidacionesDb(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AfCdComites_Lista_Obtener(int codEmpresa)
        {
            return _db.AfCdComites_Lista_Obtener(codEmpresa);
        }

        public ErrorDto<string?> AfCdComite_Descripcion_Obtener(int codEmpresa, string codComite)
        {
            return _db.AfCdComite_Descripcion_Obtener(codEmpresa, codComite);
        }

        public ErrorDto<int> AfCdLiquidaciones_Pendientes_Obtener(int codEmpresa, string codComite)
        {
            return _db.AfCdLiquidaciones_Pendientes_Obtener(codEmpresa, codComite);
        }

        public ErrorDto<List<AfCdOperacionData>> AfCdOperaciones_Lista_Obtener(int codEmpresa, string codComite)
        {
            return _db.AfCdOperaciones_Lista_Obtener(codEmpresa , codComite);
        }
    }
}
