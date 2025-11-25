using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.Controllers
{
    [Route("api/FrmPgxClientesTiposIDs")]
    [Route("api/frmPGX_ClientesTiposIDs")]
    [ApiController]
    public class FrmPgxClientesTiposIDsController : ControllerBase
    {
        readonly FrmPgxClientesTiposIDsBl ClientesTiposIDsBL;

        public FrmPgxClientesTiposIDsController(IConfiguration config)
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