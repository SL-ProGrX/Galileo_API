using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Fondos;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.Controllers.ProGrX.Fondos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFndConciliacionTeledolarSinpeController : ControllerBase
    {
        private readonly FrmFndConciliacionTeledolarSinpeBl _bl;

        public FrmFndConciliacionTeledolarSinpeController(IConfiguration config)
        {
            _bl = new FrmFndConciliacionTeledolarSinpeBl(config);
        }

        [Authorize]
        [HttpPost("ConciliacionTeledolarSinpe_Obtener")]
        public ErrorDto<List<FndConciliacionTeledolarSinpeResult>> ConciliacionTeledolarSinpe_Obtener([FromBody] FndConciliacionTeledolarSinpeParams param)
        {
            return _bl.ConciliacionTeledolarSinpe_Obtener(param);
        }
    }
}