using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class FrmUs_FormulariosController : ControllerBase
    {
        readonly FrmUsFormulariosBl FormulariosBL;

        public FrmUs_FormulariosController(IConfiguration config)
        {
            FormulariosBL = new FrmUsFormulariosBl(config);
        }

        [HttpGet("FormulariosObtener")]
        public List<FormularioDto> FormulariosObtener(int moduloId)
        {
            return FormulariosBL.FormulariosObtener(moduloId);
        }


        [HttpDelete("Formulario_Eliminar")]
        //[Authorize]
        public ErrorDto Formulario_Eliminar(int modulo, string formulario)
        {
            return FormulariosBL.Formulario_Eliminar(modulo, formulario);
        }


        [HttpPost("Formulario_Guardar")]
        //[Authorize]
        public ErrorDto Formulario_Guardar(FormularioDto request)
        {
            return FormulariosBL.Formulario_Guardar(request);
        }
    }
}
