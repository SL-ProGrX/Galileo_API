using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.General;
using Galileo_API.Models.ProGrX.General;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.General
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public sealed class FrmCcEstadoCuentaMailController : ControllerBase
    {
        private readonly FrmCcEstadoCuentaMailBl _bl;

        public FrmCcEstadoCuentaMailController(
            IConfiguration config)
        {
            _bl = new FrmCcEstadoCuentaMailBl(config);
        }

        [HttpGet(
            "CC_Estado_Cuenta_Mail_Inicializar")]
        public ErrorDto<CcEstadoCuentaMailInicialData>
            CC_Estado_Cuenta_Mail_Inicializar(
                int codEmpresa,
                string cedula)
        {
            return _bl.CC_Estado_Cuenta_Mail_Inicializar(
                codEmpresa,
                cedula);
        }

        [HttpPost(
            "CC_Estado_Cuenta_Mail_Enviar")]
        public ErrorDto CC_Estado_Cuenta_Mail_Enviar(
            int codEmpresa,
            CcEstadoCuentaMailEnviarRequest? request)
        {
            return _bl.CC_Estado_Cuenta_Mail_Enviar(
                codEmpresa,
                request);
        }
    }
}
