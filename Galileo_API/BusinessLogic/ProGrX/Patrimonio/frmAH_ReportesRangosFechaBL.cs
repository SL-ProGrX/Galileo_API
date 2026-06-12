using Galileo.Models.AH;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Patrimonio;

namespace Galileo_API.BusinessLogic.ProGrX.Patrimonio
{
    public class FrmAhReportesRangosFechaBL
    {
        private readonly FrmAhReportesRangosFechaDB _db;

        public FrmAhReportesRangosFechaBL(IConfiguration config)
        {
            _db = new FrmAhReportesRangosFechaDB(config);
        }

        public ErrorDto<FrmAhReportesRangosFechaFiltrosDto> AH_ReportesRangosFecha_Filtros_Obtener(int codEmpresa)
            => _db.AH_ReportesRangosFecha_Filtros_Obtener(codEmpresa);

        public ErrorDto<FrmAhReportesRangosFechaReporteResponse> AH_ReportesRangosFecha_Reporte_Obtener(
            int codEmpresa,
            FrmAhReportesRangosFechaReporteRequest request)
            => _db.AH_ReportesRangosFecha_Reporte_Obtener(codEmpresa, request);
    }
}
