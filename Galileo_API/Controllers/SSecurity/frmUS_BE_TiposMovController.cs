using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmUs_Be_TiposMovController : ControllerBase
    {
        readonly FrmUsBeTiposMovBl MovimientoBEBL;

        public FrmUs_Be_TiposMovController(IConfiguration config)
        {
            MovimientoBEBL = new FrmUsBeTiposMovBl(config);
        }

        [HttpGet("MovimientoBE_ObtenerTodos")]
        // [Authorize]
        public List<MovimientoBE> TiposId_ObtenerTodos(int modulo)
        {
            return MovimientoBEBL.MovimientoBE_ObtenerTodos(modulo);
        }


        [HttpPost("MovimientoBE_Guardar")]
        // [Authorize]
        public ErrorDto MovimientoBE_Guardar(MovimientoBE request)
        {
            return MovimientoBEBL.MovimientoBE_Guardar(request);
        }


        [HttpDelete("MovimientoBE_Eliminar")]
        //[Authorize]
        public ErrorDto MovimientoBE_Eliminar(string movimiento, int modulo)
        {
            return MovimientoBEBL.MovimientoBE_Eliminar(movimiento, modulo);
        }
    }
}
