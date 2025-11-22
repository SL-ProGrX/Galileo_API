using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmUsModulosController : ControllerBase
    {
        readonly FrmUsModulosBl ModulosBL;

        public FrmUsModulosController(IConfiguration config)
        {
            ModulosBL = new FrmUsModulosBl(config);
        }


        [HttpGet("Modulo_ObtenerTodos")]
        // [Authorize]
        public List<ModuloDto> Modulo_ObtenerTodos()
        {
            return ModulosBL.Modulo_ObtenerTodos();
        }


        [HttpPost("Modulo_Guardar")]
        public ErrorDto Modulo_Guardar(ModuloDto request)
        {
            return ModulosBL.Modulo_Guardar(request);
        }


        [HttpDelete("Modulo_Eliminar")]
        //[Authorize]
        public ErrorDto Modulo_Eliminar(int request)
        {
            return ModulosBL.Modulo_Eliminar(request);
        }

    }
}