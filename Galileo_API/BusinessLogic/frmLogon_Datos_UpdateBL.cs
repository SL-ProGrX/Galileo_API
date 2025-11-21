using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic
{
    public class frmLogon_Datos_UpdateBL
    {

        private readonly IConfiguration _config;
        FrmLogonDatosUpdateDb Datos_UpdateDB;

        public frmLogon_Datos_UpdateBL(IConfiguration config)
        {
            _config = config;
            Datos_UpdateDB = new FrmLogonDatosUpdateDb(_config);
        }


        public LogonUpdateData LogonObtenerDatosUsuario(string usuario)
        {
            return Datos_UpdateDB.LogonObtenerDatosUsuario(usuario);
        }

        public ErrorDto LogonUpdateDatosUsuario(LogonUpdateData info)
        {
            return Datos_UpdateDB.LogonUpdateDatosUsuario(info);
        }

    }//end class
}//end namespace
