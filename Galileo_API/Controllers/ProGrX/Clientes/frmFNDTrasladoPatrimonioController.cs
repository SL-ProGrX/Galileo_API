using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Galileo.BusinessLogic.ProGrX.Clientes;
using Galileo.Models.ProGrX.Clientes;
using Galileo.Models.ERROR;

namespace Galileo.Controllers.ProGrX.Clientes
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFndTrasladoPatrimonioController : ControllerBase
    {
        private readonly FrmFndTrasladoPatrimonioBl _bl;

        public FrmFndTrasladoPatrimonioController(IConfiguration config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            _bl = new FrmFndTrasladoPatrimonioBl(config);
        }

        [Authorize]
        [HttpGet("Fnd_TrasladoPatrimonio_Planes_Obtener")]
        public ErrorDto<List<FndTrasladoPatrimonioPlan>> Fnd_TrasladoPatrimonio_Planes_Obtener(int CodEmpresa, string IdOperadora)
        {
            return _bl.Fnd_TrasladoPatrimonio_Planes_Obtener(CodEmpresa, IdOperadora);
        }

        [Authorize]
        [HttpGet("Fnd_TrasladoPatrimonio_PlanDetalle_Obtener")]
        public ErrorDto<FndTrasladoPatrimonioDetalle?> Fnd_TrasladoPatrimonio_PlanDetalle_Obtener(
            int CodEmpresa, string IdOperadora, string CodPlan)
        {
            return _bl.Fnd_TrasladoPatrimonio_PlanDetalle_Obtener(CodEmpresa, IdOperadora, CodPlan);
        }

        [Authorize]
        [HttpGet("Fnd_TrasladoPatrimonio_Contratos_Obtener")]
        public ErrorDto<List<FndTrasladoPatrimonioContrato>> Fnd_TrasladoPatrimonio_Contratos_Obtener(
            int CodEmpresa, string IdOperadora, string CodPlan, string Destino, bool Marcado)
        {
            return _bl.Fnd_TrasladoPatrimonio_Contratos_Obtener(CodEmpresa, IdOperadora, CodPlan, Destino, Marcado);
        }

        [Authorize]
        [HttpPost("Fnd_TrasladoPatrimonio_DocumentoConsecutivo_Obtener")]
        public ErrorDto<FndDocumentoConsecutivoResult?> Fnd_TrasladoPatrimonio_DocumentoConsecutivo_Obtener(
            int CodEmpresa, [FromBody] FndDocumentoConsecutivoRequest request)
        {
            return _bl.Fnd_TrasladoPatrimonio_DocumentoConsecutivo_Obtener(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("Fnd_TrasladoPatrimonio_DocumentoConsecutivoAse_Obtener")]
        public ErrorDto<FndDocumentoConsecutivoAseResult?> Fnd_TrasladoPatrimonio_DocumentoConsecutivoAse_Obtener(
           int CodEmpresa, [FromBody] FndDocumentoConsecutivoAseRequest request)
        {
            return _bl.Fnd_TrasladoPatrimonio_DocumentoConsecutivoAse_Obtener(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("Fnd_ContratoDetalle_Insertar")]
        public ErrorDto<SimpleSuccessResult> Fnd_ContratoDetalle_Insertar(
            int CodEmpresa, [FromBody] FndContratoDetalleInsertRequest request)
        {
            return _bl.Fnd_ContratoDetalle_Insertar(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("Fnd_Contrato_UpdateAportesRendimiento")]
        public ErrorDto<SimpleSuccessResult> Fnd_Contrato_UpdateAportesRendimiento(
            int CodEmpresa, [FromBody] FndContratoUpdateRequest request)
        {
            return _bl.Fnd_Contrato_UpdateAportesRendimiento(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("Fnd_Documento_Insertar")]
        public ErrorDto<SimpleSuccessResult> Fnd_Documento_Insertar(
            int CodEmpresa, [FromBody] FndDocumentoInsertRequest request)
        {
            return _bl.Fnd_Documento_Insertar(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("Fnd_Asiento_Insertar")]
        public ErrorDto<SimpleSuccessResult> Fnd_Asiento_Insertar(
            int CodEmpresa, [FromBody] FndAsientoInsertRequest request)
        {
            return _bl.Fnd_Asiento_Insertar(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("SifTransaccion_Insertar")]
        public ErrorDto<SimpleSuccessResult> SifTransaccion_Insertar(
            int CodEmpresa, [FromBody] SifTransaccionInsertRequest request)
        {
            return _bl.SifTransaccion_Insertar(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("SifDocsAsiento_Ejecutar")]
        public ErrorDto<SimpleSuccessResult> SifDocsAsiento_Ejecutar(
            int CodEmpresa, [FromBody] SifDocsAsientoRequest request)
        {
            return _bl.SifDocsAsiento_Ejecutar(CodEmpresa, request);
        }

        [Authorize]
        [HttpGet("Fnd_TrasladoPatrimonio_SocioDetalle_Obtener")]
        public ErrorDto<List<FndTrasladoPatrimonioSocioDetalle>> Fnd_TrasladoPatrimonio_SocioDetalle_Obtener(
            int CodEmpresa, string Tcon, string Ncon)
        {
            return _bl.Fnd_TrasladoPatrimonio_SocioDetalle_Obtener(CodEmpresa, Tcon, Ncon);
        }

        [Authorize]
        [HttpPost("Fnd_AhorroConsolidado_Procesar")]
        public ErrorDto<SimpleSuccessResult> Fnd_AhorroConsolidado_Procesar(
            int CodEmpresa, [FromBody] FndAhorroConsolidadoRequest request)
        {
            return _bl.Fnd_AhorroConsolidado_Procesar(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("SifTransaccionPatrimonio_Insertar")]
        public ErrorDto<SimpleSuccessResult> SifTransaccionPatrimonio_Insertar(
            int CodEmpresa, [FromBody] SifTransaccionPatrimonioInsertRequest request)
        {
            return _bl.SifTransaccionPatrimonio_Insertar(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("Fnd_AhorroDetallado_Resumen_Obtener")]
        public ErrorDto<List<FndAhorroDetalladoResumen>> Fnd_AhorroDetallado_Resumen_Obtener(
            int CodEmpresa, [FromBody] FndAhorroDetalladoResumenRequest request)
        {
            return _bl.Fnd_AhorroDetallado_Resumen_Obtener(CodEmpresa, request);
        }

        [Authorize]
        [HttpGet("ParAfah_Cuentas_Obtener")]
        public ErrorDto<ParAfahCuentasResult?> ParAfah_Cuentas_Obtener(int CodEmpresa)
        {
            return _bl.ParAfah_Cuentas_Obtener(CodEmpresa);
        }
    }
}