using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PortalWebAppSincronizaController : ControllerBase
    {
        private readonly IConfiguration _config;

        public PortalWebAppSincronizaController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("ServidorPrincipalSincronizar")]
        public ErrorDto ServidorPrincipalSincronizar()
        {
            return new PortalWebAppSincronizaBL(_config).ServidorPrincipalSincronizar();
        }


        [HttpGet("SincronizarWebApps")]
        public ErrorDto SincronizarWebApps(int paso, string server)
        {
            return new PortalWebAppSincronizaBL(_config).SincronizarWebApps(paso, server);
        }

    }
}
