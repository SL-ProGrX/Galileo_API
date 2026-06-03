using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;
using Galileo.BusinessLogic.ProGrX_Personas;

namespace Galileo.Controllers.ProGrX_Personas
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAFCrParametrosController : ControllerBase
    {
        private readonly FrmAFCrParametrosBL _bl;

        public FrmAFCrParametrosController(IConfiguration config)
        {
            _bl = new FrmAFCrParametrosBL(config);
        }

        [Authorize]
        [HttpGet("AF_CRParametros_Obtener")]
        public ErrorDto<List<AfCrParametrosData>> AF_CRParametros_Obtener(int CodEmpresa)
        {
            return _bl.AF_CRParametros_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpPost("AF_CRParametros_Guardar")]
        public ErrorDto AF_CRParametros_Guardar(int CodEmpresa, string usuario, [FromBody] AfCrParametrosData parametros)
        {
            return _bl.AF_CRParametros_Guardar(CodEmpresa, usuario, parametros);
        }
    }
}
