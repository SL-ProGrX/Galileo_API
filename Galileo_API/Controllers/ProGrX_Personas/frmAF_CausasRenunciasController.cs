using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.ERROR;
using Galileo.BusinessLogic.ProGrX_Personas;
using Galileo.Models.ProGrX_Personas;

namespace Galileo.Controllers.ProGrX_Personas
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAFCausasRenunciasController : ControllerBase
    {
        private readonly FrmAFCausasRenunciasBL _bl;

        public FrmAFCausasRenunciasController(IConfiguration config)
        {
            _bl = new FrmAFCausasRenunciasBL(config);
        }

        [Authorize]
        [HttpGet("AF_CausasRenuncias_Obtener")]
        public ActionResult<ErrorDto<List<AfCausasRenunciasData>>> AF_CausasRenuncias_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.AF_CausasRenuncias_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("AF_CausasRenuncias_Guardar")]
        public ActionResult<ErrorDto> AF_CausasRenuncias_Guardar(int CodEmpresa, string usuario, [FromBody] AfCausasRenunciasData causa)
        {
            return _bl.AF_CausasRenuncias_Guardar(CodEmpresa, usuario, causa);
        }

        [Authorize]
        [HttpDelete("AF_CausasRenuncias_Eliminar")]
        public ActionResult<ErrorDto> AF_CausasRenuncias_Eliminar(int CodEmpresa, int id_causa, string usuario)
        {
            return _bl.AF_CausasRenuncias_Eliminar(CodEmpresa, id_causa, usuario);
        }
    }
}