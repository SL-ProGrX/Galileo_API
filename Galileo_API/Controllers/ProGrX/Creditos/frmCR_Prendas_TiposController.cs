using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrPrendasTiposController : ControllerBase
    {
        private readonly FrmCrPrendasTiposBl _bl;

        public FrmCrPrendasTiposController(IConfiguration config)
        {
            _bl = new FrmCrPrendasTiposBl(config);
        }

        [HttpGet("CrPrendasTipos_Obtener")]
        public ErrorDto<List<CrPrendasTipoData>> CrPrendasTipos_Obtener(int codEmpresa)
            => _bl.CrPrendasTipos_Obtener(codEmpresa);

        [HttpPost("CrPrendasTipos_Guardar")]
        public ErrorDto CrPrendasTipos_Guardar(int codEmpresa, [FromBody] CrPrendasTipoGuardarRequest request)
            => _bl.CrPrendasTipos_Guardar(codEmpresa, request);

        [HttpDelete("CrPrendasTipos_Eliminar")]
        public ErrorDto CrPrendasTipos_Eliminar(int codEmpresa, [FromBody] CrPrendasTipoEliminarRequest request)
            => _bl.CrPrendasTipos_Eliminar(codEmpresa, request);

        [HttpGet("CrPrendasTipos_Asignacion_Obtener")]
        public ErrorDto<List<CrPrendasTipoAsignacionData>> CrPrendasTipos_Asignacion_Obtener(
            int codEmpresa,
            string request)
            => _bl.CrPrendasTipos_Asignacion_Obtener(codEmpresa, request);

        [HttpPost("CrPrendasTipos_Asignacion_Guardar")]
        public ErrorDto CrPrendasTipos_Asignacion_Guardar(
            int codEmpresa,
            CrPrendasTipoAsignacionGuardarRequest request)
            => _bl.CrPrendasTipos_Asignacion_Guardar(codEmpresa, request);
    }
}