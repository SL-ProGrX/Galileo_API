using Galileo_API.BusinessLogic.ProGrX_Comites;
using Galileo_API.Models.ProGrX_Comites;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Galileo_API.Controllers.ProGrX_Comites
{
    [ApiController]
    [Route("api/[controller]")]
    public class FrmAfCdMiembrosJuntaController : ControllerBase
    {
        private readonly FrmAfCdMiembrosJuntaBL _bl;

        public FrmAfCdMiembrosJuntaController(IConfiguration config)
        {
            _bl = new FrmAfCdMiembrosJuntaBL(config);
        }

        [HttpGet("AfCdDirectores_Lista")]
        public ActionResult<ErrorDto<List<AfCdDirectorDto>>> AfCdDirectores_Lista([FromQuery] int codEmpresa)
            => _bl.AfCdDirectores_Lista(codEmpresa);

        [HttpGet("AfCdDirectores_ValidarComite")]
        public ActionResult<ErrorDto<List<AfCdComiteDirectorDto>>> AfCdDirectores_ValidarComite([FromQuery] int codEmpresa, [FromQuery] int codDirector)
            => _bl.AfCdDirectores_ValidarComite(codEmpresa, codDirector);

        [HttpPost("AfCdDirectores_Guardar")]
        public ActionResult<ErrorDto<bool>> AfCdDirectores_Guardar([FromQuery] int codEmpresa, [FromBody] AfCdDirectorSaveDto dto)
            => _bl.AfCdDirectores_Guardar(codEmpresa, dto);

        [HttpDelete("AfCdDirectores_Eliminar")]
        public ActionResult<ErrorDto<bool>> AfCdDirectores_Eliminar(
            [FromQuery] int codEmpresa,
            [FromQuery] int codDirector,
            [FromQuery] string usuario)
            => _bl.AfCdDirectores_Eliminar(codEmpresa, codDirector, usuario);
    }
}
