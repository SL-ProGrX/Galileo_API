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
    public class FrmUsBeTiposMovController : ControllerBase
    {
        readonly FrmUsBeTiposMovBl MovimientoBEBL;

        public FrmUsBeTiposMovController(IConfiguration config)
        {
            MovimientoBEBL = new FrmUsBeTiposMovBl(config);
        }

        [HttpGet("MovimientoBE_ObtenerTodos")]
        public List<MovimientoBE> TiposId_ObtenerTodos(int modulo)
        {
            return MovimientoBEBL.MovimientoBE_ObtenerTodos(modulo);
        }


        [HttpPost("MovimientoBE_Guardar")]
        public ErrorDto MovimientoBE_Guardar(MovimientoBE request)
        {
            return MovimientoBEBL.MovimientoBE_Guardar(request);
        }


        [HttpDelete("MovimientoBE_Eliminar")]
        public ErrorDto MovimientoBE_Eliminar(string movimiento, int modulo)
        {
            return MovimientoBEBL.MovimientoBE_Eliminar(movimiento, modulo);
        }
    }
}
