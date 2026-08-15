using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.Models;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCntXRepBalanceComprobacionController : ControllerBase
    {
        private readonly FrmCntXRepBalanceComprobacionBl _bl;

        public FrmCntXRepBalanceComprobacionController(IConfiguration config)
        {
            _bl = new FrmCntXRepBalanceComprobacionBl(config);
        }

        [Authorize]
        [HttpGet("CntX_Unidades_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> CntX_Unidades_Listar(
            int codEmpresa,
            int codContabilidad)
        {
            return _bl.CntX_Unidades_Listar(codEmpresa, codContabilidad);
        }

        [Authorize]
        [HttpPost("CntX_Preliminar_Montar")]
        public ErrorDto<bool> CntX_Preliminar_Montar(
            int codEmpresa,
            [FromBody] CntXPreliminarMontarRequest request)
        {
            return _bl.CntX_Preliminar_Montar(codEmpresa, request);
        }

        [Authorize]
        [HttpPost("CntX_Movimientos_Restructurar")]
        public ErrorDto CntX_Movimientos_Restructurar(
            int codEmpresa,
            [FromBody] CntXCalculosRestructuraRequest request)
        {
            return _bl.CntX_Movimientos_Restructurar(codEmpresa, request);
        }
    }
}
