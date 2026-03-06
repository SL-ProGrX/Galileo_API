using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Galileo.Models.ERROR;
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
    }
}
