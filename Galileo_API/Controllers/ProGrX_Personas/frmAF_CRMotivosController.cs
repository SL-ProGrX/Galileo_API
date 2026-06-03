using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;
using Galileo.BusinessLogic.ProGrX_Personas;

namespace Galileo.Controllers.ProGrX_Personas
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAFCrMotivosController : ControllerBase
    {
        private readonly FrmAFCrMotivosBL _bl;

        public FrmAFCrMotivosController(IConfiguration config)
        {
            _bl = new FrmAFCrMotivosBL(config);
        }

        [Authorize]
        [HttpGet("AF_CRMotivos_Obtener")]
        public ActionResult<ErrorDto<List<AfCrMotivosData>>> AF_CRMotivos_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.AF_CRMotivos_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("AF_CRMotivos_Guardar")]
        public ActionResult<ErrorDto> AF_CRMotivos_Guardar(int CodEmpresa, string usuario, [FromBody] AfCrMotivosData motivo)
        {
            return _bl.AF_CRMotivos_Guardar(CodEmpresa, usuario, motivo);
        }

        [Authorize]
        [HttpDelete("AF_CRMotivos_Eliminar")]
        public ActionResult<ErrorDto> AF_CRMotivos_Eliminar(int CodEmpresa, string cod_motivo, string usuario)
        {
            return _bl.AF_CRMotivos_Eliminar(CodEmpresa, cod_motivo, usuario);
        }
    }
}
