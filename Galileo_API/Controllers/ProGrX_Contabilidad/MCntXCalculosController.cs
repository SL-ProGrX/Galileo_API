using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MCntXCalculosController : ControllerBase
    {
        private readonly MCntXCalculosBl _bl;

        public MCntXCalculosController(IConfiguration config) => _bl = new MCntXCalculosBl(config);

        [HttpPost("SbCntX_RestructuraMovimientosRSM")]
        public ErrorDto SbCntX_RestructuraMovimientosRSM(
            int codEmpresa,
            CntXCalculosRestructuraRequest request)
        {
            return _bl.SbCntX_RestructuraMovimientosRSM(codEmpresa, request);
        }

        [HttpPost("SbCntX_PeriodoCierre")]
        public ErrorDto SbCntX_PeriodoCierre(
            int codEmpresa,
            CntXCalculosPeriodoProcesoRequest request)
        {
            return _bl.SbCntX_PeriodoCierre(codEmpresa, request);
        }

        [HttpPost("SbCntX_CierreFiscal")]
        public ErrorDto SbCntX_CierreFiscal(
            int codEmpresa,
            CntXCalculosPeriodoProcesoRequest request)
        {
            return _bl.SbCntX_CierreFiscal(codEmpresa, request);
        }
    }
}
