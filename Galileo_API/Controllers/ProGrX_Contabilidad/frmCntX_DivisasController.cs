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
    public class FrmCntXDivisasController : ControllerBase
    {
        private readonly FrmCntXDivisasBl _bl;

        public FrmCntXDivisasController(IConfiguration config) => _bl = new FrmCntXDivisasBl(config);

        [HttpGet("CntXDivisas_Unidades_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CntXDivisas_Unidades_Obtener(int codEmpresa, int codConta)
        {
            return _bl.CntXDivisas_Unidades_Obtener(codEmpresa, codConta);
        }

        [HttpGet("CntXDivisas_CentroCostos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CntXDivisas_CentroCostos_Obtener(int codEmpresa, int codConta)
        {
            return _bl.CntXDivisas_CentroCostos_Obtener(codEmpresa, codConta);
        }

        [HttpGet("CntXDivisas_Lista_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CntXDivisas_Lista_Obtener(int codEmpresa, int codConta)
        {
            return _bl.CntXDivisas_Lista_Obtener(codEmpresa, codConta);
        }

        [HttpGet("CntXDivisas_Obtener")]
        public ErrorDto<CntXDivisaData> CntXDivisas_Obtener(int codEmpresa, int codConta, string codDivisa)
        {
            return _bl.CntXDivisas_Obtener(codEmpresa, codConta, codDivisa);
        }

        [HttpGet("CntXDivisas_Scroll_Obtener")]
        public ErrorDto<CntXDivisaData> CntXDivisas_Scroll_Obtener(int CodEmpresa, int codConta, int scrollCode, string codDivisa)
        {
            return _bl.CntXDivisas_Scroll_Obtener(CodEmpresa, codConta, scrollCode, codDivisa);
        }
    }
}