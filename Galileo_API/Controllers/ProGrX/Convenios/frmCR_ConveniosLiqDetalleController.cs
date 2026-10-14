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
    public class FrmCrConveniosLiqDetalleController : ControllerBase
    {
        private readonly FrmCrConveniosLiqDetalleBl _bl;

        public FrmCrConveniosLiqDetalleController(IConfiguration config)
        {
            _bl = new FrmCrConveniosLiqDetalleBl(config);
        }

        [HttpGet("CrConveniosLiqDetalle_Cabecera_Obtener")]
        public ErrorDto<CrLiqDetalleCabecera?> CrConveniosLiqDetalle_Cabecera_Obtener(
            [FromQuery] int codEmpresa,
            [FromQuery] string codConvenio,
            [FromQuery] int codOrden)
            => _bl.CrConveniosLiqDetalle_Cabecera_Obtener(codEmpresa, codConvenio, codOrden);

        [HttpGet("CrConveniosLiqDetalle_Instituciones_Obtener")]
        public ErrorDto<List<CrLiqDetalleInstitucion>> CrConveniosLiqDetalle_Instituciones_Obtener(
            [FromQuery] int codEmpresa,
            [FromQuery] string codConvenio,
            [FromQuery] int codOrden,
            [FromQuery] string tipo)
            => _bl.CrConveniosLiqDetalle_Instituciones_Obtener(codEmpresa, codConvenio, codOrden, tipo);

        [HttpGet("CrConveniosLiqDetalle_Consulta_Obtener")]
        public ErrorDto<CrLiqDetalleConsulta?> CrConveniosLiqDetalle_Consulta_Obtener(
            [FromQuery] int codEmpresa,
            [FromQuery] string codConvenio,
            [FromQuery] int codOrden,
            [FromQuery] string tipo,
            [FromQuery] string informe,
            [FromQuery] int? codInstitucion)
            => _bl.CrConveniosLiqDetalle_Consulta_Obtener(
                codEmpresa, codConvenio, codOrden, tipo, informe, codInstitucion);

        /// <summary>
        /// Devuelve el CSV como archivo; ante un error responde el ErrorDto en JSON.
        /// </summary>
        [HttpGet("CrConveniosLiqDetalle_Archivo_Generar")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDto<CrLiqDetalleArchivo?>), StatusCodes.Status200OK)]
        public IActionResult CrConveniosLiqDetalle_Archivo_Generar(
            [FromQuery] int codEmpresa,
            [FromQuery] string codConvenio,
            [FromQuery] int codOrden,
            [FromQuery] string tipo,
            [FromQuery] int? codInstitucion,
            [FromQuery] string? nombreInstitucion)
        {
            var resultado = _bl.CrConveniosLiqDetalle_Archivo_Generar(
                codEmpresa, codConvenio, codOrden, tipo, codInstitucion, nombreInstitucion);

            if (resultado.Code != 0 || resultado.Result == null)
                return Ok(new ErrorDto<CrLiqDetalleArchivo?>
                {
                    Code = resultado.Code,
                    Description = resultado.Description,
                    Result = null
                });

            return File(resultado.Result.contenido, "text/csv", resultado.Result.nombre);
        }
    }
}
