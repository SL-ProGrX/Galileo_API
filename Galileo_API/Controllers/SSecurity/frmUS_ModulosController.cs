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
    public class FrmUsModulosController : ControllerBase
    {
        readonly FrmUsModulosBl ModulosBL;

        public FrmUsModulosController(IConfiguration config)
        {
            ModulosBL = new FrmUsModulosBl(config);
        }


        [HttpGet("Modulo_ObtenerTodos")]
        public ErrorDto<List<ModuloDto>> Modulo_ObtenerTodos()
        {
            return ModulosBL.Modulo_ObtenerTodos();
        }


        [HttpPost("Modulo_Guardar")]
        public ErrorDto Modulo_Guardar(ModuloDto request)
        {
            return ModulosBL.Modulo_Guardar(request);
        }


        [HttpDelete("Modulo_Eliminar")]
        public ErrorDto Modulo_Eliminar(int request, int codEmpresa = 0, string usuario = "")
        {
            return ModulosBL.Modulo_Eliminar(request, codEmpresa, usuario);
        }

    }
}
