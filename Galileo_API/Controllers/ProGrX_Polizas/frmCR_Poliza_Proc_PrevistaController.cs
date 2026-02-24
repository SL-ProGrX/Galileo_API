using Galileo_API.BusinessLogic.ProGrX_Polizas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Polizas
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrPolizaProcPrevistaController : ControllerBase
    {
        private readonly FrmCrPolizaProcPrevistaBL _bl;

        public FrmCrPolizaProcPrevistaController(IConfiguration config)
        {
            _bl = new FrmCrPolizaProcPrevistaBL(config);
        }
    }
}
