using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd;

namespace Galileo_API.BusinessLogic.ProGrX_EstudioCrd
{
    public class FrmPreaSubReporteBL
    {
        private readonly FrmPreaSubReporteDB _db;

        public FrmPreaSubReporteBL(IConfiguration config)
        {
            _db = new FrmPreaSubReporteDB(config);
        }

        public ErrorDto<FrmPreaSubReporteCargarResponse> Prea_frmPreaSubReporte_Cargar(
            int codEmpresa,
            FrmPreaSubReporteCargarRequest request)
        {
            return _db.Prea_frmPreaSubReporte_Cargar(codEmpresa, request);
        }

        public ErrorDto<FrmPreaSubReporteImprimirResponse> Prea_frmPreaSubReporte_Imprimir_Obtener(
            int codEmpresa,
            FrmPreaSubReporteImprimirRequest request)
        {
            return _db.Prea_frmPreaSubReporte_Imprimir_Obtener(codEmpresa, request);
        }
    }
}
