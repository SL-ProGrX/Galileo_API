using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Hipotecario;
using static Galileo_API.Models.ProGrX_Hipotecario.FrmVivReportesGarantiasModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Hipotecario
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmVivReportesGarantiasController : ControllerBase
    {
        private readonly FrmVivReportesGarantiasBL _bl;

        public FrmVivReportesGarantiasController(IConfiguration config)
        {
            _bl = new FrmVivReportesGarantiasBL(config);
        }

        [HttpGet("FrmVivReportesGarantias_Combo_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> FrmVivReportesGarantias_Combo_Obtener(int CodEmpresa,string tipo)
        {
            return _bl.FrmVivReportesGarantias_Combo_Obtener(CodEmpresa, tipo);
        }

        [HttpPost("FrmVivReportesGarantias_Reporte_Generar")]
        public ErrorDto<VivReporteGarantiasResponse> FrmVivReportesGarantias_Reporte_Generar(int CodEmpresa,[FromBody] VivReporteGarantiasRequest request)
        {
            return _bl.FrmVivReportesGarantias_Reporte_Generar(CodEmpresa, request);
        }

        [HttpPost("FrmVivReportesGarantias_ProdAcum_Generar")]
        public ErrorDto<VivReporteGarantiasResponse> FrmVivReportesGarantias_ProdAcum_Generar(int CodEmpresa,[FromBody] VivReporteGarantiasProdAcumRequest request)
        {
            return _bl.FrmVivReportesGarantias_ProdAcum_Generar(CodEmpresa, request);
        }
    }
}
