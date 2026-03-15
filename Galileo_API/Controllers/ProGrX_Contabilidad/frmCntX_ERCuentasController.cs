using Galileo.Models;
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
    public class FrmCntXErCuentasController : ControllerBase
    {
        private readonly FrmCntXErCuentasBL _bl;

        public FrmCntXErCuentasController(IConfiguration config)
        {
            _bl = new FrmCntXErCuentasBL(config);
        }

        [HttpGet("CntXInvPeriodico_Lista")]
        public ActionResult<ErrorDto<List<CntXInvPeriodicoDto>>> CntXInvPeriodico_Lista([FromQuery] int codEmpresa, [FromQuery] int codContabilidad)
            => _bl.CntXInvPeriodico_Lista(codEmpresa, codContabilidad);

        [HttpGet("CntXCuentasClasificacion")]
        public ActionResult<ErrorDto<List<DropDownListaGenericaModel>>> CntXCuentasClasificacion([FromQuery] int codEmpresa, [FromQuery] int codContabilidad)
            => _bl.CntXCuentasClasificacion(codEmpresa, codContabilidad);

        [HttpPost("CntXInvPeriodico_Guardar")]
        public ActionResult<ErrorDto<bool>> CntXInvPeriodico_Guardar([FromQuery] int codEmpresa, [FromBody] CntXInvPeriodicoSaveParams param)
           => _bl.CntXInvPeriodico_Guardar(codEmpresa, param);

        [HttpDelete("CntXInvPeriodico_Eliminar")]
        public ActionResult<ErrorDto<bool>> CntXInvPeriodico_Eliminar([FromQuery] int codEmpresa, [FromBody] CntXInvPeriodicoDeleteParams param)
            => _bl.CntXInvPeriodico_Eliminar(codEmpresa, param);

        [HttpGet("CntXCuentasClasificacionA_Validar")]
        public ActionResult<ErrorDto<int>> CntXCuentasClasificacionA_Validar([FromQuery] int codEmpresa, [FromQuery] int codContabilidad, [FromQuery] string codCuenta)
            => _bl.CntXCuentasClasificacionA_Validar(codEmpresa, codContabilidad, codCuenta);
    }
}
