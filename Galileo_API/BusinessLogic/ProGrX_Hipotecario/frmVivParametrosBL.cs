using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Hipotecario;
using Galileo_API.Models.ProGrX_Hipotecario;

namespace Galileo_API.BusinessLogic.ProGrX_Hipotecario
{
    public class FrmVivParametrosBl
    {
        private readonly FrmVivParametrosDb _db;

        public FrmVivParametrosBl(IConfiguration config)
            => _db = new FrmVivParametrosDb(config);

        public ErrorDto<List<VivParametrosData>> VivParametros_Obtener(int codEmpresa)
        {
            return _db.VivParametros_Obtener(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> VivTiposDesembolsos_Obtener(int codEmpresa)
        {
            return _db.VivTiposDesembolsos_Obtener(codEmpresa);
        }

        public ErrorDto VivParametros_Guardar(int codEmpresa, string usuario, VivParametrosData request)
        {
            return _db.VivParametros_Guardar(codEmpresa, usuario, request);
        }
    }
}
