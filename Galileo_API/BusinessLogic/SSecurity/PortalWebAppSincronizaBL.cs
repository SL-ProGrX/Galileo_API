using Galileo.DataBaseTier;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic
{
    public class PortalWebAppSincronizaBL
    {

        private readonly IConfiguration _config;

        public PortalWebAppSincronizaBL(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto ServidorPrincipalSincronizar()
        {
            return new PortalWebAppSincronizaDb(_config).ServidorPrincipalSincronizar();
        }

        public ErrorDto SincronizarWebApps(int paso, string server)
        {
            return new PortalWebAppSincronizaDb(_config).SincronizarWebApps(paso, server);
        }
    }
}