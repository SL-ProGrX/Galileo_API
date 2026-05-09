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
        public ActionResult<ErrorDto<ExisteDto?>> AreaCuenta_Existe(
            [FromQuery] int codEmpresa,
            [FromQuery] int codigoConta,
            [FromQuery] string cuentaNodo,
            [FromQuery] int areaActual)
            => _bl.AreaCuenta_Existe(codEmpresa, codigoConta, cuentaNodo, areaActual);

        [HttpDelete("Area_Eliminar")]
        public ActionResult<ErrorDto<bool>> Area_Eliminar(
            [FromQuery] int codEmpresa,
            [FromQuery] int codigoConta,
            [FromQuery] int areaActual)
            => _bl.Area_Eliminar(codEmpresa, codigoConta, areaActual);

        [HttpPost("AreaCuenta_Insertar")]
        public ActionResult<ErrorDto<bool>> AreaCuenta_Insertar(
            [FromQuery] int codEmpresa,
            [FromQuery] int codigoConta,
            [FromQuery] int areaActual,
            [FromQuery] string cuentaMarcada)
            => _bl.AreaCuenta_Insertar(codEmpresa, codigoConta, areaActual, cuentaMarcada);

        [HttpGet("AreaCuenta_DetalleLista")]
        public ActionResult<ErrorDto<List<AreaCuentaDetalleDto>>> AreaCuenta_DetalleLista(
            [FromQuery] int codEmpresa,
            [FromQuery] int codigoConta,
            [FromQuery] int areaActual)
            => _bl.AreaCuenta_DetalleLista(codEmpresa, codigoConta, areaActual);

        [HttpGet("AreaCuenta_ExistePorCuenta")]
        public ActionResult<ErrorDto<ExisteDto?>> AreaCuenta_ExistePorCuenta(
            [FromQuery] int codEmpresa,
            [FromQuery] int codigoConta,
            [FromQuery] string codCuenta,
            [FromQuery] int areaActual)
            => _bl.AreaCuenta_ExistePorCuenta(codEmpresa, codigoConta, codCuenta, areaActual);

        [HttpPost("AreaCuenta_InsertarMadre")]
        public ActionResult<ErrorDto<bool>> AreaCuenta_InsertarMadre(
            [FromQuery] int codEmpresa,
            [FromQuery] int codigoConta,
            [FromQuery] int areaActual,
            [FromQuery] string cuentaMadre)
            => _bl.AreaCuenta_InsertarMadre(codEmpresa, codigoConta, areaActual, cuentaMadre);

        [HttpPost("AreaDefinicion_Insertar")]
        public ActionResult<ErrorDto<int>> AreaDefinicion_Insertar(
            [FromQuery] int codEmpresa,
            [FromQuery] int codigoConta,
            [FromQuery] string nombreArea,
            [FromQuery] bool chkActiva,
            [FromQuery] string usuario)
            => _bl.AreaDefinicion_Insertar(codEmpresa, codigoConta, nombreArea, chkActiva, usuario);

        [HttpGet("Unidades_Lista")]
        public ActionResult<ErrorDto<List<UnidadDto>>> Unidades_Lista(
            [FromQuery] int codEmpresa,
            [FromQuery] int codigoConta)
            => _bl.Unidades_Lista(codEmpresa, codigoConta);

        [HttpGet("CentroCostos_ListaPorUnidad")]
        public ActionResult<ErrorDto<List<CentroCostoDto>>> CentroCostos_ListaPorUnidad(
            [FromQuery] int codEmpresa,
            [FromQuery] int codigoConta,
            [FromQuery] string unidadActual)
            => _bl.CentroCostos_ListaPorUnidad(codEmpresa, codigoConta, unidadActual);

        [HttpDelete("AreaDefinicion_Eliminar")]
        public ActionResult<ErrorDto<bool>> AreaDefinicion_Eliminar(
            [FromQuery] int codEmpresa,
            [FromQuery] int codigoConta,
            [FromQuery] int areaActual)
            => _bl.AreaDefinicion_Eliminar(codEmpresa, codigoConta, areaActual);
    }
}
