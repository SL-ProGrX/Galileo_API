using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Credito;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Credito
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrSeguimientoTramitesController : ControllerBase
    {
        private readonly FrmCrSeguimientoTramitesBL _BL;

        public FrmCrSeguimientoTramitesController(IConfiguration config)
        {
            _BL = new FrmCrSeguimientoTramitesBL(config);
        }

        [HttpGet("Cr_SeguimientoTramites_Obtener")]
        public ErrorDto<List<dynamic>> Cr_SeguimientoTramites_Obtener(int CodEmpresa, string? filtro)
        {
            return _BL.Cr_SeguimientoTramites_Obtener(CodEmpresa, filtro);
        }

    }
}
