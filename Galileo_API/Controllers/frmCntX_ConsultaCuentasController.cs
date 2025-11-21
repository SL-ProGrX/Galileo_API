using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class frmCntX_ConsultaCuentasController : Controller
    {
        private readonly IConfiguration _config;

        public frmCntX_ConsultaCuentasController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("ObtenerCuentas")]
        public List<CtnxCuentasDto> ObtenerCuentas(int CodEmpresa, CuentaVarModel cuenta)
        {
            return new frmCntX_ConsultaCuentasBL(_config).ObtenerCuentas(CodEmpresa, cuenta);
        }
        [HttpPost("ObtenerCuentasArbol")]
        public List<CtnxCuentasArbolModel> ObtenerCuentasArbol(int CodEmpresa, CuentaVarModel cuenta)
        {
            return new frmCntX_ConsultaCuentasBL(_config).ObtenerCuentasArbol(CodEmpresa, cuenta);
        }
        [HttpGet("ObtenerDivisas")]
        public List<DropDownListaGenericaModel> ObtenerDivisas(int CodEmpresa, int Contavilidad)
        {
            return new frmCntX_ConsultaCuentasBL(_config).ObtenerDivisas(CodEmpresa, Contavilidad);
        }
        [HttpGet("ObtenerTipoCuentas")]
        public List<DropDownListaGenericaModel> ObtenerTiposCuentas(int CodEmpresa, int Contavilidad)
        {
            return new frmCntX_ConsultaCuentasBL(_config).ObtenerTiposCuentas(CodEmpresa, Contavilidad);
        }

    }
}
