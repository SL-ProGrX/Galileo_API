using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic
{
    public class FrmLogonDatosUpdateBl
    {
        readonly FrmLogon_DatosUpdateDb Datos_UpdateDB;

        public FrmLogonDatosUpdateBl(IConfiguration config)
        {
            Datos_UpdateDB = new FrmLogon_DatosUpdateDb(config);
        }

        public LogonUpdateData LogonObtenerDatosUsuario(string usuario)
        {
            return Datos_UpdateDB.LogonObtenerDatosUsuario(usuario);
        }

        public ErrorDto LogonUpdateDatosUsuario(LogonUpdateData info)
        {
            return Datos_UpdateDB.LogonUpdateDatosUsuario(info);
        }

    }
}
