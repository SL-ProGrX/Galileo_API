using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.General;
using Galileo_API.Models.ProGrX.General;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.General
{
    [Route("api/FrmCcReportesEstudioAuxiliares")]
    [ApiController]
    public class FrmCcReportesEstudioAuxiliaresController : ControllerBase
    {
        private readonly FrmCcReportesEstudioBL _BL;

        public FrmCcReportesEstudioAuxiliaresController(IConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            _BL = new FrmCcReportesEstudioBL(config);
        }

        [Authorize]
        [HttpGet("CC_ReportesEstudio_Catalogos_Obtener")]
        public ErrorDto<CcReportesEstudioCatalogosResponseDto> CC_ReportesEstudio_Catalogos_Obtener(int codEmpresa)
        {
            return _BL.CC_ReportesEstudio_Catalogos_Obtener(codEmpresa);
        }

        [Authorize]
        [HttpGet("CC_ReportesEstudio_Lineas_Obtener")]
        public ErrorDto<List<Dictionary<string, object?>>> CC_ReportesEstudio_Lineas_Obtener(
            int codEmpresa, [FromQuery] CcReportesEstudioLineasRequestDto request)
        {
            return _BL.CC_ReportesEstudio_Lineas_Obtener(codEmpresa, request);
        }

        [Authorize]
        [HttpGet("CC_ReportesEstudio_Resultado_Obtener")]
        public ErrorDto<List<Dictionary<string, object?>>> CC_ReportesEstudio_Resultado_Obtener(
            int codEmpresa, string usuario, [FromQuery] CcReportesEstudioGenerarRequestDto request)
        {
            return _BL.CC_ReportesEstudio_Resultado_Obtener(codEmpresa, usuario, request);
        }
    }
}
