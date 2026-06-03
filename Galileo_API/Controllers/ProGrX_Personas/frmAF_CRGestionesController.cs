using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;
using Galileo.BusinessLogic.ProGrX_Personas;

namespace Galileo.Controllers.ProGrX_Personas
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAFCrGestionesController : ControllerBase
    {
        private readonly FrmAFCrGestionesBL _bl;

        public FrmAFCrGestionesController(IConfiguration config)
        {
            _bl = new FrmAFCrGestionesBL(config);
        }

        [Authorize]
        [HttpGet("AF_CRGestiones_Obtener")]
        public ActionResult<ErrorDto<List<AfCrGestionesData>>> AF_CRGestiones_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.AF_CRGestiones_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("AF_CRGestiones_Guardar")]
        public ActionResult<ErrorDto> AF_CRGestiones_Guardar(int CodEmpresa, string usuario, [FromBody] AfCrGestionesData gestion)
        {
            return _bl.AF_CRGestiones_Guardar(CodEmpresa, usuario, gestion);
        }

        [Authorize]
        [HttpDelete("AF_CRGestiones_Eliminar")]
        public ActionResult<ErrorDto> AF_CRGestiones_Eliminar(int CodEmpresa, string cod_gestion, string usuario)
        {
            return _bl.AF_CRGestiones_Eliminar(CodEmpresa, cod_gestion, usuario);
        }
    }
}
