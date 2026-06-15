using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCrEnCobroCuotasController : ControllerBase
    {
        private readonly FrmCrEnCobroCuotasBL BL;

        public FrmCrEnCobroCuotasController(IConfiguration config)
        {
            BL = new FrmCrEnCobroCuotasBL(config);
        }

        [Authorize]
        [HttpGet("CR_EnCobroCuotas_Inicial_Obtener")]
        public ErrorDto<CrEnCobroCuotasInicialDto> CR_EnCobroCuotas_Inicial_Obtener(int CodEmpresa, string cedula)
        {
            return BL.CR_EnCobroCuotas_Inicial_Obtener(CodEmpresa, cedula);
        }

        [Authorize]
        [HttpGet("CR_EnCobroCuotas_Deductoras_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_EnCobroCuotas_Deductoras_Dropdown_Obtener(int CodEmpresa, int codInstitucion)
        {
            return BL.CR_EnCobroCuotas_Deductoras_Dropdown_Obtener(CodEmpresa, codInstitucion);
        }

        [Authorize]
        [HttpGet("CR_EnCobroCuotas_Deductora_Info_Obtener")]
        public ErrorDto<CrEnCobroCuotasInicialDto> CR_EnCobroCuotas_Deductora_Info_Obtener(int CodEmpresa, int codInstitucion)
        {
            return BL.CR_EnCobroCuotas_Deductora_Info_Obtener(CodEmpresa, codInstitucion);
        }

        [Authorize]
        [HttpGet("CR_EnCobroCuotas_Proceso_Scroll_Obtener")]
        public ErrorDto<CrEnCobroCuotasProcesoScrollDto> CR_EnCobroCuotas_Proceso_Scroll_Obtener(int CodEmpresa,int scrollCode,decimal procesoActual)
        {
            return BL.CR_EnCobroCuotas_Proceso_Scroll_Obtener(CodEmpresa, scrollCode, procesoActual);
        }

        [Authorize]
        [HttpPost("CR_EnCobroCuotas_Resumen_Lista_Obtener")]
        public ErrorDto<CrEnCobroCuotasListaResult<CrEnCobroCuotasResumenData>> CR_EnCobroCuotas_Resumen_Lista_Obtener(int CodEmpresa,[FromBody] CrEnCobroCuotasConsultaRequest request)
        {
            return BL.CR_EnCobroCuotas_Resumen_Lista_Obtener(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("CR_EnCobroCuotas_Resumen_Lista_Export")]
        public ErrorDto<CrEnCobroCuotasListaResult<CrEnCobroCuotasResumenData>> CR_EnCobroCuotas_Resumen_Lista_Export(int CodEmpresa,[FromBody] CrEnCobroCuotasConsultaRequest request)
        {
            return BL.CR_EnCobroCuotas_Resumen_Lista_Export(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("CR_EnCobroCuotas_Detalle_Lista_Obtener")]
        public ErrorDto<CrEnCobroCuotasListaResult<CrEnCobroCuotasDetalleData>> CR_EnCobroCuotas_Detalle_Lista_Obtener(int CodEmpresa,[FromBody] CrEnCobroCuotasConsultaRequest request)
        {
            return BL.CR_EnCobroCuotas_Detalle_Lista_Obtener(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("CR_EnCobroCuotas_Detalle_Lista_Export")]
        public ErrorDto<CrEnCobroCuotasListaResult<CrEnCobroCuotasDetalleData>> CR_EnCobroCuotas_Detalle_Lista_Export(int CodEmpresa,[FromBody] CrEnCobroCuotasConsultaRequest request)
        {
            return BL.CR_EnCobroCuotas_Detalle_Lista_Export(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("CR_EnCobroCuotas_Envio_Lista_Obtener")]
        public ErrorDto<CrEnCobroCuotasListaResult<CrEnCobroCuotasEnvioData>> CR_EnCobroCuotas_Envio_Lista_Obtener(int CodEmpresa,[FromBody] CrEnCobroCuotasConsultaRequest request)
        {
            return BL.CR_EnCobroCuotas_Envio_Lista_Obtener(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("CR_EnCobroCuotas_Envio_Lista_Export")]
        public ErrorDto<CrEnCobroCuotasListaResult<CrEnCobroCuotasEnvioData>> CR_EnCobroCuotas_Envio_Lista_Export(int CodEmpresa,[FromBody] CrEnCobroCuotasConsultaRequest request)
        {
            return BL.CR_EnCobroCuotas_Envio_Lista_Export(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("CR_EnCobroCuotas_Recepcion_Lista_Obtener")]
        public ErrorDto<CrEnCobroCuotasRecepcionResult> CR_EnCobroCuotas_Recepcion_Lista_Obtener(int CodEmpresa,[FromBody] CrEnCobroCuotasConsultaRequest request)
        {
            return BL.CR_EnCobroCuotas_Recepcion_Lista_Obtener(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("CR_EnCobroCuotas_Recepcion_Lista_Export")]
        public ErrorDto<CrEnCobroCuotasRecepcionResult> CR_EnCobroCuotas_Recepcion_Lista_Export(int CodEmpresa,[FromBody] CrEnCobroCuotasConsultaRequest request)
        {
            return BL.CR_EnCobroCuotas_Recepcion_Lista_Export(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("CR_EnCobroCuotas_Historial_Lista_Obtener")]
        public ErrorDto<CrEnCobroCuotasListaResult<CrEnCobroCuotasHistorialData>> CR_EnCobroCuotas_Historial_Lista_Obtener(int CodEmpresa,[FromBody] CrEnCobroCuotasConsultaRequest request)
        {
            return BL.CR_EnCobroCuotas_Historial_Lista_Obtener(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("CR_EnCobroCuotas_Historial_Lista_Export")]
        public ErrorDto<CrEnCobroCuotasListaResult<CrEnCobroCuotasHistorialData>> CR_EnCobroCuotas_Historial_Lista_Export(int CodEmpresa,[FromBody] CrEnCobroCuotasConsultaRequest request)
        {
            return BL.CR_EnCobroCuotas_Historial_Lista_Export(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("CR_EnCobroCuotas_ResumenDeductoras_Lista_Obtener")]
        public ErrorDto<CrEnCobroCuotasListaResult<CrEnCobroCuotasResumenDeductoraData>> CR_EnCobroCuotas_ResumenDeductoras_Lista_Obtener(int CodEmpresa,[FromBody] CrEnCobroCuotasConsultaRequest request)
        {
            return BL.CR_EnCobroCuotas_ResumenDeductoras_Lista_Obtener(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("CR_EnCobroCuotas_ResumenDeductoras_Lista_Export")]
        public ErrorDto<CrEnCobroCuotasListaResult<CrEnCobroCuotasResumenDeductoraData>> CR_EnCobroCuotas_ResumenDeductoras_Lista_Export(int CodEmpresa,[FromBody] CrEnCobroCuotasConsultaRequest request)
        {
            return BL.CR_EnCobroCuotas_ResumenDeductoras_Lista_Export(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("CR_EnCobroCuotas_Bitacora_Lista_Obtener")]
        public ErrorDto<CrEnCobroCuotasListaResult<CrEnCobroCuotasBitacoraData>> CR_EnCobroCuotas_Bitacora_Lista_Obtener(int CodEmpresa,
            [FromBody] CrEnCobroCuotasConsultaRequest request)
        {
            return BL.CR_EnCobroCuotas_Bitacora_Lista_Obtener(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("CR_EnCobroCuotas_Bitacora_Lista_Export")]
        public ErrorDto<CrEnCobroCuotasListaResult<CrEnCobroCuotasBitacoraData>> CR_EnCobroCuotas_Bitacora_Lista_Export(int CodEmpresa,[FromBody] CrEnCobroCuotasConsultaRequest request)
        {
            return BL.CR_EnCobroCuotas_Bitacora_Lista_Export(CodEmpresa, request);
        }
    }
}