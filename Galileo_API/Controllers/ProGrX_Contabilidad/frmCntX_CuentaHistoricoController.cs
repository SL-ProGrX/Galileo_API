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
    public class FrmCntXCuentaHistoricoController : ControllerBase
    {
        private readonly FrmCntXCuentaHistoricoBl _bl;

        public FrmCntXCuentaHistoricoController(IConfiguration config) => _bl = new FrmCntXCuentaHistoricoBl(config);
        
        [HttpGet("CntXCuentaHistorico_Unidades_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CntXCuentaHistorico_Unidades_Obtener(int codEmpresa, int codConta)
        {
            return _bl.CntXCuentaHistorico_Unidades_Obtener(codEmpresa, codConta);
        }

        [HttpGet("CntXCuentaHistorico_CentroCostos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CntXCuentaHistorico_CentroCostos_Obtener(int codEmpresa, int codConta, string codUnidad)
        {
            return _bl.CntXCuentaHistorico_CentroCostos_Obtener(codEmpresa, codConta, codUnidad);
        }

        [HttpGet("CntXCuentaHistorico_Obtener")]
        public ErrorDto<List<CntXCuentaHistoricoData>> CntXCuentaHistorico_Obtener(
            int codEmpresa, int codConta, string cuenta, string codUnidad, string codCentroCosto)
        {
            return _bl.CntXCuentaHistorico_Obtener(codEmpresa, codConta, cuenta, codUnidad, codCentroCosto);
        }
    }
}