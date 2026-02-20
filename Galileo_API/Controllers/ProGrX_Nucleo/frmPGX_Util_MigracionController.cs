using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Nucleo;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class FrmPgxUtilMigracionController : ControllerBase
    {
        private readonly FrmPgxUtilMigracionBL _bl;
        public FrmPgxUtilMigracionController(IConfiguration config)
        {
            _bl = new FrmPgxUtilMigracionBL(config);
        }

        [Authorize]
        [HttpPost("PGX_UtilMigracion_Aplicar")]
        public ErrorDto PGX_UtilMigracion_Aplicar(int CodEmpresa, string usuario, List<PgxMigracionData> file)
        {
            return _bl.PGX_UtilMigracion_Aplicar(CodEmpresa, usuario, file);
        }

    }
}
