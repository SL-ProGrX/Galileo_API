using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCoControlComGeneracionController : ControllerBase
    {
        private readonly FrmCoControlComGeneracionBL _bl;

        public FrmCoControlComGeneracionController(IConfiguration config)
        {
            _bl = new FrmCoControlComGeneracionBL(config);
        }

        [HttpPost("Co_ControlComGeneracion_Actualizar")]
        public ErrorDto Co_ControlComGeneracion_Actualizar(int CodEmpresa)
        {
            return _bl.Co_ControlComGeneracion_Actualizar(CodEmpresa);
        }
    }
}
