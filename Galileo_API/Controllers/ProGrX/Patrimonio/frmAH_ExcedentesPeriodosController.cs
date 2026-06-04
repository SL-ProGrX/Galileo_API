using Galileo.Models.AH;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Patrimonio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Patrimonio
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmAhExcedentesPeriodosController : ControllerBase
    {
        private readonly FrmAhExcedentesPeriodosBL _bl;

        public FrmAhExcedentesPeriodosController(IConfiguration config)
        {
            _bl = new FrmAhExcedentesPeriodosBL(config);
        }

        [HttpGet("Ah_ExcedentesPeriodos_Lista")]
        public ActionResult<ErrorDto<List<FrmAhExcedentesPeriodosListaDto>>> Ah_ExcedentesPeriodos_Lista(
            [FromQuery] int codEmpresa)
        {
            return _bl.Ah_ExcedentesPeriodos_Lista(codEmpresa);
        }

        [HttpGet("Ah_ExcedentesPeriodos_Obtener")]
        public ActionResult<ErrorDto<FrmAhExcedentesPeriodosDetalleDto>> Ah_ExcedentesPeriodos_Obtener(
            [FromQuery] int codEmpresa,
            [FromQuery] int periodoId)
        {
            return _bl.Ah_ExcedentesPeriodos_Obtener(codEmpresa, periodoId);
        }

        [HttpGet("Ah_ExcedentesPeriodos_Bitacora_Lista")]
        public ActionResult<ErrorDto<List<BitacoraExcedenteDto>>> Ah_ExcedentesPeriodos_Bitacora_Lista(
            [FromQuery] int codEmpresa,
            [FromQuery] int periodoId,
            [FromQuery] string etapa = "T")
        {
            return _bl.Ah_ExcedentesPeriodos_Bitacora_Lista(codEmpresa, periodoId, etapa);
        }

        [HttpGet("Ah_ExcedentesPeriodos_Resumen_Lista")]
        public ActionResult<ErrorDto<List<FrmAhExcedentesPeriodosResumenDto>>> Ah_ExcedentesPeriodos_Resumen_Lista(
            [FromQuery] int codEmpresa,
            [FromQuery] int periodoId)
        {
            return _bl.Ah_ExcedentesPeriodos_Resumen_Lista(codEmpresa, periodoId);
        }

        [HttpPost("Ah_ExcedentesPeriodos_Insertar")]
        public ActionResult<ErrorDto<FrmAhExcedentesPeriodosGuardarResponse>> Ah_ExcedentesPeriodos_Insertar(
            [FromQuery] int codEmpresa,
            [FromBody] FrmAhExcedentesPeriodosGuardarRequest request)
        {
            return _bl.Ah_ExcedentesPeriodos_Insertar(codEmpresa, request);
        }

        [HttpPut("Ah_ExcedentesPeriodos_Actualizar")]
        public ActionResult<ErrorDto<FrmAhExcedentesPeriodosGuardarResponse>> Ah_ExcedentesPeriodos_Actualizar(
            [FromQuery] int codEmpresa,
            [FromBody] FrmAhExcedentesPeriodosGuardarRequest request)
        {
            return _bl.Ah_ExcedentesPeriodos_Actualizar(codEmpresa, request);
        }

        [HttpDelete("Ah_ExcedentesPeriodos_Eliminar")]
        public ActionResult<ErrorDto<bool>> Ah_ExcedentesPeriodos_Eliminar(
            [FromQuery] int codEmpresa,
            [FromQuery] int periodoId,
            [FromQuery] string usuario)
        {
            return _bl.Ah_ExcedentesPeriodos_Eliminar(codEmpresa, periodoId, usuario);
        }

        [HttpPut("Ah_ExcedentesPeriodos_BaseAplicacion_Actualizar")]
        public ActionResult<ErrorDto<bool>> Ah_ExcedentesPeriodos_BaseAplicacion_Actualizar(
            [FromQuery] int codEmpresa,
            [FromBody] FrmAhExcedentesPeriodosBaseAplicacionRequest request)
        {
            return _bl.Ah_ExcedentesPeriodos_BaseAplicacion_Actualizar(codEmpresa, request);
        }

        [HttpPut("Ah_ExcedentesPeriodos_EstadoNota_Actualizar")]
        public ActionResult<ErrorDto<bool>> Ah_ExcedentesPeriodos_EstadoNota_Actualizar(
            [FromQuery] int codEmpresa,
            [FromBody] FrmAhExcedentesPeriodosEstadoNotaRequest request)
        {
            return _bl.Ah_ExcedentesPeriodos_EstadoNota_Actualizar(codEmpresa, request);
        }

        [HttpPost("Ah_ExcedentesPeriodos_RecalcularBase")]
        public ActionResult<ErrorDto<bool>> Ah_ExcedentesPeriodos_RecalcularBase(
            [FromQuery] int codEmpresa,
            [FromBody] FrmAhExcedentesPeriodosRecalcularBaseRequest request)
        {
            return _bl.Ah_ExcedentesPeriodos_RecalcularBase(codEmpresa, request);
        }
    }
}
