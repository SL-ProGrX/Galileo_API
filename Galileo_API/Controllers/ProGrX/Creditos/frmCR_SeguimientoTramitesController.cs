using Galileo_API.BusinessLogic.ProGrX.Credito;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Credito
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCrSeguimientoTramitesController : ControllerBase
    {
        private readonly FrmCrSeguimientoTramitesBL _BL;

        public FrmCrSeguimientoTramitesController(IConfiguration config)
        {
            _BL = new FrmCrSeguimientoTramitesBL(config);
        }

    }
}
