using Galileo.DataBaseTier;
using Galileo.Models.Security;

namespace Galileo.BusinessLogic
{
    public class BitacoraBL
    {
        private readonly IConfiguration _config;

        public BitacoraBL(IConfiguration config)
        {
            _config = config;
        }

        public List<BitacoraResultDto> BitacoraObtener(BitacoraRequestDto bitacoraRequestDto)
        {
            return new BitacoraDb(_config).BitacoraObtener(bitacoraRequestDto);
        }
    }
}