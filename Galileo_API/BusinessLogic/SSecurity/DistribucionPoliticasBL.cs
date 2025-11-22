using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.BusinessLogic
{
    public class DistribucionPoliticasBL
    {
        private readonly IConfiguration _config;

        public DistribucionPoliticasBL(IConfiguration config)
        {
            _config = config;
        }

        public List<PaisObtenerDto> PaisObtener()
        {
            return new DistribucionPoliticasDb(_config).PaisObtener();
        }
        public List<ProvinciasObtenerDto> ProvinciasObtener(string CodPais)
        {
            return new DistribucionPoliticasDb(_config).ProvinciasObtener(CodPais);
        }
        public List<CantonesObtenerDto> CantonesObtener(string CodPais, string CodProvincia)
        {
            return new DistribucionPoliticasDb(_config).CantonesObtener(CodPais, CodProvincia);
        }

        public List<DistritosObtenerDto> DistritosObtener(string CodPais, string CodProvincia, string CodCanton)
        {
            return new DistribucionPoliticasDb(_config).DistritosObtener(CodPais, CodProvincia, CodCanton);
        }

        public ErrorDto FxGuardar(GuardarDto dto)
        {
            return new DistribucionPoliticasDb(_config).FxGuardar(dto);
        }
    }
}