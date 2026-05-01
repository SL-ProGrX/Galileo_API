using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Hipotecario; 
using static Galileo_API.Models.ProGrX_Hipotecario.FrmVivReportesGarantiasModels;

namespace Galileo_API.BusinessLogic.ProGrX_Hipotecario
{
    public class FrmVivReportesGarantiasBL
    {
        private readonly FrmVivReportesGarantiasDB _db;

        public FrmVivReportesGarantiasBL(IConfiguration config)
        {
            _db = new FrmVivReportesGarantiasDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> FrmVivReportesGarantias_Combo_Obtener(int codEmpresa, string tipo)
            => _db.FrmVivReportesGarantias_Combo_Obtener(codEmpresa, tipo);

        public ErrorDto<VivReporteGarantiasResponse> FrmVivReportesGarantias_Reporte_Generar(int codEmpresa, VivReporteGarantiasRequest request)
             => _db.FrmVivReportesGarantias_Reporte_Generar(codEmpresa, request);

        public ErrorDto<VivReporteGarantiasResponse> FrmVivReportesGarantias_ProdAcum_Generar(int CodEmpresa, VivReporteGarantiasProdAcumRequest request)
            => _db.FrmVivReportesGarantias_ProdAcum_Generar(CodEmpresa, request);

     
    }
}
