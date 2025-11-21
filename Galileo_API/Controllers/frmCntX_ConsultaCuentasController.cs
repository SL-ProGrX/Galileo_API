using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCntXConsultaCuentasController : ControllerBase
    {
        private readonly IConfiguration _config;

        public FrmCntXConsultaCuentasController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("ObtenerCuentas")]
        public List<CtnxCuentasDto> ObtenerCuentas(int CodEmpresa, CuentaVarModel cuenta)
        {
            return new FrmCntXConsultaCuentasBl(_config).ObtenerCuentas(CodEmpresa, cuenta);
        }
        [HttpPost("ObtenerCuentasArbol")]
        public List<CtnxCuentasArbolModel> ObtenerCuentasArbol(int CodEmpresa, CuentaVarModel cuenta)
        {
            return new FrmCntXConsultaCuentasBl(_config).ObtenerCuentasArbol(CodEmpresa, cuenta);
        }
        [HttpGet("ObtenerDivisas")]
        public List<DropDownListaGenericaModel> ObtenerDivisas(int CodEmpresa, int Contavilidad)
        {
            return new FrmCntXConsultaCuentasBl(_config).ObtenerDivisas(CodEmpresa, Contavilidad);
        }
        [HttpGet("ObtenerTipoCuentas")]
        public List<DropDownListaGenericaModel> ObtenerTiposCuentas(int CodEmpresa, int Contavilidad)
        {
            return new FrmCntXConsultaCuentasBl(_config).ObtenerTiposCuentas(CodEmpresa, Contavilidad);
        }

    }
}
