using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogonController : ControllerBase
    {
        readonly LogonBL logonBL;

        public LogonController(IConfiguration config)
        {
            logonBL = new LogonBL(config);
        }

        [HttpGet("IntentosObtener")]
        //[Authorize]
        public IntentosObtenerDto IntentosObtener()
        {
            return logonBL.IntentosObtener();
        }

        [HttpPost("LoginObtener")]
        //[Authorize]
        public ErrorDto LoginObtener(LoginObtenerDto req)
        {
            return logonBL.LoginObtener(req);
        }

        [HttpGet("ClientesObtener")]
        //[Authorize]
        public ErrorDto<List<ClientesEmpresasObtenerDto>> ClientesObtener(string Usuario)
        {
            return logonBL.ClientesObtener(Usuario);
        }

        [HttpPost("ValidarDatosParaRenovarContra")]
        //[Authorize]
        public int ValidarDatos(string Usuario, string Email)
        {
            return logonBL.ValidarDatos(Usuario, Email);
        }

        [HttpPost("ValidarTokenParaRenovarContra")]
        //[Authorize]
        public int ValidarToken(string Usuario, string Token)
        {
            return logonBL.ValidarToken(Usuario, Token);
        }

        [HttpPost("EnviarTokenParaRenovarContra")]
        //[Authorize]
        public int EnviarToken(string Usuario)
        {
            return logonBL.EnviarToken(Usuario);
        }


        [HttpGet("TFA_Data_Load")]
        //[Authorize]
        public TfaData TFA_Data_Load(string Usuario)
        {
            return logonBL.TFA_Data_Load(Usuario);
        }

        [HttpPost("TFA_Codigo_EnviarMAIL")]
        //[Authorize]
        public Task<ErrorDto> TFA_Codigo_EnviarMAIL(string Usuario, string email)
        {
            return logonBL.TFA_Codigo_EnviarMAIL(Usuario, email);
        }

        [HttpPost("TFA_Codigo_Validar")]
        //[Authorize]
        public ErrorDto TFA_Codigo_Validar(string Usuario, string codigo)
        {
            return logonBL.TFA_Codigo_Validar(Usuario, codigo);
        }

    }
}
