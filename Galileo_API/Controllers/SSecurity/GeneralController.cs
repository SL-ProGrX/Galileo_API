using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.Security;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeneralController : ControllerBase
    {
        private readonly IConfiguration _config;

        public GeneralController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("PadronConsultar")]
        //[Authorize]
        public List<PadronConsultarResponseDto> PadronConsultar(PadronConsultarRequestDto padronConsultarRequestDto)
        {
            return new GeneralBL(_config).PadronConsultar(padronConsultarRequestDto);
        }

        [HttpPost("ValidaCuenta")]
        public ErrorGeneralDto ValidaCuenta(ValidaCuentaRequestDto validaCuentaRequestDto)
        {
            return new GeneralBL(_config).ValidaCuenta(validaCuentaRequestDto);
        }
    }
}
