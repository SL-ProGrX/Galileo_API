using Microsoft.Reporting.NETCore;
using Microsoft.AspNetCore.Mvc;

namespace Galileo.DataBaseTier
{
    public static class ReportRenderer
    {
        private const string PdfMimeType = "application/pdf";

        public static IActionResult AsPdf(LocalReport report, string? nombreReporte)
        {
            var bytes = report.Render("PDF", null, out _, out _, out _, out _, out _);
            return new FileContentResult(bytes, PdfMimeType)
            {
                FileDownloadName = $"{nombreReporte ?? "reporte"}.pdf"
            };
        }

        public static IActionResult AsJson(Dictionary<string, object> dataSets, List<string> warnings)
        {
            var payload = new { Code = 0, DataSets = dataSets, Warnings = warnings };
            return new OkObjectResult(payload);
        }

        public static IActionResult Error(string message, int status = 500)
            => new ObjectResult(new { Code = -1, Description = message }) { StatusCode = status };
    }
}

