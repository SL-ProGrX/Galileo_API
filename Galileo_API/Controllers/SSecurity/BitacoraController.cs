using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.Security;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BitacoraController : ControllerBase
    {
        private readonly IConfiguration _config;

        public BitacoraController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("BitacoraObtener")]
        //[Authorize]
        public List<BitacoraResultDto> BitacoraObtener(BitacoraRequestDto bitacoraRequestDto)
        {
            return new BitacoraBL(_config).BitacoraObtener(bitacoraRequestDto);
        }
    }
}