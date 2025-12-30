using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.Controllers.ProGrX.Fondos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFndReportesGeneralesController : ControllerBase
    {
        private readonly FrmFndReportesGeneralesBl _bl;

        public FrmFndReportesGeneralesController(IConfiguration config)
        {
            _bl = new FrmFndReportesGeneralesBl(config);
        }

        [Authorize]
        [HttpGet("Fnd_ReportesGenerales_Catalogo_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_ReportesGenerales_Catalogo_Obtener(int CodEmpresa, int Index)
        {
            return _bl.Fnd_ReportesGenerales_Catalogo_Obtener(CodEmpresa, Index);
        }

        [Authorize]
        [HttpGet("Fnd_ReportesGenerales_Planes_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_ReportesGenerales_Planes_Obtener(int CodEmpresa, int CodOperadora, string? CodPlan, string? Usuario)
        {
            return _bl.Fnd_ReportesGenerales_Planes_Obtener(CodEmpresa, CodOperadora, CodPlan, Usuario);
        }

        [Authorize]
        [HttpGet("Fnd_ReportesGenerales_Plan_Scroll_Obtener")]
        public ErrorDto<DropDownListaGenericaModel> Fnd_ReportesGenerales_Plan_Scroll_Obtener(int codEmpresa, int CodOperadora, string? CodPlan, int scrollCode)
        {
            return _bl.Fnd_ReportesGenerales_Plan_Scroll_Obtener(codEmpresa, CodOperadora, CodPlan, scrollCode);
        }

        [Authorize]
        [HttpPost("Fnd_ReportesGenerales_CuboAplicar")]
        public ErrorDto Fnd_ReportesGenerales_CuboAplicar(int CodEmpresa, FndReportesGeneralesCuboFiltros Filtros)
        {
            return _bl.Fnd_ReportesGenerales_CuboAplicar(CodEmpresa, Filtros);
        }
    }
}
