using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.Security;
using Galileo.Models.ERROR;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminRolController : ControllerBase
    {
        private readonly IConfiguration _config;

        public AdminRolController(IConfiguration config)
        {
            _config = config;
        }


        [HttpGet("UsuarioPlataforma_Obtener")]
        // [Authorize]
        public List<UsuarioPlataforma> UsuarioPlataforma_Obtener(string? usuarioFiltro)
        {
            return new AdminRolBL(_config).UsuarioPlataforma_Obtener(usuarioFiltro);
        }

        [HttpGet("UsuarioAdmin_Obtener")]
        // [Authorize]
        public List<UsuarioAdmin> UsuarioAdmin_Obtener(string? usuarioFiltro)
        {
            return new AdminRolBL(_config).UsuarioAdmin_Obtener(usuarioFiltro);
        }

        [HttpGet("ClientesAsigna_Obtener")]
        // [Authorize]
        public List<ClienteAsignado> ClientesAsigna_Obtener(string usuario, string? ClienteFiltro)
        {
            return new AdminRolBL(_config).ClientesAsigna_Obtener(usuario, ClienteFiltro);
        }

        [HttpGet("AdminRoles_Obtener")]
        // [Authorize]
        public AdminLocalRoles AdminRoles_Obtener(string usuario)
        {
            return new AdminRolBL(_config).AdminRoles_Obtener(usuario);
        }

        [HttpGet("AdminRolesCliente_Obtener")]
        // [Authorize]
        public AdminLocalRolesCliente AdminRolesCliente_Obtener(string usuario, int cliente)
        {
            return new AdminRolBL(_config).AdminRolesCliente_Obtener(usuario, cliente);
        }

        [HttpPost("AdminLocal_Insertar")]
        // [Authorize]
        public ErrorDto AdminLocal_Insertar(AdminLocalInsert request)
        {
            return new AdminRolBL(_config).AdminLocal_Insertar(request);
        }

        [HttpPost("AdminClienteRoles_Insertar")]
        // [Authorize]
        public ErrorDto AdminClienteRoles_Insertar(AdminLocalRolesInsert request)
        {
            return new AdminRolBL(_config).AdminClienteRoles_Insertar(request);
        }

    }
}
