using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.Controllers.ProGrX.Clientes
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAFLiquidacionReportesController : ControllerBase
    {
        private readonly FrmAfLiquidacionReportesBL _bl;

        public FrmAFLiquidacionReportesController(IConfiguration config)
        {
            _bl = new FrmAfLiquidacionReportesBL(config);
        }

        [Authorize]
        [HttpGet("AF_LiqReportes_Instituciones_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_LiqReportes_Instituciones_Obtener(int CodEmpresa)
        {
            return _bl.AF_LiqReportes_Instituciones_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_LiqReportes_Obtener")]
        public ErrorDto<AfLiquidacionReportesData?> AF_LiqReportes_Obtener(int CodEmpresa, int liquidacion)
        {
            return _bl.AF_LiqReportes_Obtener(CodEmpresa, liquidacion);
        }
    }
}