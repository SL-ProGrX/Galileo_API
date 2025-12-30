using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/FrmUsCuentaReestablece")]
    [Route("api/frmUS_CuentaReestablece")]
    [ApiController]
    [Authorize]
    public class FrmUsCuentaReestableceController : ControllerBase
    {
        readonly FrmUsCuentaReestableceBl CuentaReestableceBL;

        public FrmUsCuentaReestableceController(IConfiguration config)
        {
            CuentaReestableceBL = new FrmUsCuentaReestableceBl(config);
        }

        [HttpPost("UsuarioCuentaReestablecer")]
        public ErrorDto UsuarioCuentaReestablecer(CuentaReestablecer usuarioCuentaReestablecerDto)
        {
            return CuentaReestableceBL.UsuarioCuentaReestablecer(usuarioCuentaReestablecerDto);
        }
    }
}
