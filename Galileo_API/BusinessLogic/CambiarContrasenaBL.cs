using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic
{
    public class CambiarContrasenaBL
    {
        private readonly IConfiguration _config;

        public CambiarContrasenaBL(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<ParametrosObtenerDto> ParametrosObtener()
        {
            return new CambiarContrasenaDB(_config).ParametrosObtener();
        }

        public ErrorDto<List<string>> KeyHistoryObtener(string Usuario, int topQuantity)
        {
            return new CambiarContrasenaDB(_config).KeyHistoryObtener(Usuario, topQuantity);
        }

        public ErrorDto CambiarClave(ClaveCambiarDto cambioClave)
        {
            return new CambiarContrasenaDB(_config).CambiarClave(cambioClave);
        }
    }
}
