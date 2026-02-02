using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Mvc;

namespace Galileo.BusinessLogic
{
    public class MReportingServicesBL
    {
        private readonly IConfiguration _config;
        private readonly MReportingServicesDB _reportingServicesDB;

        public MReportingServicesBL(IConfiguration config)
        {
            _config = config;
            _reportingServicesDB = new MReportingServicesDB(config);

        }

        public IActionResult ReporteRDLC_v2(FrmReporteGlobal data)
        {
            return _reportingServicesDB.ReporteRDLC_v2(data);
        }

        public ErrorDto<object> ReporteRDLC(FrmReporteGlobal data)
        {
            return _reportingServicesDB.ReporteRDLC(data);
        }
        public ErrorDto<object> ReportesInfo(int CodEmpresa)
        {
            return _reportingServicesDB.ReportesInfo(CodEmpresa);
        }

    }
}
