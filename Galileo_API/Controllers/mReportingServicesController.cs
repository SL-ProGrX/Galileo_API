using Galileo.BusinessLogic;
using Galileo.Models;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Mvc;

namespace PgxAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MReportingServicesController : ControllerBase
    {

        private readonly MReportingServicesBL _reportingServicesBL;
        public MReportingServicesController(IConfiguration config)
        {
            _reportingServicesBL = new MReportingServicesBL(config);
        }

        [HttpPost("ReporteRDLC_v2")]
        public IActionResult ReporteRDLC_v2(FrmReporteGlobal data)
        {
            var result = _reportingServicesBL.ReporteRDLC_v2(data);
            if (result is FileContentResult fcr)
            {
                var nombreReporte = data.nombreReporte + ".pdf";
                // Forzar inline:
                Response.Headers["Content-Disposition"] =
                    $"inline; filename={nombreReporte}";
                // Opcional: anular FileDownloadName para evitar "attachment"
                fcr.FileDownloadName = nombreReporte;
                return fcr;
            }
           

                return result;
        }

        [HttpPost("ReporteRDLC")]
        public ErrorDto<object> ReporteRDLC(FrmReporteGlobal data)
        {
            return _reportingServicesBL.ReporteRDLC(data);
        }


        [HttpGet("ReportesInfo/{CodEmpresa}")]
        public ErrorDto<object> ReportesInfo(int CodEmpresa)
        {
           return _reportingServicesBL.ReportesInfo(CodEmpresa);
        }

    }
}
