using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.BusinessLogic
{
    public class FrmUsCuentaReestableceBl
    {
        readonly FrmUsCuentaReestableceDb CuentaReestableceDB;

        public FrmUsCuentaReestableceBl(IConfiguration config)
        {
            CuentaReestableceDB = new FrmUsCuentaReestableceDb(config);
        }

        public ErrorDto UsuarioCuentaReestablecer(CuentaReestablecer cuentaUsuarioReestablecerDto)
        {
            return CuentaReestableceDB.UsuarioCuentaReestablecer(cuentaUsuarioReestablecerDto);
        }

    }
}