using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCOControlReportesController : ControllerBase
    {
        private readonly FrmCOControlReportesBL BL;

        public FrmCOControlReportesController(IConfiguration config)
        {
            BL = new FrmCOControlReportesBL(config);
        }

        
        [HttpGet("CO_ControlReportes_Catalogo_Obtener")]
        public ErrorDto<List<CoControlReporteItemDto>> CO_ControlReportes_Catalogo_Obtener(int CodEmpresa)
        {
            return BL.CO_ControlReportes_Catalogo_Obtener(CodEmpresa);
        }

        [HttpGet("CO_ControlReportes_Filtros_Obtener")]
        public ErrorDto<CoControlReportesFiltrosDto> CO_ControlReportes_Filtros_Obtener(int CodEmpresa)
        {
            return BL.CO_ControlReportes_Filtros_Obtener(CodEmpresa);
        }

        [HttpPost("CO_ControlReportes_Cubo_Procesar")]
        public ErrorDto CO_ControlReportes_Cubo_Procesar(
            int CodEmpresa,
            [FromBody] CoControlReportesCuboRequestDto data)
        {
            return BL.CO_ControlReportes_Cubo_Procesar(CodEmpresa, data);
        }
    }
}
