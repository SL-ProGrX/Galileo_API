using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [ApiController]
    [Route("api/[controller]")]
    public class FrmCntXDiferidosGeneracionController : ControllerBase
    {
        private readonly FrmCntXDiferidosGeneracionBL _bl;

        public FrmCntXDiferidosGeneracionController(IConfiguration config)
        {
            _bl = new FrmCntXDiferidosGeneracionBL(config);
        }

        [HttpPost("Diferidos_Pendientes_Lista")]
        public ActionResult<ErrorDto<List<CntXDiferidoPendienteDto>>> Diferidos_Pendientes_Lista([FromQuery] int codEmpresa, [FromBody] CntXDiferidoPendienteParams param)
            => _bl.Diferidos_Pendientes_Lista(codEmpresa, param);

        [HttpPost("Diferido_Asiento")]
        public ActionResult<ErrorDto<CntXDiferidoAsientoResult?>> Diferido_Asiento([FromQuery] int codEmpresa, [FromBody] CntXDiferidoAsientoParams param)
            => _bl.Diferido_Asiento(codEmpresa, param);
    }
}
