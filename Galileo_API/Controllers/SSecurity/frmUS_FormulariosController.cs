using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmUsFormulariosController : ControllerBase
    {
        readonly FrmUsFormulariosBl FormulariosBL;

        public FrmUsFormulariosController(IConfiguration config)
        {
            FormulariosBL = new FrmUsFormulariosBl(config);
        }

        [HttpGet("FormulariosObtener")]
        public ErrorDto<List<FormularioDto>> FormulariosObtener(int moduloId)
        {
            return FormulariosBL.FormulariosObtener(moduloId);
        }


        [HttpDelete("Formulario_Eliminar")]
        public ErrorDto Formulario_Eliminar(int modulo, string formulario, int codEmpresa = 0, string usuario = "")
        {
            return FormulariosBL.Formulario_Eliminar(modulo, formulario, codEmpresa, usuario);
        }


        [HttpPost("Formulario_Guardar")]
        public ErrorDto Formulario_Guardar(FormularioDto request)
        {
            return FormulariosBL.Formulario_Guardar(request);
        }
    }
}
