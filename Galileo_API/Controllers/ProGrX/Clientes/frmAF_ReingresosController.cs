using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.BusinessLogic.ProGrX.Clientes;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAFReingresosController : ControllerBase
    {
        private readonly FrmAFReingresosBL _bl;
        public FrmAFReingresosController(IConfiguration config)
        {
            _bl = new FrmAFReingresosBL(config);
        }

        [Authorize]
        [HttpGet("AF_PromotoresReingreso_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_PromotoresReingreso_Obtener(int CodEmpresa)
        {
            return _bl.AF_PromotoresReingreso_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpPost("AF_Persona_ActivarYVincular")]
        public ErrorDto AF_Persona_ActivarYVincular(int CodEmpresa, string request)
        {
            return _bl.AF_Persona_ActivarYVincular(CodEmpresa, request);
        }
    }
}