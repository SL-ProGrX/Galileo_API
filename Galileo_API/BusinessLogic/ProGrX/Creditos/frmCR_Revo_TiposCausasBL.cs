using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrRevoTiposCausasBl
    {
        private readonly FrmCrRevoTiposCausasDb _db;

        public FrmCrRevoTiposCausasBl(IConfiguration config)
        {
            _db = new FrmCrRevoTiposCausasDb(config);
        }

        public ErrorDto<List<CrRevoTiposCausasData>> CR_Revo_TiposCausas_Obtener(int codEmpresa)
        {
            return _db.CR_Revo_TiposCausas_Obtener(codEmpresa);
        }

        public ErrorDto CR_Revo_TiposCausas_Guardar(
            int codEmpresa,
            string usuario,
            CrRevoTiposCausasData request)
        {
            return _db.CR_Revo_TiposCausas_Guardar(codEmpresa, usuario, request);
        }

        public ErrorDto CR_Revo_TiposCausas_Eliminar(
            int codEmpresa,
            string usuario,
            string codCausa)
        {
            return _db.CR_Revo_TiposCausas_Eliminar(codEmpresa, usuario, codCausa);
        }
    }
}
