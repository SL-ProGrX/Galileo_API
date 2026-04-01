using Galileo_API.BusinessLogic.ProGrX_Comites;
using Galileo_API.Models.ProGrX_Comites;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Galileo_API.Controllers.ProGrX_Comites
{
    [ApiController]
    [Route("api/[controller]")]
    public class FrmAfCdParametrosController : ControllerBase
    {
        private readonly FrmAfCdParametrosBL _bl;

        public FrmAfCdParametrosController(IConfiguration config)
        {
            _bl = new FrmAfCdParametrosBL(config);
        }

        [HttpGet("AfCdParametros_Lista")]
        public ActionResult<ErrorDto<List<AfCdParametroDto>>> AfCdParametros_Lista([FromQuery] int codEmpresa)
            => _bl.AfCdParametros_Lista(codEmpresa);

        [HttpPost("AfCdParametros_Update")]
        public ActionResult<ErrorDto<bool>> AfCdParametros_Update([FromQuery] int codEmpresa, [FromBody] AfCdParametroUpdateDto dto)
            => _bl.AfCdParametros_Update(codEmpresa, dto);
    }
}
