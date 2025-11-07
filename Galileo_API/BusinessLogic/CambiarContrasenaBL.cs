using PgxAPI.DataBaseTier;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;

namespace PgxAPI.BusinessLogic
{
    public class CambiarContrasenaBL
    {
        private readonly IConfiguration _config;

        public CambiarContrasenaBL(IConfiguration config)
        {
            _config = config;
        }

        public ParametrosObtenerDto ParametrosObtener()
        {
            return new CambiarContrasenaDB(_config).ParametrosObtener();
        }

        public List<string> KeyHistoryObtener(string Usuario, int topQuantity)
        {
            return new CambiarContrasenaDB(_config).KeyHistoryObtener(Usuario, topQuantity);
        }

        public ErrorDto CambiarClave(ClaveCambiarDto cambioClave)
        {
            return new CambiarContrasenaDB(_config).CambiarClave(cambioClave);
        }
    }
}
