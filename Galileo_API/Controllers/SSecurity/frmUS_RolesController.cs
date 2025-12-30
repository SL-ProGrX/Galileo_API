using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/FrmUsRoles")]
    [Route("api/frmUS_Roles")]
    [ApiController]
    [Authorize]
    public class FrmUsRolesController : ControllerBase
    {
        readonly FrmUsRolesBl RolesBL;

        public FrmUsRolesController(IConfiguration config)
        {
            RolesBL = new FrmUsRolesBl(config);
        }

        [HttpGet("RolFiltroObtener")]
        public List<RolesObtenerDto> RolFiltroObtener(string filtro)
        {
            return RolesBL.RolFiltroObtener(filtro);
        }

        [HttpGet("RolesObtener")]
        public List<RolesObtenerDto> RolesObtener()
        {
            return RolesBL.RolesObtener();
        }

        [HttpPost("RolGuardar")]
        public ErrorDto RolGuardar(RolInsertarDto req)
        {
            return RolesBL.RolGuardar(req);
        }

        [HttpDelete("RolEliminar")]
        public ErrorDto RolEliminar(string RolId)
        {
            return RolesBL.RolEliminar(RolId);
        }

        [HttpGet("ClientesObtener")]
        public List<ClientesObtenerDto> ClientesObtener()
        {
            return RolesBL.ClientesObtener();
        }
    }
}
