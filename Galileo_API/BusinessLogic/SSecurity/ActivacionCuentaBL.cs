using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.BusinessLogic
{
    public class ActivacionCuentaBL
    {
        private readonly IConfiguration _config;

        public ActivacionCuentaBL(IConfiguration config)
        {
            _config = config;
        }

        public ActivacionCuentaDto UsuarioEstado_Obtener(string user)
        {
            return new ActivacionCuentaDB(_config).UsuarioEstado_Obtener(user);
        }

        public ErrorDto UsuarioEstado_Actualizar(ActivacionCuentaDto request)
        {
            return new ActivacionCuentaDB(_config).UsuarioEstado_Actualizar(request);
        }
    }
}
