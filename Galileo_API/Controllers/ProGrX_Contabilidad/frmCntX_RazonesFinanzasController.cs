using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [ApiController]
    [Route("api/[controller]")]
    public class FrmCntXRazonesFinanzasController : ControllerBase
    {
        private readonly FrmCntXRazonesFinanzasBL _bl;

        public FrmCntXRazonesFinanzasController(IConfiguration config)
        {
            _bl = new FrmCntXRazonesFinanzasBL(config);
        }


        [HttpGet("CntXRazonesFinanzas_Lista")]
        public ActionResult<ErrorDto<List<CntXRazonesFinanzasDto>>> CntXRazonesFinanzas_Lista([FromQuery] int codEmpresa, [FromQuery] int codContabilidad)
            => _bl.CntXRazonesFinanzas_Lista(codEmpresa, codContabilidad);

        [HttpGet("CntXRazonesFinanzas_Existe")]
        public ActionResult<ErrorDto<bool>> CntXRazonesFinanzas_Existe([FromQuery] int codEmpresa, [FromQuery] int codContabilidad)
            => _bl.CntXRazonesFinanzas_Existe(codEmpresa, codContabilidad);

        [HttpPost("CntXRazonesFinanzas_Guardar")]
        public ActionResult<ErrorDto<bool>> CntXRazonesFinanzas_Guardar([FromQuery] int codEmpresa, [FromBody] CntXRazonesFinanzasSaveParams param)
            => _bl.CntXRazonesFinanzas_Guardar(codEmpresa, param);

        [HttpGet("CntXRazonFinanciera_Lista")]
        public ActionResult<ErrorDto<List<CntXRazonFinancieraDto>>> CntXRazonFinanciera_Lista([FromQuery] int codEmpresa, [FromQuery] int codContabilidad)
            => _bl.CntXRazonFinanciera_Lista(codEmpresa, codContabilidad);

        [HttpGet("CntXRazonFinancieraTipos_Lista")]
        public ActionResult<ErrorDto<List<CntXRazonFinancieraTipoDto>>> CntXRazonFinancieraTipos_Lista([FromQuery] int codEmpresa, [FromQuery] int codContabilidad)
            => _bl.CntXRazonFinancieraTipos_Lista(codEmpresa, codContabilidad);

        [HttpPost("CntXRazonFinanciera_Guardar")]
        public ActionResult<ErrorDto<bool>> CntXRazonFinanciera_Guardar([FromQuery] int codEmpresa, [FromBody] CntXRazonFinancieraSaveParams param)
            => _bl.CntXRazonFinanciera_Guardar(codEmpresa, param);

        [HttpGet("CntXRazonFinancieraGrupos_Combo")]
        public ActionResult<ErrorDto<List<DropDownListaGenericaModel>>> CntXRazonFinancieraGrupos_Combo([FromQuery] int codEmpresa, [FromQuery] int codContabilidad)
            => _bl.CntXRazonFinancieraGrupos_Combo(codEmpresa, codContabilidad);


        [HttpGet("CntXRazonFinancieraSimple_Lista")]
        public ActionResult<ErrorDto<List<DropDownListaGenericaModel>>> CntXRazonFinancieraSimple_Lista([FromQuery] int codEmpresa, [FromQuery] int codContabilidad, [FromQuery] string codGrupo, [FromQuery] string orden = "cod_razon")
            => _bl.CntXRazonFinancieraSimple_Lista(codEmpresa, codContabilidad, codGrupo, orden);

        [HttpGet("CntXRazonFinanciera_Notas")]
        public ActionResult<ErrorDto<CntXRazonNotasDto>> CntXRazonFinanciera_Notas([FromQuery] int codEmpresa, [FromQuery] int codContabilidad,[FromQuery] string codGrupo, [FromQuery] string codRazon)
            => _bl.CntXRazonFinanciera_Notas(codEmpresa, codContabilidad, codGrupo, codRazon);

        [HttpGet("CntXRazonFinanciera_Detalle")]
        public ActionResult<ErrorDto<List<CntXRazonDetalleDto>>> CntXRazonFinanciera_Detalle([FromQuery] int codEmpresa, [FromQuery] int codContabilidad,[FromQuery] string codRazon)
            => _bl.CntXRazonFinanciera_Detalle(codEmpresa, codContabilidad, codRazon);

        [HttpGet("CntXRazonDetalle_ProximoIdx")]
        public ActionResult<ErrorDto<CntXRazonDetalleIdxDto>> CntXRazonDetalle_ProximoIdx([FromQuery] int codEmpresa, [FromQuery] int codContabilidad, [FromQuery] string codRazon)
            => _bl.CntXRazonDetalle_ProximoIdx(codEmpresa, codContabilidad, codRazon);

        [HttpGet("CntXRazonDetalle_ValidaB")]
        public ActionResult<ErrorDto<int?>> CntXRazonDetalle_ValidaB([FromQuery] int codEmpresa, [FromQuery] int codContabilidad, [FromQuery] string codRazon, [FromQuery] int? excludeIdx)
            => _bl.CntXRazonDetalle_ValidaB(codEmpresa, codContabilidad, codRazon, excludeIdx);

        [HttpPost("CntXRazonDetalle_Insertar")]
        public ActionResult<ErrorDto<bool>> CntXRazonDetalle_Insertar([FromQuery] int codEmpresa,[FromBody] CntXRazonDetalleDto param)
            => _bl.CntXRazonDetalle_Insertar(codEmpresa, param);

        [HttpPut("CntXRazonDetalle_Actualizar")]
        public ActionResult<ErrorDto<bool>> CntXRazonDetalle_Actualizar([FromQuery] int codEmpresa,[FromBody] CntXRazonDetalleDto param)
            => _bl.CntXRazonDetalle_Actualizar(codEmpresa, param);

        [HttpDelete("CntXRazonDetalle_Eliminar")]
        public ActionResult<ErrorDto<bool>> CntXRazonDetalle_Eliminar([FromQuery] int codEmpresa, [FromQuery] int codContabilidad,[FromQuery] string codRazon, [FromQuery] int idx)
            => _bl.CntXRazonDetalle_Eliminar(codEmpresa, codContabilidad, codRazon, idx);
    }
}
