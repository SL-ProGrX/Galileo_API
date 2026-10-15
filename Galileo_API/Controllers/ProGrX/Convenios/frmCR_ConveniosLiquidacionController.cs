using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Convenios;
using Galileo_API.Models.ProGrX.Convenios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Convenios
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrConveniosLiquidacionController : ControllerBase
    {
        private readonly FrmCrConveniosLiquidacionBl _bl;

        public FrmCrConveniosLiquidacionController(IConfiguration config)
        {
            _bl = new FrmCrConveniosLiquidacionBl(config);
        }

        [HttpGet("CrConveniosLiquidacion_Convenios_Obtener")]
        public ErrorDto<List<CrConvenioConsultaData>> CrConveniosLiquidacion_Convenios_Obtener(
            [FromQuery] int codEmpresa)
            => _bl.CrConveniosLiquidacion_Convenios_Obtener(codEmpresa);

        [HttpGet("CrConveniosLiquidacion_Convenio_Navegar")]
        public ErrorDto<CrConvenioConsultaData?> CrConveniosLiquidacion_Convenio_Navegar(
            [FromQuery] int codEmpresa,
            [FromQuery] string? codConvenio,
            [FromQuery] bool siguiente)
            => _bl.CrConveniosLiquidacion_Convenio_Navegar(codEmpresa, codConvenio, siguiente);

        [HttpGet("CrConveniosLiquidacion_Convenio_Obtener")]
        public ErrorDto<CrLiqConvenioData?> CrConveniosLiquidacion_Convenio_Obtener(
            [FromQuery] int codEmpresa,
            [FromQuery] string? codConvenio,
            [FromQuery] string? descripcion)
            => _bl.CrConveniosLiquidacion_Convenio_Obtener(codEmpresa, codConvenio, descripcion);

        [HttpGet("CrConveniosLiquidacion_Ordenes_Obtener")]
        public ErrorDto<List<CrLiqOrdenListaData>> CrConveniosLiquidacion_Ordenes_Obtener(
            [FromQuery] int codEmpresa,
            [FromQuery] string codConvenio,
            [FromQuery] int lineas = 50)
            => _bl.CrConveniosLiquidacion_Ordenes_Obtener(codEmpresa, codConvenio, lineas);

        [HttpGet("CrConveniosLiquidacion_Orden_Obtener")]
        public ErrorDto<CrLiqOrdenData?> CrConveniosLiquidacion_Orden_Obtener(
            [FromQuery] int codEmpresa,
            [FromQuery] string codConvenio,
            [FromQuery] int codOrden)
            => _bl.CrConveniosLiquidacion_Orden_Obtener(codEmpresa, codConvenio, codOrden);

        [HttpGet("CrConveniosLiquidacion_Orden_Nueva")]
        public ErrorDto<CrLiqNuevaOrdenData> CrConveniosLiquidacion_Orden_Nueva(
            [FromQuery] int codEmpresa,
            [FromQuery] string codConvenio)
            => _bl.CrConveniosLiquidacion_Orden_Nueva(codEmpresa, codConvenio);

        [ValidateAntiForgeryToken]
        [HttpPost("CrConveniosLiquidacion_Orden_Guardar")]
        public ErrorDto<CrLiqOrdenData?> CrConveniosLiquidacion_Orden_Guardar(
            [FromQuery] int codEmpresa,
            [FromBody] CrLiqOrdenGuardarRequest request)
            => _bl.CrConveniosLiquidacion_Orden_Guardar(codEmpresa, request);

        [HttpGet("CrConveniosLiquidacion_Orden_Calcular")]
        public ErrorDto<CrLiqOrdenData?> CrConveniosLiquidacion_Orden_Calcular(
            [FromQuery] int codEmpresa,
            [FromQuery] string codConvenio,
            [FromQuery] int codOrden)
            => _bl.CrConveniosLiquidacion_Orden_Calcular(codEmpresa, codConvenio, codOrden);

        [ValidateAntiForgeryToken]
        [HttpPost("CrConveniosLiquidacion_Orden_Cerrar")]
        public ErrorDto<CrLiqOrdenData?> CrConveniosLiquidacion_Orden_Cerrar(
            [FromQuery] int codEmpresa,
            [FromBody] CrLiqOrdenGuardarRequest request)
            => _bl.CrConveniosLiquidacion_Orden_Cerrar(codEmpresa, request);

        [HttpGet("CrConveniosLiquidacion_Flotante_Obtener")]
        public ErrorDto<List<CrLiqFlotanteData>> CrConveniosLiquidacion_Flotante_Obtener(
            [FromQuery] int codEmpresa,
            [FromQuery] string codConvenio)
            => _bl.CrConveniosLiquidacion_Flotante_Obtener(codEmpresa, codConvenio);
    }
}
