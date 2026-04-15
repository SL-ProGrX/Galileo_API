using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Hipotecario;
using Galileo_API.Models.ProGrX_Hipotecario;

namespace Galileo_API.BusinessLogic.ProGrX_Hipotecario
{
    public class FrmVivTiposDesembolsosBl
    {
        private readonly FrmVivTiposDesembolsosDb _db;

        public FrmVivTiposDesembolsosBl(IConfiguration config)
            => _db = new FrmVivTiposDesembolsosDb(config);

        public ErrorDto<List<VivTiposDesembolsosData>> VivTiposDesembolsos_Obtener(int codEmpresa)
        {
            return _db.VivTiposDesembolsos_Obtener(codEmpresa);
        }

        public ErrorDto VivTiposDesembolsos_Guardar(int codEmpresa, int operacion, VivTiposDesembolsosData request)
        {
            return _db.VivTiposDesembolsos_Guardar(codEmpresa, operacion, request);
        }

        public ErrorDto VivTiposDesembolsos_Eliminar(int codEmpresa, string codigo, string usuario)
        {
            return _db.VivTiposDesembolsos_Eliminar(codEmpresa, codigo, usuario);
        }
    }
}
