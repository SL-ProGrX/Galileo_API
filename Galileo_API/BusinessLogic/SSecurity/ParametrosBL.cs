using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.BusinessLogic
{
    public class ParametrosBL
    {
        private readonly IConfiguration _config;

        public ParametrosBL(IConfiguration config)
        {
            _config = config;
        }

        public ParametrosDto Parametros_Obtener()
        {
            return new ParametrosDB(_config).Parametros_Obtener();
        }

        public ErrorDto Parametros_Insertar(ParametrosDto request)
        {
            return new ParametrosDB(_config).Parametros_Insertar(request);
        }
    }
}
