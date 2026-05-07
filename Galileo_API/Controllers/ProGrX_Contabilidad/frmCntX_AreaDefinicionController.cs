using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [ApiController]
    [Route("api/[controller]")]
    public class FrmCntXAreaDefinicionController : ControllerBase
    {
        private readonly FrmCntXAreaDefinicionBL _bl;

        public FrmCntXAreaDefinicionController(IConfiguration config)
        {
            _bl = new FrmCntXAreaDefinicionBL(config);
        }

        [HttpGet("AreaDefinicion_Lista")]
        public ActionResult<ErrorDto<List<AreaDefinicionDto>>> AreaDefinicion_Lista(
            [FromQuery] int codEmpresa,
            [FromQuery] int codigoConta,
            [FromQuery] string order)
            => _bl.AreaDefinicion_Lista(codEmpresa, codigoConta, order);

        [HttpGet("TiposCuentas_Lista")]
        public ActionResult<ErrorDto<List<TipoCuentaDto>>> TiposCuentas_Lista(
            [FromQuery] int codEmpresa,
            [FromQuery] int codigoConta)
            => _bl.TiposCuentas_Lista(codEmpresa, codigoConta);

        [HttpGet("Cuentas_ListaNodo")]
        public ActionResult<ErrorDto<List<CuentaNodoDto>>> Cuentas_ListaNodo(
            [FromQuery] int codEmpresa,
            [FromQuery] int codigoConta,
            [FromQuery] string tipoCuenta,
            [FromQuery] string cuentaActual,
            [FromQuery] string nodo)
            => _bl.Cuentas_ListaNodo(codEmpresa, codigoConta, tipoCuenta, cuentaActual, nodo);

        [HttpGet("AreaCuenta_Existe")]
        public ActionResult<ErrorDto<ExisteDto>> AreaCuenta_Existe(
            [FromQuery] int codEmpresa,
            [FromQuery] int codigoConta,
            [FromQuery] string cuentaNodo,
            [FromQuery] int areaActual)
            => _bl.AreaCuenta_Existe(codEmpresa, codigoConta, cuentaNodo, areaActual);
    }
}
