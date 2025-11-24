using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmUs_RolesController : ControllerBase
    {
        readonly FrmUsRolesBl RolesBL;

        public FrmUs_RolesController(IConfiguration config)
        {
            RolesBL = new FrmUsRolesBl(config);
        }

        [HttpGet("RolFiltroObtener")]
        //[Authorize]
        public List<RolesObtenerDto> RolFiltroObtener(string filtro)
        {
            return RolesBL.RolFiltroObtener(filtro);
        }

        [HttpGet("RolesObtener")]
        //[Authorize]
        public List<RolesObtenerDto> RolesObtener()
        {
            return RolesBL.RolesObtener();
        }

        [HttpPost("RolGuardar")]
        // [Authorize]
        public ErrorDto RolGuardar(RolInsertarDto req)
        {
            return RolesBL.RolGuardar(req);
        }

        [HttpDelete("RolEliminar")]
        //[Authorize]
        public ErrorDto RolEliminar(string RolId)
        {
            return RolesBL.RolEliminar(RolId);
        }

        [HttpGet("ClientesObtener")]
        //[Authorize]
        public List<ClientesObtenerDto> ClientesObtener()
        {
            return RolesBL.ClientesObtener();
        }
    }
}
