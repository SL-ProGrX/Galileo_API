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
    public class FrmCntXRepGeneralResultadosController : ControllerBase
    {
        private readonly FrmCntXRepGeneralResultadosBL BL;

        public FrmCntXRepGeneralResultadosController(IConfiguration config)
        {
            BL = new FrmCntXRepGeneralResultadosBL(config);
        }

        [Authorize]
        [HttpGet("CntX_Unidades_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CntX_Unidades_Dropdown_Obtener(int CodEmpresa, int codContabilidad)
        {
            return BL.CntX_Unidades_Dropdown_Obtener(CodEmpresa, codContabilidad);
        }

        [Authorize]
        [HttpGet("CntX_CentroCosto_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CntX_CentroCosto_Dropdown_Obtener(int CodEmpresa, int codContabilidad, string? codUnidad)
        {
            return BL.CntX_CentroCosto_Dropdown_Obtener(CodEmpresa, codContabilidad, codUnidad);
        }

        [Authorize]
        [HttpPost("CntX_RepGeneralResultados_ValidarReporte")]
        public ErrorDto<CntXRepGeneralResultadosValidarResponseDto> CntX_RepGeneralResultados_ValidarReporte(int CodEmpresa,[FromBody] CntXRepGeneralResultadosValidarRequestDto data)
        {
            return BL.CntX_RepGeneralResultados_ValidarReporte(CodEmpresa, data);
        }
    }
}