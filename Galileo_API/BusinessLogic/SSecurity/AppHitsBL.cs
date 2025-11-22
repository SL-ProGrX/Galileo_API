using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.BusinessLogic
{
    public class AppHitsBL
    {
        private readonly IConfiguration _config;

        public AppHitsBL(IConfiguration config)
        {
            _config = config;
        }

        public List<AppHits> AppHits_ObtenerTodos()
        {
            return new AppHitsDB(_config).AppHits_ObtenerTodos();
        }

        public ErrorDto AppHits_Insertar(AppHits request)
        {
            return new AppHitsDB(_config).AppHits_Insertar(request);
        }

        public ErrorDto AppHits_Eliminar(AppHits request)
        {
            return new AppHitsDB(_config).AppHits_Eliminar(request);
        }

        public ErrorDto AppHits_Actualizar(AppHits request)
        {
            return new AppHitsDB(_config).AppHits_Actualizar(request);
        }
    }
}
