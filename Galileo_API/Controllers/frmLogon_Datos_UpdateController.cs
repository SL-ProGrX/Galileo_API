using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class frmLogon_Datos_UpdateController : ControllerBase
    {
        private readonly IConfiguration _config;
        frmLogon_Datos_UpdateBL Datos_UpdateBL;

        public frmLogon_Datos_UpdateController(IConfiguration config)
        {
            _config = config;
            Datos_UpdateBL = new frmLogon_Datos_UpdateBL(_config);
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


    }//end class
}//end namespace
