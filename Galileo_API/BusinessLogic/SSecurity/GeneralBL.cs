using Galileo.DataBaseTier;
using Galileo.Models.Security;

namespace Galileo.BusinessLogic
{
    public class GeneralBL
    {
        private readonly IConfiguration _config;

        public GeneralBL(IConfiguration config)
        {
            _config = config;
        }

        public List<PadronConsultarResponseDto> PadronConsultar(PadronConsultarRequestDto padronConsultarDto)
        {
            return new GeneralDB(_config).PadronConsultar(padronConsultarDto);
        }

        public ErrorGeneralDto ValidaCuenta(ValidaCuentaRequestDto validaCuentaRequestDto)
        {
            return new GeneralDB(_config).ValidaCuenta(validaCuentaRequestDto);
        }
    }
}
