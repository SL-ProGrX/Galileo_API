using Galileo.Models.AH;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Patrimonio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Patrimonio
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmAHMovimientosController : ControllerBase
    {
        private readonly FrmAHMovimientosBL _bl;

        public FrmAHMovimientosController(IConfiguration config)
        {
            _bl = new FrmAHMovimientosBL(config);
        }

        [HttpGet("AH_Movimientos_Filtros_Obtener")]
        public ErrorDto<MovimientosPatrimonioFiltrosDto?> AH_Movimientos_Filtros_Obtener(
            [FromQuery] int CodEmpresa)
            => _bl.AH_Movimientos_Filtros_Obtener(CodEmpresa);

        [HttpGet("AH_Movimientos_Consulta_Obtener")]
        public ErrorDto<List<MovimientosPatrimonioDto>> AH_Movimientos_Consulta_Obtener(
            [FromQuery] int CodEmpresa,
            [FromQuery] MovimientosPatrimonioConsultaRequest request)
            => _bl.AH_Movimientos_Consulta_Obtener(CodEmpresa, request);
    }
}
