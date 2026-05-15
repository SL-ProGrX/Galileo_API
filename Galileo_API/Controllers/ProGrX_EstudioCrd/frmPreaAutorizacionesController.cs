using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using Galileo_API.BusinessLogic.ProGrX_EstudioCrd;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Galileo_API.Controllers.ProGrX_EstudioCrd
{
    [ApiController]
    [Route("api/[controller]")]
    public class FrmPreaAutorizacionesController : ControllerBase
    {
        private readonly FrmPreaAutorizacionesBL _bl;
        public FrmPreaAutorizacionesController(IConfiguration config)
        {
            _bl = new FrmPreaAutorizacionesBL(config);
        }

        [HttpGet("PreaAutorizaciones_ObtenerComite")]
        public ActionResult<ErrorDto<PreaComiteIdDto>> PreaAutorizaciones_ObtenerComite(
            [FromQuery] int codEmpresa,
            [FromQuery] string expediente)
            => _bl.PreaAutorizaciones_ObtenerComite(codEmpresa, expediente);

        [HttpGet("PreaAutorizaciones_ObtenerMiembros")]
        public ActionResult<ErrorDto<List<PreaComiteMiembroDto>>> PreaAutorizaciones_ObtenerMiembros(
            [FromQuery] int codEmpresa,
            [FromQuery] int comite,
            [FromQuery] string expediente)
            => _bl.PreaAutorizaciones_ObtenerMiembros(codEmpresa, comite, expediente);

        [HttpPost("PreaAutorizaciones_Insertar")]
        public ActionResult<ErrorDto<bool>> PreaAutorizaciones_Insertar(
            [FromQuery] int codEmpresa,
            [FromBody] PreaAutorizadorRequestDto request)
            => _bl.PreaAutorizaciones_Insertar(codEmpresa, request);

        [HttpDelete("PreaAutorizaciones_Eliminar")]
        public ActionResult<ErrorDto<bool>> PreaAutorizaciones_Eliminar(
            [FromQuery] int codEmpresa,
            [FromQuery] string expediente,
            [FromQuery] string cedula)
            => _bl.PreaAutorizaciones_Eliminar(codEmpresa, expediente, cedula);
    }
}
