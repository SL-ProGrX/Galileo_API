using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.BusinessLogic
{
    public class PerfilUsuarioBL
    {
        private readonly IConfiguration _config;

        public PerfilUsuarioBL(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<PerfilUsuarioDto> PerfilUsuario_Obtener(string usuario)
        {
            return new PerfilUsuarioDB(_config).UsuarioPerfilConsultar(usuario);
        }

        public ErrorDto PerfilUsuario_Actualizar(PerfilUsuarioDto request)
        {
            return new PerfilUsuarioDB(_config).PerfilUsuario_Actualizar(request);
        }
    }
}
