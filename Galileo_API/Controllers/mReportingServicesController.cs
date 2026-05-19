using Galileo.BusinessLogic;
using Galileo.Models;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Galileo.Controllers
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
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDto<IActionResult>), StatusCodes.Status200OK)]
        public IActionResult ReporteRDLC_v2(FrmReporteGlobal data)
        {
            var result = _reportingServicesBL.ReporteRDLC_v2(data);

            if (result is FileContentResult fcr)
            {
                var nombreReporte = $"{data.nombreReporte}.pdf";
                Response.Headers["Content-Disposition"] = $"inline; filename={nombreReporte}";
                fcr.FileDownloadName = nombreReporte;
                return fcr;
            }

            if (result is ObjectResult objectResult)
            {
                return new ObjectResult(objectResult.Value)
                {
                    StatusCode = StatusCodes.Status200OK
                };
            }

            return new ObjectResult(new ErrorDto<IActionResult>
            {
                Code = -1,
                Description = "Unexpected result type"
            })
            {
                StatusCode = StatusCodes.Status200OK
            };
            
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
