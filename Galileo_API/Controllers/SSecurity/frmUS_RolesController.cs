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
    public class FrmUsRolesController : ControllerBase
    {
        readonly FrmUsRolesBl RolesBL;

        public FrmUsRolesController(IConfiguration config)
        {
            RolesBL = new FrmUsRolesBl(config);
        }

        [HttpGet("RolFiltroObtener")]
        public ErrorDto<List<RolesObtenerDto>> RolFiltroObtener(string filtro)
        {
            return RolesBL.RolFiltroObtener(filtro);
        }

        [HttpGet("RolesObtener")]
        public ErrorDto<List<RolesObtenerDto>> RolesObtener()
        {
            return RolesBL.RolesObtener();
        }

        [HttpPost("RolGuardar")]
        public ErrorDto RolGuardar(RolInsertarDto req)
        {
            return RolesBL.RolGuardar(req);
        }

        [HttpDelete("RolEliminar")]
        public ErrorDto RolEliminar(string RolId, int codEmpresa = 0, string usuario = "")
        {
            return RolesBL.RolEliminar(RolId, codEmpresa, usuario);
        }

        [HttpPost("RolesVincular")]
        public ErrorDto RolesVincular(RolesVincularDto req)
        {
            return RolesBL.RolesVincular(req);
        }

        [HttpGet("ClientesObtener")]
        public ErrorDto<List<ClientesObtenerDto>> ClientesObtener()
        {
            return RolesBL.ClientesObtener();
        }
    }
}
