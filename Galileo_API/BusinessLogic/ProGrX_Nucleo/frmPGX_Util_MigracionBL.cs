using Galileo.DataBaseTier.ProGrX_Nucleo;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.BusinessLogic.ProGrX_Nucleo
{
    public class FrmPgxUtilMigracionBL (IConfiguration config)
    {
        private readonly FrmPgxUtilMigracionDB _db = new(config);

        public ErrorDto PGX_UtilMigracion_Aplicar(int CodEmpresa, string usuario, List<PgxMigracionData> file)
        {
            return _db.PGX_UtilMigracion_Aplicar(CodEmpresa, usuario, file);
        }
    }
}
