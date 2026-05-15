using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Fondos;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers.ProGrX.Fondos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFndCierreMensualFndController : ControllerBase
    {
        private readonly FrmFndCierreMensualFndBL _bl;

        public FrmFndCierreMensualFndController(IConfiguration config)
        {
            _bl = new FrmFndCierreMensualFndBL(config);
        }

        [Authorize]
        [HttpPost("Fnd_CierreMensual_Aplicar")]
        public ErrorDto Fnd_CierreMensual_Aplicar(int CodEmpresa)
        {
            return _bl.Fnd_CierreMensual_Aplicar(CodEmpresa);
        }
    }
}