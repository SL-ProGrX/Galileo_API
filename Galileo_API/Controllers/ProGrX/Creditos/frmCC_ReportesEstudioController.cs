using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public sealed class FrmCcReportesEstudioController : ControllerBase
    {
        private readonly FrmCcReportesEstudioBL _bl;

        public FrmCcReportesEstudioController(IConfiguration config) => _bl = new FrmCcReportesEstudioBL(config);

        [HttpGet("CC_ReportesEstudio_Catalogos_Obtener")]
        public ErrorDto<CcReportesEstudioCatalogosResponseDto> CC_ReportesEstudio_Catalogos_Obtener(int codEmpresa)
            => _bl.CC_ReportesEstudio_Catalogos_Obtener(codEmpresa);

        [HttpGet("CC_ReportesEstudio_Lineas_Obtener")]
        public ErrorDto<List<Dictionary<string, object?>>> CC_ReportesEstudio_Lineas_Obtener(
            int codEmpresa, [FromQuery] CcReportesEstudioLineasRequestDto request)
            => _bl.CC_ReportesEstudio_Lineas_Obtener(codEmpresa, request);

        [HttpGet("CC_ReportesEstudio_Resultado_Obtener")]
        public ErrorDto<List<Dictionary<string, object?>>> CC_ReportesEstudio_Resultado_Obtener(
            int codEmpresa, string usuario, [FromQuery] CcReportesEstudioGenerarRequestDto request)
            => _bl.CC_ReportesEstudio_Resultado_Obtener(codEmpresa, usuario, request);
    }
}
