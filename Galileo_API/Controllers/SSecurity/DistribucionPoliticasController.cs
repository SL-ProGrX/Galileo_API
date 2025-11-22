using Microsoft.AspNetCore.Mvc;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DistribucionPoliticasController : ControllerBase
    {
        private readonly IConfiguration _config;

        public DistribucionPoliticasController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("PaisObtener")]
        public List<PaisObtenerDto> PaisObtener()
        {
            return new DistribucionPoliticasDb(_config).PaisObtener();
        }

        [HttpGet("ProvinciasObtener")]
        public List<ProvinciasObtenerDto> ProvinciasObtener(string CodPais)
        {
            return new DistribucionPoliticasDb(_config).ProvinciasObtener(CodPais);
        }

        [HttpGet("CantonesObtener")]
        public List<CantonesObtenerDto> CantonesObtener(string CodPais, string CodProvincia)
        {
            return new DistribucionPoliticasDb(_config).CantonesObtener(CodPais, CodProvincia);
        }

        [HttpGet("DistritosObtener")]
        public List<DistritosObtenerDto> DistritosObtener(string CodPais, string CodProvincia, string CodCanton)
        {
            return new DistribucionPoliticasDb(_config).DistritosObtener(CodPais, CodProvincia, CodCanton);
        }

        [HttpPost("FxGuardar")]
        public ErrorDto FxGuardar(GuardarDto dto)
        {
            return new DistribucionPoliticasDb(_config).FxGuardar(dto);
        }
    }
}