using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;
using Galileo_API.DataBaseTier.ProGrX_Nucleo;

namespace Galileo_API.BusinessLogic.ProGrX_Nucleo
{
    public class FrmSifGlobalesBL(IConfiguration config)
    {
        private readonly FrmSifGlobalesDB _db = new(config);

        public ErrorDto<List<SifVariableGlobalDto>> Obtener() => _db.Obtener();

        public ErrorDto Guardar(int codEmpresa, string usuario, SifVariableGlobalDto dato) =>
            _db.Guardar(codEmpresa, usuario, dato);
    }
}
