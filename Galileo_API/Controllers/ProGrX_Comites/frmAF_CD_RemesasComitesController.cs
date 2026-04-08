using Galileo_API.BusinessLogic.ProGrX_Comites;
using Galileo_API.Models.ProGrX_Comites;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Galileo_API.Controllers.ProGrX_Comites
{
    [ApiController]
    [Route("api/[controller]")]
    public class FrmAfCdRemesasComitesController : ControllerBase
    {
        private readonly FrmAfCdRemesasComitesBL _bl;

        public FrmAfCdRemesasComitesController(IConfiguration config)
        {
            _bl = new FrmAfCdRemesasComitesBL(config);
        }

        [HttpGet("AfCdRemesasTes_Lista")]
        public ActionResult<ErrorDto<List<AfCdRemesaTesDto>>> AfCdRemesasTes_Lista([FromQuery] int codEmpresa)
            => _bl.AfCdRemesasTes_Lista(codEmpresa);

        [HttpPost("AfCdRemesasTes_Guardar")]
        public ActionResult<ErrorDto<bool>> AfCdRemesasTes_Guardar([FromQuery] int codEmpresa, [FromBody] AfCdRemesaTesSaveDto dto)
            => _bl.AfCdRemesasTes_Guardar(codEmpresa, dto);

        [HttpDelete("AfCdRemesasTes_Eliminar")]
        public ActionResult<ErrorDto<bool>> AfCdRemesasTes_Eliminar([FromQuery] int codEmpresa, [FromQuery] int codRemesa)
            => _bl.AfCdRemesasTes_Eliminar(codEmpresa, codRemesa);
    }
}
