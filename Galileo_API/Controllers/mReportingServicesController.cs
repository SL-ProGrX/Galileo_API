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
            //var result = _reportingServicesBL.ReporteRDLC_v2(data);

            //if (result is FileContentResult fcr)
            //{
            //    var nombreReporte = data.nombreReporte + ".pdf";
            //    // Forzar inline:
            //    Response.Headers["Content-Disposition"] =
            //        $"inline; filename={nombreReporte}";
            //    // Opcional: anular FileDownloadName para evitar "attachment"
            //    fcr.FileDownloadName = nombreReporte;
            //    return fcr;
            //}

            //if (result is ObjectResult objectResult && objectResult.Value != null)
            //{
            //    var j = JObject.FromObject(objectResult.Value);

            //    var code = j["Code"]?.Value<int?>() ?? 0;
            //    var desc = j["Description"]?.Value<string>() ?? string.Empty;

            //    return new ObjectResult(new ErrorDto<IActionResult>
            //    {
            //        Code = code,
            //        Description = desc
            //    })
            //    {
            //        StatusCode = 200
            //    };
            //}

            //return new ObjectResult(new ErrorDto<IActionResult>
            //{
            //    Code = 0,
            //    Description = "Unexpected result type"
            //})
            //{
            //    StatusCode = 200
            //};
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
