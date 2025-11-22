using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmUsCuentaReestableceController : ControllerBase
    {
        readonly FrmUsCuentaReestableceBl CuentaReestableceBL;

        public FrmUsCuentaReestableceController(IConfiguration config)
        {
            CuentaReestableceBL = new FrmUsCuentaReestableceBl(config);
        }

        [HttpPost("UsuarioCuentaReestablecer")]
        //[Authorize]
        public ErrorDto UsuarioCuentaReestablecer(CuentaReestablecer usuarioCuentaReestablecerDto)
        {
            return CuentaReestableceBL.UsuarioCuentaReestablecer(usuarioCuentaReestablecerDto);
        }
    }
}
