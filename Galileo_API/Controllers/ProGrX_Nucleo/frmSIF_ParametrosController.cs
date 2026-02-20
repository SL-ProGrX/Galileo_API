using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.SIF;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmSifParametrosController : ControllerBase
    {
        private readonly FrmSifParametrosBL _bl;
        public FrmSifParametrosController(IConfiguration config)
        {
            _bl = new FrmSifParametrosBL(config);
        }

        [HttpGet("obtener_ParametrosSistema")]
        [Authorize]
        public ErrorDto<List<SifParametrosDto>> obtener_ParametrosSistema(int CodEmpresa)
        {
            return _bl.obtener_ParametrosSistema(CodEmpresa);
        }

        [HttpPut("Parametros_Actualizar")]
        [Authorize]
        public ErrorDto Parametros_Actualizar(int CodEmpresa, string usuario, SifParametrosDto parametros)
        {
            return _bl.Parametros_Actualizar(CodEmpresa, usuario, parametros);
        }
    }
}