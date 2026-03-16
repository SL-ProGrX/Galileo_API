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
    public class FrmCntXPlantillaAsientosGeneraController : ControllerBase
    {
        private readonly FrmCntXPlantillaAsientosGeneraBL _bl;

        public FrmCntXPlantillaAsientosGeneraController(IConfiguration config)
        {
            _bl = new FrmCntXPlantillaAsientosGeneraBL(config);
        }

        [HttpGet("CntXPlantillaAsientos_Lista")]
        public ActionResult<ErrorDto<List<CntXPlantillaAsientosDto>>> CntXPlantillaAsientos_Lista([FromQuery] int codEmpresa, [FromQuery] int codContabilidad)
            => _bl.CntXPlantillaAsientos_Lista(codEmpresa, codContabilidad);

        [HttpGet("CntXPlantillaAsientos_Get")]
        public ActionResult<ErrorDto<CntXPlantillaAsientosDto?>> CntXPlantillaAsientos_Get([FromQuery] int codEmpresa, [FromQuery] int codContabilidad, [FromQuery] string codPlantilla)
           => _bl.CntXPlantillaAsientos_Get(codEmpresa, codContabilidad, codPlantilla);

        [HttpPut("CntXPlantillaAsientos_UpdateConsecutivo")]
        public ActionResult<ErrorDto<bool>> CntXPlantillaAsientos_UpdateConsecutivo([FromQuery] int codEmpresa, [FromBody] CntXPlantillaAsientosUpdateParams param)
            => _bl.CntXPlantillaAsientos_UpdateConsecutivo(codEmpresa, param);

        [HttpPost("CntxAsientos_Insert")]
        public ActionResult<ErrorDto<bool>> CntxAsientos_Insert([FromQuery] int codEmpresa, [FromBody] CntxAsientosInsertParams param)
            => _bl.CntxAsientos_Insert(codEmpresa, param);

        [HttpGet("CntXPlantillaDetalle_Lista")]
        public ActionResult<ErrorDto<List<CntXPlantillaDetalleDto>>> CntXPlantillaDetalle_Lista([FromQuery] int codEmpresa, [FromQuery] int codContabilidad, [FromQuery] string codPlantilla)
            => _bl.CntXPlantillaDetalle_Lista(codEmpresa, codContabilidad, codPlantilla);

        [HttpPost("CntxAsientosDetalle_Insert")]
        public ActionResult<ErrorDto<bool>> CntxAsientosDetalle_Insert([FromQuery] int codEmpresa, [FromBody] CntxAsientosDetalleInsertParams param)
            => _bl.CntxAsientosDetalle_Insert(codEmpresa, param);

        [HttpGet("CntXPeriodos_ExisteAbierto")]
        public ActionResult<ErrorDto<int>> CntXPeriodos_ExisteAbierto([FromQuery] int codEmpresa, [FromQuery] int codContabilidad, [FromQuery] int anio, [FromQuery] int mes)
            => _bl.CntXPeriodos_ExisteAbierto(codEmpresa, codContabilidad, anio, mes);
    }
}
