using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_EstudioCrd
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmPreaEstudiov2Controller : ControllerBase
    {
        private readonly FrmPreaEstudiov2BL _bl;

        public FrmPreaEstudiov2Controller(IConfiguration config)
        {
            _bl = new FrmPreaEstudiov2BL(config);
        }

        /// <summary>
        /// Carga la información completa de un expediente de estudio de crédito.
        /// </summary>
        [HttpPost("Prea_frmPreaEstudiov2_Scroll")]
        public ErrorDto<FrmPreaEstudiov2ScrollResponse> Prea_frmPreaEstudiov2_Scroll(
            int codEmpresa,
            [FromBody] FrmPreaEstudiov2ScrollRequest request)
        {
            return _bl.Prea_frmPreaEstudiov2_Scroll(codEmpresa, request);
        }

        [HttpPost("Prea_frmPreaEstudiov2_Cargar")]
        public ErrorDto<FrmPreaEstudiov2CargaResponse> Prea_frmPreaEstudiov2_Cargar(
            int codEmpresa,
            [FromBody] FrmPreaEstudiov2CargaRequest request)
        {
            return _bl.Prea_frmPreaEstudiov2_Cargar(codEmpresa, request);
        }

        /// <summary>
        /// Obtiene la información del tab Hipotecario.
        /// </summary>
        [HttpPost("Prea_frmPreaEstudiov2_Hipotecario_Obtener")]
        public ErrorDto<FrmPreaEstudiov2HipotecarioResponse> Prea_frmPreaEstudiov2_Hipotecario_Obtener(
            int codEmpresa,
            FrmPreaEstudiov2HipotecarioRequest request)
        {
            return _bl.Prea_frmPreaEstudiov2_Hipotecario_Obtener(codEmpresa, request);
        }

        /// <summary>
        /// Cambia el expediente a estado abandonado.
        /// </summary>
        [HttpPost("Prea_frmPreaEstudiov2_Abandonar")]
        public ErrorDto<FrmPreaEstudiov2AbandonarResponse> Prea_frmPreaEstudiov2_Abandonar(
            int codEmpresa,
            [FromBody] FrmPreaEstudiov2AbandonarRequest request)
        {
            return _bl.Prea_frmPreaEstudiov2_Abandonar(codEmpresa, request);
        }

        /// <summary>
        /// Guarda el preanálisis (nuevo o modificado).
        /// </summary>
        [HttpPost("Prea_frmPreaEstudiov2_Guardar")]
        public ErrorDto<FrmPreaEstudiov2GuardarResponse> Prea_frmPreaEstudiov2_Guardar(
            int codEmpresa,
            [FromBody] FrmPreaEstudiov2GuardarRequest request)
        {
            return _bl.Prea_frmPreaEstudiov2_Guardar(codEmpresa, request);
        }

        /// <summary>
        /// Consulta las deducciones del expediente.
        /// </summary>
        [HttpGet("Prea_frmPreaEstudiov2_Deducciones_Consultar")]
        public ErrorDto<FrmPreaEstudiov2DeduccionesResponse> Prea_frmPreaEstudiov2_Deducciones_Consultar(
            int codEmpresa,
            [FromQuery] string cod_preanalisis)
        {
            return _bl.Prea_frmPreaEstudiov2_Deducciones_Consultar(codEmpresa, cod_preanalisis);
        }

        /// <summary>
        /// Agrega una deducción al expediente.
        /// </summary>
        [HttpPost("Prea_frmPreaEstudiov2_Deducciones_Agregar")]
        public ErrorDto<FrmPreaEstudiov2DeduccionesResponse> Prea_frmPreaEstudiov2_Deducciones_Agregar(
            int codEmpresa,
            [FromBody] FrmPreaEstudiov2DeduccionesAgregarRequest request)
        {
            return _bl.Prea_frmPreaEstudiov2_Deducciones_Agregar(codEmpresa, request);
        }

        /// <summary>
        /// Consulta los créditos en tránsito del expediente.
        /// </summary>
        [HttpGet("Prea_frmPreaEstudiov2_Creditos_Consultar")]
        public ErrorDto<FrmPreaEstudiov2CreditosResponse> Prea_frmPreaEstudiov2_Creditos_Consultar(
            int codEmpresa,
            [FromQuery] string cod_preanalisis)
        {
            return _bl.Prea_frmPreaEstudiov2_Creditos_Consultar(codEmpresa, cod_preanalisis);
        }

        /// <summary>
        /// Consulta las refundiciones del expediente.
        /// </summary>
        [HttpGet("Prea_frmPreaEstudiov2_Refundiciones_Consultar")]
        public ErrorDto<FrmPreaEstudiov2RefundicionesResponse> Prea_frmPreaEstudiov2_Refundiciones_Consultar(
            int codEmpresa,
            [FromQuery] string cod_preanalisis)
        {
            return _bl.Prea_frmPreaEstudiov2_Refundiciones_Consultar(codEmpresa, cod_preanalisis);
        }

        /// <summary>
        /// Consulta las fianzas del expediente.
        /// </summary>
        [HttpGet("Prea_frmPreaEstudiov2_Fianzas_Consultar")]
        public ErrorDto<FrmPreaEstudiov2FianzasResponse> Prea_frmPreaEstudiov2_Fianzas_Consultar(
            int codEmpresa,
            [FromQuery] string cod_preanalisis)
        {
            return _bl.Prea_frmPreaEstudiov2_Fianzas_Consultar(codEmpresa, cod_preanalisis);
        }

        /// <summary>
        /// Consulta los desembolsos del expediente.
        /// </summary>
        [HttpGet("Prea_frmPreaEstudiov2_Desembolsos_Consultar")]
        public ErrorDto<FrmPreaEstudiov2DesembolsosResponse> Prea_frmPreaEstudiov2_Desembolsos_Consultar(
            int codEmpresa,
            [FromQuery] string cod_preanalisis)
        {
            return _bl.Prea_frmPreaEstudiov2_Desembolsos_Consultar(codEmpresa, cod_preanalisis);
        }

        /// <summary>
        /// Guarda un desembolso del expediente.
        /// </summary>
        [HttpPost("Prea_frmPreaEstudiov2_Desembolsos_Guardar")]
        public ErrorDto<FrmPreaEstudiov2DesembolsosResponse> Prea_frmPreaEstudiov2_Desembolsos_Guardar(
            int codEmpresa,
            [FromBody] FrmPreaEstudiov2DesembolsoGuardarRequest request)
        {
            return _bl.Prea_frmPreaEstudiov2_Desembolsos_Guardar(codEmpresa, request);
        }

        /// <summary>
        /// Elimina un desembolso del expediente.
        /// </summary>
        [HttpPost("Prea_frmPreaEstudiov2_Desembolsos_Eliminar")]
        public ErrorDto<FrmPreaEstudiov2DesembolsosResponse> Prea_frmPreaEstudiov2_Desembolsos_Eliminar(
            int codEmpresa,
            [FromQuery] string cod_preanalisis,
            [FromQuery] int id_desembolso)
        {
            return _bl.Prea_frmPreaEstudiov2_Desembolsos_Eliminar(codEmpresa, cod_preanalisis, id_desembolso);
        }

        /// <summary>
        /// Consulta el historial del expediente.
        /// </summary>
        [HttpGet("Prea_frmPreaEstudiov2_Historial_Consultar")]
        public ErrorDto<FrmPreaEstudiov2HistorialResponse> Prea_frmPreaEstudiov2_Historial_Consultar(
            int codEmpresa,
            [FromQuery] string cod_preanalisis)
        {
            return _bl.Prea_frmPreaEstudiov2_Historial_Consultar(codEmpresa, cod_preanalisis);
        }

        /// <summary>
        /// Consulta los adjuntos del expediente.
        /// </summary>
        [HttpGet("Prea_frmPreaEstudiov2_Adjuntos_Consultar")]
        public ErrorDto<List<FrmPreaEstudiov2AdjuntoDto>> Prea_frmPreaEstudiov2_Adjuntos_Consultar(
            int codEmpresa,
            [FromQuery] string cod_preanalisis)
        {
            return _bl.Prea_frmPreaEstudiov2_Adjuntos_Consultar(codEmpresa, cod_preanalisis);
        }

        /// <summary>
        /// Consulta la resolución del expediente.
        /// </summary>
        [HttpGet("Prea_frmPreaEstudiov2_Resolucion_Consultar")]
        public ErrorDto<FrmPreaEstudiov2ResolucionResponse> Prea_frmPreaEstudiov2_Resolucion_Consultar(
            int codEmpresa,
            [FromQuery] string cod_preanalisis)
        {
            return _bl.Prea_frmPreaEstudiov2_Resolucion_Consultar(codEmpresa, cod_preanalisis);
        }

        /// <summary>
        /// Guarda observaciones del expediente.
        /// </summary>
        [HttpPost("Prea_frmPreaEstudiov2_Observaciones_Guardar")]
        public ErrorDto<string> Prea_frmPreaEstudiov2_Observaciones_Guardar(
            int codEmpresa,
            [FromBody] FrmPreaEstudiov2ObservacionesRequest request)
        {
            return _bl.Prea_frmPreaEstudiov2_Observaciones_Guardar(codEmpresa, request);
        }

        /// <summary>
        /// Asigna comité resolutivo al expediente.
        /// </summary>
        [HttpPost("Prea_frmPreaEstudiov2_Comite_Asignar")]
        public ErrorDto<FrmPreaEstudiov2ComiteAsignarResponse> Prea_frmPreaEstudiov2_Comite_Asignar(
            int codEmpresa,
            [FromBody] FrmPreaEstudiov2ComiteAsignarRequest request)
        {
            return _bl.Prea_frmPreaEstudiov2_Comite_Asignar(codEmpresa, request);
        }

        /// <summary>
        /// Copia un expediente existente.
        /// </summary>
        [HttpPost("Prea_frmPreaEstudiov2_Copiar")]
        public ErrorDto<FrmPreaEstudiov2CopiarResponse> Prea_frmPreaEstudiov2_Copiar(
            int codEmpresa,
            [FromBody] FrmPreaEstudiov2CopiarRequest request)
        {
            return _bl.Prea_frmPreaEstudiov2_Copiar(codEmpresa, request);
        }

        /// <summary>
        /// Solicita el estado del expediente.
        /// </summary>
        [HttpPost("Prea_frmPreaEstudiov2_Solicitar")]
        public ErrorDto<FrmPreaEstudiov2SolicitarResponse> Prea_frmPreaEstudiov2_Solicitar(
            int codEmpresa,
            [FromBody] FrmPreaEstudiov2SolicitarRequest request)
        {
            return _bl.Prea_frmPreaEstudiov2_Solicitar(codEmpresa, request);
        }

        /// <summary>
        /// Guarda una incapacidad del expediente.
        /// </summary>
        [HttpPost("Prea_frmPreaEstudiov2_Incapacidades_Guardar")]
        public ErrorDto<string> Prea_frmPreaEstudiov2_Incapacidades_Guardar(
            int codEmpresa,
            [FromBody] FrmPreaEstudiov2IncapacidadGuardarRequest request)
        {
            return _bl.Prea_frmPreaEstudiov2_Incapacidades_Guardar(codEmpresa, request);
        }

        /// <summary>
        /// Elimina incapacidades del expediente.
        /// </summary>
        [HttpPost("Prea_frmPreaEstudiov2_Incapacidades_Eliminar")]
        public ErrorDto<string> Prea_frmPreaEstudiov2_Incapacidades_Eliminar(
            int codEmpresa,
            [FromQuery] string cod_preanalisis)
        {
            return _bl.Prea_frmPreaEstudiov2_Incapacidades_Eliminar(codEmpresa, cod_preanalisis);
        }

        /// <summary>
        /// Guarda un extra del expediente.
        /// </summary>
        [HttpPost("Prea_frmPreaEstudiov2_Extras_Guardar")]
        public ErrorDto<string> Prea_frmPreaEstudiov2_Extras_Guardar(
            int codEmpresa,
            [FromBody] FrmPreaEstudiov2ExtraGuardarRequest request)
        {
            return _bl.Prea_frmPreaEstudiov2_Extras_Guardar(codEmpresa, request);
        }

        /// <summary>
        /// Consulta las causas de seguimiento del expediente.
        /// </summary>
        [HttpGet("Prea_frmPreaEstudiov2_Causas_Consultar")]
        public ErrorDto<List<FrmPreaEstudiov2CausaDto>> Prea_frmPreaEstudiov2_Causas_Consultar(
            int codEmpresa,
            [FromQuery] string cod_preanalisis,
            [FromQuery] string tipo)
        {
            return _bl.Prea_frmPreaEstudiov2_Causas_Consultar(codEmpresa, cod_preanalisis, tipo);
        }

        /// <summary>
        /// Guarda observaciones de una causa del expediente.
        /// </summary>
        [HttpPost("Prea_frmPreaEstudiov2_Causas_Guardar")]
        public ErrorDto<string> Prea_frmPreaEstudiov2_Causas_Guardar(
            int codEmpresa,
            [FromBody] FrmPreaEstudiov2CausasGuardarRequest request)
        {
            return _bl.Prea_frmPreaEstudiov2_Causas_Guardar(codEmpresa, request);
        }
    }
}
