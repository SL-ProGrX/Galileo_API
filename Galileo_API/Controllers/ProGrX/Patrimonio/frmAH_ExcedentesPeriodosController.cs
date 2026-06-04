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

        [HttpGet("Patrimonio_frmAH_ExcedentesPeriodos_Lista")]
        public ActionResult<ErrorDto<List<FrmAhExcedentesPeriodosListaDto>>> Patrimonio_frmAH_ExcedentesPeriodos_Lista(
            [FromQuery] int codEmpresa)
        {
            return _bl.Patrimonio_frmAH_ExcedentesPeriodos_Lista(codEmpresa);
        }

        [HttpGet("Patrimonio_frmAH_ExcedentesPeriodos_Obtener")]
        public ActionResult<ErrorDto<FrmAhExcedentesPeriodosDetalleDto>> Patrimonio_frmAH_ExcedentesPeriodos_Obtener(
            [FromQuery] int codEmpresa,
            [FromQuery] int periodoId)
        {
            return _bl.Patrimonio_frmAH_ExcedentesPeriodos_Obtener(codEmpresa, periodoId);
        }

        [HttpGet("Patrimonio_frmAH_ExcedentesPeriodos_Bitacora_Lista")]
        public ActionResult<ErrorDto<List<BitacoraExcedenteDto>>> Patrimonio_frmAH_ExcedentesPeriodos_Bitacora_Lista(
            [FromQuery] int codEmpresa,
            [FromQuery] int periodoId,
            [FromQuery] string etapa = "T")
        {
            return _bl.Patrimonio_frmAH_ExcedentesPeriodos_Bitacora_Lista(codEmpresa, periodoId, etapa);
        }

        [HttpGet("Patrimonio_frmAH_ExcedentesPeriodos_Resumen_Lista")]
        public ActionResult<ErrorDto<List<FrmAhExcedentesPeriodosResumenDto>>> Patrimonio_frmAH_ExcedentesPeriodos_Resumen_Lista(
            [FromQuery] int codEmpresa,
            [FromQuery] int periodoId)
        {
            return _bl.Patrimonio_frmAH_ExcedentesPeriodos_Resumen_Lista(codEmpresa, periodoId);
        }

        [HttpPost("Patrimonio_frmAH_ExcedentesPeriodos_Insertar")]
        public ActionResult<ErrorDto<FrmAhExcedentesPeriodosGuardarResponse>> Patrimonio_frmAH_ExcedentesPeriodos_Insertar(
            [FromQuery] int codEmpresa,
            [FromBody] FrmAhExcedentesPeriodosGuardarRequest request)
        {
            return _bl.Patrimonio_frmAH_ExcedentesPeriodos_Insertar(codEmpresa, request);
        }

        [HttpPut("Patrimonio_frmAH_ExcedentesPeriodos_Actualizar")]
        public ActionResult<ErrorDto<FrmAhExcedentesPeriodosGuardarResponse>> Patrimonio_frmAH_ExcedentesPeriodos_Actualizar(
            [FromQuery] int codEmpresa,
            [FromBody] FrmAhExcedentesPeriodosGuardarRequest request)
        {
            return _bl.Patrimonio_frmAH_ExcedentesPeriodos_Actualizar(codEmpresa, request);
        }

        [HttpDelete("Patrimonio_frmAH_ExcedentesPeriodos_Eliminar")]
        public ActionResult<ErrorDto<bool>> Patrimonio_frmAH_ExcedentesPeriodos_Eliminar(
            [FromQuery] int codEmpresa,
            [FromQuery] int periodoId,
            [FromQuery] string usuario)
        {
            return _bl.Patrimonio_frmAH_ExcedentesPeriodos_Eliminar(codEmpresa, periodoId, usuario);
        }

        [HttpPut("Patrimonio_frmAH_ExcedentesPeriodos_BaseAplicacion_Actualizar")]
        public ActionResult<ErrorDto<bool>> Patrimonio_frmAH_ExcedentesPeriodos_BaseAplicacion_Actualizar(
            [FromQuery] int codEmpresa,
            [FromBody] FrmAhExcedentesPeriodosBaseAplicacionRequest request)
        {
            return _bl.Patrimonio_frmAH_ExcedentesPeriodos_BaseAplicacion_Actualizar(codEmpresa, request);
        }

        [HttpPut("Patrimonio_frmAH_ExcedentesPeriodos_EstadoNota_Actualizar")]
        public ActionResult<ErrorDto<bool>> Patrimonio_frmAH_ExcedentesPeriodos_EstadoNota_Actualizar(
            [FromQuery] int codEmpresa,
            [FromBody] FrmAhExcedentesPeriodosEstadoNotaRequest request)
        {
            return _bl.Patrimonio_frmAH_ExcedentesPeriodos_EstadoNota_Actualizar(codEmpresa, request);
        }

        [HttpPost("Patrimonio_frmAH_ExcedentesPeriodos_RecalcularBase")]
        public ActionResult<ErrorDto<bool>> Patrimonio_frmAH_ExcedentesPeriodos_RecalcularBase(
            [FromQuery] int codEmpresa,
            [FromBody] FrmAhExcedentesPeriodosRecalcularBaseRequest request)
        {
            return _bl.Patrimonio_frmAH_ExcedentesPeriodos_RecalcularBase(codEmpresa, request);
        }
    }
}
