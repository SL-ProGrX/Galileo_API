using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/FrmUsOpciones")]
    [Route("api/frmUS_Opciones")]
    [ApiController]
    [Authorize]
    public class FrmUsOpcionesController : ControllerBase
    {
        readonly FrmUsOpcionesBl OpcionesBL;

        public FrmUsOpcionesController(IConfiguration config)
        {
            OpcionesBL = new FrmUsOpcionesBl(config);
        }


        [HttpGet("Modulo_ObtenerTodos")]
        public List<ModuloDto> Modulo_ObtenerTodos()
        {
            return OpcionesBL.Modulo_ObtenerTodos();
        }


        [HttpGet("Formulario_ObtenerTodos")]
        public List<FormularioDto> Formulario_ObtenerTodos(int modulo)
        {
            return OpcionesBL.Formulario_ObtenerTodos(modulo);
        }


        [HttpGet("Opcion_ObtenerTodos")]
        public List<OpcionDto> Opcion_ObtenerTodos(int modulo, string formulario)
        {
            return OpcionesBL.Opcion_ObtenerTodos(modulo, formulario);
        }


        [HttpDelete("Opcion_Eliminar")]
        public ErrorDto Opcion_Eliminar(string codigo, string formulario, int modulo)
        {
            return OpcionesBL.Opcion_Eliminar(codigo, formulario, modulo);
        }


        [HttpPost("Opcion_Guardar")]
        public ErrorDto Opcion_Guardar(OpcionDto request)
        {
            return OpcionesBL.Opcion_Guardar(request);
        }
    }
}
