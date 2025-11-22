using Galileo.DataBaseTier;
using Galileo.Models.Security;

namespace Galileo.BusinessLogic
{
    public class AppLogBL
    {
        private readonly IConfiguration _config;

        public AppLogBL(IConfiguration config)
        {
            _config = config;
        }

        public List<AppLog> AppLog_ObtenerTodos(int empresa, string ini, string fin)
        {
            return new AppLogDB(_config).AppLog_ObtenerTodos(empresa, ini, fin);
        }
    }
}
