using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MCntLinkController : ControllerBase
    {
        private readonly IConfiguration _config;

        public MCntLinkController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("sbgCntParametros")]
        // [Authorize]
        public ErrorDto<DefMascarasDto> sbgCntParametros(int CodEmpresa)
        {
            return new MCntLinkBl(_config).sbgCntParametros(CodEmpresa);
        }

        [HttpGet("fxgCntCuentaFormato")]
        // [Authorize]
        public string fxgCntCuentaFormato(int CodEmpresa, bool blnMascara, string pCuenta, int optMensaje = 1)
        {
            return new MCntLinkBl(_config).fxgCntCuentaFormato(CodEmpresa, blnMascara, pCuenta, optMensaje);
        }

        [HttpGet("fxgCntCuentaValida")]
        // [Authorize]
        public bool fxgCntCuentaValida(int CodEmpresa, string vCuenta)
        {
            return new MCntLinkBl(_config).fxgCntCuentaValida(CodEmpresa, vCuenta);
        }

    }
}
