
using Galileo_API.DataBaseTier.ProGrX_EstudioCrd;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;

namespace Galileo_API.BusinessLogic.ProGrX_EstudioCrd
{
    public class FrmPreaTiposExtrasBl
    {
        private readonly FrmPreaTiposExtrasDb _db;

        public FrmPreaTiposExtrasBl(IConfiguration config)
            => _db = new FrmPreaTiposExtrasDb(config);

        public ErrorDto<List<CrdPreaTiposExtrasData>> CrPreaTiposExtras_Obtener(int codEmpresa)
        {
            return _db.CrPreaTiposExtras_Obtener(codEmpresa);
        }

        public ErrorDto CrPreaTiposExtras_Guardar(int codEmpresa, string usuario, CrdPreaTiposExtrasData request)
        {
            return _db.CrPreaTiposExtras_Guardar(codEmpresa, usuario, request);
        }

        public ErrorDto CrPreaTiposExtras_Eliminar(int codEmpresa, string codExtra, string usuario)
        {
            return _db.CrPreaTiposExtras_Eliminar(codEmpresa, codExtra, usuario);
        }
    }
}
