using Microsoft.AspNetCore.Mvc;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic
{
    public class mReportingServicesBL
    {
        private readonly IConfiguration _config;
        private readonly mReportingServicesDB _reportingServicesDB;

        public mReportingServicesBL(IConfiguration config)
        {
            _config = config;
            _reportingServicesDB = new mReportingServicesDB(_config);
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
