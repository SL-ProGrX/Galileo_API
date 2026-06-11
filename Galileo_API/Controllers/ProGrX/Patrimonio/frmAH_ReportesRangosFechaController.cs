using Galileo.DataBaseTier;
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
    public class FrmAhReportesRangosFechaController : ControllerBase
    {
        private readonly FrmAhReportesRangosFechaBL _bl;

        public FrmAhReportesRangosFechaController(IConfiguration config)
        {
            _bl = new FrmAhReportesRangosFechaBL(config);
        }

        [HttpGet("AH_ReportesRangosFecha_Filtros_Obtener")]
        public ErrorDto<FrmAhReportesRangosFechaFiltrosDto> AH_ReportesRangosFecha_Filtros_Obtener(int CodEmpresa)
        {
            if (!ModelState.IsValid)
            {
                return new ErrorDto<FrmAhReportesRangosFechaFiltrosDto>
                {
                    Code = 400,
                    Description = "Invalid request data."
                };
            }

            return _bl.AH_ReportesRangosFecha_Filtros_Obtener(CodEmpresa);
        }

        [HttpPost("AH_ReportesRangosFecha_Reporte_Obtener")]
        public ErrorDto<FrmAhReportesRangosFechaReporteResponse> AH_ReportesRangosFecha_Reporte_Obtener(
            int CodEmpresa,
            [FromBody] FrmAhReportesRangosFechaReporteRequest request)
        {
            if (request == null)
            {
                return DbHelper.CreateErrorResponse(
                    "Datos requeridos",
                    -2,
                    new FrmAhReportesRangosFechaReporteResponse());
            }

            if (!ModelState.IsValid)
            {
                return new ErrorDto<FrmAhReportesRangosFechaReporteResponse>
                {
                    Code = 400,
                    Description = "Invalid request data."
                };
            }

            return _bl.AH_ReportesRangosFecha_Reporte_Obtener(CodEmpresa, request);
        }
    }
}
