using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCntXRepEspecialesController : ControllerBase
    {
        private readonly FrmCntxRepEspecialesBl _bl;

        public FrmCntXRepEspecialesController(IConfiguration config)
        {
            _bl = new FrmCntxRepEspecialesBl(config);
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

        [HttpPost]
        [Route("GenerarReporte")]
        public ErrorDto<bool> GenerarReporte(int CodEmpresa,int CodContabilidad, CntxRepEspecialFiltroDto filtros)
        {
            return _bl.GenerarReporte(CodEmpresa,CodContabilidad,filtros);
        }
    }
}