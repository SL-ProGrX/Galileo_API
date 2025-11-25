using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.Controllers
{
    [Route("api/frmLogon_Datos_Update")]
    [Route("api/FrmLogonDatosUpdate")]
    [ApiController]
    public class FrmLogonDatosUpdateController : ControllerBase
    {
        readonly FrmLogonDatosUpdateBl Datos_UpdateBL;

        public FrmLogonDatosUpdateController(IConfiguration config)
        {
            Datos_UpdateBL = new FrmLogonDatosUpdateBl(config);
        }

        [HttpGet("LogonObtenerDatosUsuario")]
        public LogonUpdateData LogonObtenerDatosUsuario(string usuario)
        {
            return Datos_UpdateBL.LogonObtenerDatosUsuario(usuario);
        }

        [HttpPost("LogonUpdateDatosUsuario")]
        public ErrorDto LogonUpdateDatosUsuario(LogonUpdateData info)
        {
            return Datos_UpdateBL.LogonUpdateDatosUsuario(info);
        }
    }
}
