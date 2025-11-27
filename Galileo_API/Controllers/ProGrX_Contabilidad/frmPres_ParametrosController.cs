using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.PRE;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class frmPres_ParametrosController : ControllerBase
    {
        readonly FrmPresParametrosBl _bl;
        public frmPres_ParametrosController(IConfiguration config)
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