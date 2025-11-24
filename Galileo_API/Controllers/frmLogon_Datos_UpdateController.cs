using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmLogon_Datos_UpdateController : ControllerBase
    {
        readonly FrmLogon_DatosUpdateBl Datos_UpdateBL;

        public FrmLogon_Datos_UpdateController(IConfiguration config)
        {
            Datos_UpdateBL = new FrmLogon_DatosUpdateBl(config);
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
