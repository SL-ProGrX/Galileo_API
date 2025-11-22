using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.BusinessLogic
{
    public class UsuarioBL
    {

        private readonly IConfiguration _config;

        public UsuarioBL(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto UsuarioCuentaRevisar(UsuarioCuentaRevisarDto cuentaUsuarioRevisarDto)
        {
            return new UsuarioDB(_config).UsuarioCuentaRevisar(cuentaUsuarioRevisarDto);
        }

        public UsuarioCuentaRevisarDto UsuarioCuentaObtener(string nombreUsuario)
        {
            return new UsuarioDB(_config).UsuarioCuentaObtener(nombreUsuario);
        }

        public List<UsuarioCuentaMovimientoResultDto> UsuarioCuentaMovimientosObtener(UsuarioCuentaMovimientoRequestDto usuarioCuentaMovimientoRequestDto)
        {
            return new UsuarioDB(_config).UsuarioCuentaMovimientosObtener(usuarioCuentaMovimientoRequestDto);
        }

    }
}