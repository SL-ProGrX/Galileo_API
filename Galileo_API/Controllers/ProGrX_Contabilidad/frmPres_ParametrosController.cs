using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.PRE;

namespace Galileo.Controllers
{
    [Route("api/frmPres_Parametros")]
    [Route("api/FrmPresParametros")]
    [ApiController]
    public class FrmPresParametrosController : ControllerBase
    {
        readonly FrmPresParametrosBl _bl;
        public FrmPresParametrosController(IConfiguration config)
        {
            _bl = new FrmPresParametrosBl(config);
        }

        [Authorize]
        [HttpPost("PresParametros_Guardar")]
        public ErrorDto PresParametros_Guardar(int CodEmpresa, PresParametrosDto parametros)
        {
            return _bl.PresParametros_Guardar(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpGet("PresParametrosLista_Obtener")]
        public ErrorDto<List<PresParametrosDto>> PresParametrosLista_Obtener(int CodEmpresa)
        {
            return _bl.PresParametrosLista_Obtener(CodEmpresa);
        }
    }
}