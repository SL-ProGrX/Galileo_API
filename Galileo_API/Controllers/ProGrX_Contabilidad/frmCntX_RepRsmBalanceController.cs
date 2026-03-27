using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCntXRsmBalanceController : ControllerBase
    {
        private readonly FrmCntXRsmBalanceBl _bl;

        public FrmCntXRsmBalanceController(IConfiguration config)
        {
            _bl = new FrmCntXRsmBalanceBl(config);
        }

        [HttpGet("CntX_Unidades_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> CntX_Unidades_Listar(int codEmpresa, int codContabilidad)
        {
            return _bl.CntX_Unidades_Listar(codEmpresa, codContabilidad);
        }

        [HttpGet("CntX_CentroCostos_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> CntX_CentroCostos_Listar(int codEmpresa, int codContabilidad, string unidad)
        {
            return _bl.CntX_CentroCostos_Listar(codEmpresa, codContabilidad, unidad);
        }

        [HttpPost("GenerarReporte")]
        public ErrorDto<bool> GenerarReporte(
            [FromQuery] int codEmpresa,
            [FromQuery] int codContabilidad,
            [FromBody] CntxRsmBalanceFiltroDto filtros)
        {
            return _bl.GenerarReporte(codEmpresa, codContabilidad, filtros);
        }
    }
}