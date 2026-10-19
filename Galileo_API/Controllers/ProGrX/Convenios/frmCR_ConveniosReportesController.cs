using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Convenios;
using Galileo_API.Models.ProGrX.Convenios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Convenios
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrConveniosReportesController : ControllerBase
    {
        private readonly FrmCrConveniosReportesBl _bl;

        public FrmCrConveniosReportesController(IConfiguration config)
        {
            _bl = new FrmCrConveniosReportesBl(config);
        }

        [HttpGet("CrConveniosReportes_Catalogo_Obtener")]
        public ErrorDto<List<CrConveniosReportesCatalogoData>> CrConveniosReportes_Catalogo_Obtener(
            [FromQuery] int codEmpresa)
            => _bl.CrConveniosReportes_Catalogo_Obtener(codEmpresa);

        [HttpGet("CrConveniosReportes_Catalogo_Describe")]
        public ErrorDto<CrConveniosReportesCatalogoData> CrConveniosReportes_Catalogo_Describe(
            [FromQuery] int codEmpresa,
            [FromQuery] string codigo)
            => _bl.CrConveniosReportes_Catalogo_Describe(codEmpresa, codigo);
    }
}
