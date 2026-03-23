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
    public class FrmCntXRepMovPeriodoController : ControllerBase
    {
        private readonly FrmCntXRepMovPeriodoBl _bl;

        public FrmCntXRepMovPeriodoController(IConfiguration config)
        {
            _bl = new FrmCntXRepMovPeriodoBl(config);
        }

        [HttpGet("CntX_Periodos_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> CntX_Periodos_Listar(int codEmpresa, int codContabilidad)
        {
            return _bl.CntX_Periodos_Listar(codEmpresa, codContabilidad);
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

        [HttpGet("CntX_Areas_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> CntX_Areas_Listar(int codEmpresa, int codContabilidad)
        {
            return _bl.CntX_Areas_Listar(codEmpresa, codContabilidad);
        }

        [HttpPost("GenerarReporte")]
        public ErrorDto<bool> GenerarReporte(int codEmpresa, int codContabilidad, CntxRepMovPeriodoFiltroDto filtros)
        {
            return _bl.GenerarReporte(codEmpresa, codContabilidad, filtros);
        }
    }
}