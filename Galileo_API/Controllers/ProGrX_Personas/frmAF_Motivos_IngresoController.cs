using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Personas;
using Galileo_API.BusinessLogic.ProGrX_Personas;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Personas
{
    [ApiController]
    [Route("api/[controller]")]
    public class FrmAfMotivosIngresoController : ControllerBase
    {

        private readonly FrmAfMotivosIngresoBL _bl;

        public FrmAfMotivosIngresoController(IConfiguration config)
        {
            _bl = new FrmAfMotivosIngresoBL(config);
        }

        [HttpGet("AF_MotivosIngreso_Obtener")]
        public ErrorDto<MotivoIngresoLista> AF_MotivosIngreso_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.AF_MotivosIngreso_Obtener(CodEmpresa, filtros);
        }

        [HttpGet("AF_MotivosIngreso_Valida")]
        public ErrorDto AF_MotivosIngreso_Valida([FromQuery] int CodEmpresa, [FromQuery] string CodMotivo)
        {
            return _bl.AF_MotivosIngreso_Valida(CodEmpresa, CodMotivo);
        }

        [HttpPost("AF_MotivosIngreso_Guardar")]
        public ErrorDto AF_MotivosIngreso_Guardar([FromQuery] int CodEmpresa, [FromQuery] string Usuario, [FromBody] MotivoIngresoData motivoIngreso)
        {
            return _bl.AF_MotivosIngreso_Guardar(CodEmpresa, Usuario, motivoIngreso);
        }

        [HttpDelete("AF_MotivosIngreso_Eliminar")]
        public ErrorDto AF_MotivosIngreso_Eliminar([FromQuery] int CodEmpresa, [FromQuery] string Usuario, [FromQuery] string CodMotivo)
        {
            return _bl.AF_MotivosIngreso_Eliminar(CodEmpresa, Usuario, CodMotivo);
        }
    }
}
