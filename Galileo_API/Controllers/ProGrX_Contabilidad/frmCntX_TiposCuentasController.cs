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
    public class FrmCntXTiposCuentasController : ControllerBase
    {
        private readonly FrmCntXTiposCuentasBl _bl;

        public FrmCntXTiposCuentasController(IConfiguration config) => _bl = new FrmCntXTiposCuentasBl(config);
        
        [HttpGet("CntXTiposCuentas_Obtener")]
        public ErrorDto<List<CntXTiposCuentasData>> CntXTiposCuentas_Obtener(int codEmpresa, int codConta)
        {
            return _bl.CntXTiposCuentas_Obtener(codEmpresa, codConta);
        }

        [HttpPost("CntXTiposCuentas_Guardar")]
        public ErrorDto CntXTiposCuentas_Guardar(int codEmpresa, int codConta, string usuario, CntXTiposCuentasData request)
        {
            return _bl.CntXTiposCuentas_Guardar(codEmpresa, codConta, usuario, request);
        }

        [HttpPost("CntXTiposCuentas_Eliminar")]
        public ErrorDto CntXTiposCuentas_Eliminar(int codEmpresa, int codConta, string usuario, string tipoCuenta)
        {
            return _bl.CntXTiposCuentas_Eliminar(codEmpresa, codConta, usuario, tipoCuenta);
        }
    }
}