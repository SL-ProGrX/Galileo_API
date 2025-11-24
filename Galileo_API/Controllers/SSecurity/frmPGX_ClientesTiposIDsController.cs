using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmPgx_ClientesTiposIDsController : ControllerBase
    {
        readonly FrmPgxClientesTiposIDsBl ClientesTiposIDsBL;

        public FrmPgx_ClientesTiposIDsController(IConfiguration config)
        {
            ClientesTiposIDsBL = new FrmPgxClientesTiposIDsBl(config);
        }

        [HttpGet("TipoId_ObtenerTodos")]
        // [Authorize]
        public List<TipoId> TiposId_ObtenerTodos()
        {
            return ClientesTiposIDsBL.TipoId_ObtenerTodos();
        }

        [HttpDelete("TipoId_Eliminar")]
        //[Authorize]
        public ErrorDto TipoId_Eliminar(string tipo_id)
        {
            return ClientesTiposIDsBL.TipoId_Eliminar(tipo_id);
        }

        [HttpPost("TipoId_Guardar")]
        //[Authorize]
        public ErrorDto TipoId_Guardar(TipoId request)
        {
            return ClientesTiposIDsBL.TipoId_Guardar(request);
        }
    }
}