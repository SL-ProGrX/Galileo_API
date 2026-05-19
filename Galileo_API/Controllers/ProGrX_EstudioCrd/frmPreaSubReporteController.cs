using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_EstudioCrd
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FrmPreaSubReporteController : ControllerBase
    {
        private readonly FrmPreaSubReporteBL _bl;

        public FrmPreaSubReporteController(IConfiguration config)
        {
            _bl = new FrmPreaSubReporteBL(config);
        }

        [HttpPost("Prea_frmPreaSubReporte_Cargar")]
        public ErrorDto<FrmPreaSubReporteCargarResponse> Prea_frmPreaSubReporte_Cargar(
            int codEmpresa,
            [FromBody] FrmPreaSubReporteCargarRequest request)
        {
            return _bl.Prea_frmPreaSubReporte_Cargar(codEmpresa, request);
        }

        [HttpPost("Prea_frmPreaSubReporte_Imprimir_Obtener")]
        public ErrorDto<FrmPreaSubReporteImprimirResponse> Prea_frmPreaSubReporte_Imprimir_Obtener(
            int codEmpresa,
            [FromBody] FrmPreaSubReporteImprimirRequest request)
        {
            return _bl.Prea_frmPreaSubReporte_Imprimir_Obtener(codEmpresa, request);
        }
    }
}
