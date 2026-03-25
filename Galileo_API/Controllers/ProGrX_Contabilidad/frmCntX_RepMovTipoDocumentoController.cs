using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [ApiController]
    [Route("api/[controller]")]
    public class FrmCntXRepMovTipoDocumentoController : ControllerBase
    {
        private readonly FrmCntXRepMovTipoDocumentoBL _bl;

        public FrmCntXRepMovTipoDocumentoController(IConfiguration config)
        {
            _bl = new FrmCntXRepMovTipoDocumentoBL(config);
        }

        [HttpGet("TiposAsiento_Lista")]
        public ActionResult<ErrorDto<List<CntXTipoAsientoDto>>> TiposAsiento_Lista(
            [FromQuery] int codEmpresa,
            [FromQuery] int codContabilidad)
            => _bl.TiposAsiento_Lista(codEmpresa, codContabilidad);

        [HttpPost("Asientos_Lista")]
        public ActionResult<ErrorDto<List<CntXAsientoDto>>> Asientos_Lista(
            [FromQuery] int codEmpresa,
            [FromBody] CntXAsientoParams param)
            => _bl.Asientos_Lista(codEmpresa, param);
    }
}
