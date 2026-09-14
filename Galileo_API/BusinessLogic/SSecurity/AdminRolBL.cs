using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.BusinessLogic
{
    public class AdminRolBL
    {
        private readonly IConfiguration _config;

        public AdminRolBL(IConfiguration config)
        {
            _config = config;
        }
        public ErrorDto<List<UsuarioPlataforma>> UsuarioPlataforma_Obtener(string? usuarioFiltro)
        {
            return new AdminRolDB(_config).UsuarioPlataforma_Obtener(usuarioFiltro);
        }

        public ErrorDto<List<UsuarioAdmin>> UsuarioAdmin_Obtener(string? usuarioFiltro)
        {
            return new AdminRolDB(_config).UsuarioAdmin_Obtener(usuarioFiltro);
        }

        public ErrorDto<List<ClienteAsignado>> ClientesAsigna_Obtener(string usuario, string? ClienteFiltro)
        {
            return new AdminRolDB(_config).ClientesAsigna_Obtener(usuario, ClienteFiltro);
        }

        public ErrorDto<AdminLocalRoles> AdminRoles_Obtener(string usuario)
        {
            return new AdminRolDB(_config).AdminRoles_Obtener(usuario);
        }

        public ErrorDto<AdminLocalRolesCliente> AdminRolesCliente_Obtener(string usuario, int cliente)
        {
            return new AdminRolDB(_config).AdminRolesCliente_Obtener(usuario, cliente);
        }

        public ErrorDto AdminLocal_Insertar(AdminLocalInsert request)
        {
            return new AdminRolDB(_config).AdminLocal_Insertar(request);
        }

        public ErrorDto AdminClienteRoles_Insertar(AdminLocalRolesInsert request)
        {
            return new AdminRolDB(_config).AdminClienteRoles_Insertar(request);
        }
    }
}
