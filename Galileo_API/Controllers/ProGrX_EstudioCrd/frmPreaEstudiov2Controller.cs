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

        /// <summary>
        /// Recalcula Cuota/Pólizas/Compromiso al cambiar Monto/Plazo/Tasa/Monto
        /// Construcción o una póliza.
        /// </summary>
        [HttpPost("Prea_frmPreaEstudiov2_Credito_Recalcular")]
        public ErrorDto<FrmPreaEstudiov2CreditoRecalculoResponse> Prea_frmPreaEstudiov2_Credito_Recalcular(
            int codEmpresa,
            [FromBody] FrmPreaEstudiov2CreditoRecalcularRequest request)
        {
            return _bl.Prea_frmPreaEstudiov2_Credito_Recalcular(codEmpresa, request);
        }

        /// <summary>
        /// Borra un expediente (equivalente a sbBorrar en VB6).
        /// </summary>
        [HttpPost("Prea_frmPreaEstudiov2_Borrar")]
        public ErrorDto<FrmPreaEstudiov2GuardarResponse> Prea_frmPreaEstudiov2_Borrar(
            int codEmpresa,
            [FromQuery] string codPreanalisis,
            [FromQuery] string codPreanalisisRef)
        {
            return _bl.Prea_frmPreaEstudiov2_Borrar(codEmpresa, codPreanalisis, codPreanalisisRef);
        }

        /// <summary>
        /// Guarda la grilla "Tabla de Salarios" (sbSalarios_Guardar en VB6).
        /// </summary>
        [HttpPost("Prea_frmPreaEstudiov2_TablaSalarios_Guardar")]
        public ErrorDto<List<FrmPreaEstudiov2SalarioDetalleDto>> Prea_frmPreaEstudiov2_TablaSalarios_Guardar(
            int codEmpresa,
            [FromBody] FrmPreaEstudiov2TablaSalariosGuardarRequest request)
        {
            return _bl.Prea_frmPreaEstudiov2_TablaSalarios_Guardar(codEmpresa, request);
        }

        /// <summary>
        /// Cambia oficina y ejecutivo colocador (btnOficinaCambia_Click en VB6).
        /// </summary>
        [HttpPost("Prea_frmPreaEstudiov2_OficinaEjecutivo_Cambiar")]
        public ErrorDto<string> Prea_frmPreaEstudiov2_OficinaEjecutivo_Cambiar(
            int codEmpresa,
            [FromBody] FrmPreaEstudiov2OficinaEjecutivoCambiarRequest request)
        {
            return _bl.Prea_frmPreaEstudiov2_OficinaEjecutivo_Cambiar(codEmpresa, request);
        }

        [HttpPost("Prea_frmPreaEstudiov2_Cargar")]
        public ErrorDto<FrmPreaEstudiov2CargaResponse> Prea_frmPreaEstudiov2_Cargar(
            int codEmpresa,
            [FromBody] FrmPreaEstudiov2CargaRequest request)
        {
            return _bl.Prea_frmPreaEstudiov2_Cargar(codEmpresa, request);
        }

        /// <summary>
        /// Recarga destino/garantía en cascada cuando cambia la Línea de crédito
        /// seleccionada (equivalente a txtLinea_LostFocus en VB6).
        /// </summary>
        [HttpGet("Prea_frmPreaEstudiov2_DestinosGarantias_Consultar")]
        public ErrorDto<FrmPreaEstudiov2DestinosGarantiasResponse> Prea_frmPreaEstudiov2_DestinosGarantias_Consultar(
            int codEmpresa,
            [FromQuery] string linea)
        {
            return _bl.Prea_frmPreaEstudiov2_DestinosGarantias_Consultar(codEmpresa, linea);
        }

        /// <summary>
        /// Valida si la cédula requiere abrir Verificación de Datos Personales.
        /// </summary>
        [HttpGet("Prea_frmPreaEstudiov2_Persona_ValidarDatos")]
        public ErrorDto<int> Prea_frmPreaEstudiov2_Persona_ValidarDatos(
            int codEmpresa,
            [FromQuery] string cedula)
        {
            return _bl.Prea_frmPreaEstudiov2_Persona_ValidarDatos(codEmpresa, cedula);
        }

        /// <summary>
        /// Calcula el Monto según el FORMULARIO de la Garantía (F01/F06).
        /// </summary>
        [HttpPost("Prea_frmPreaEstudiov2_Garantia_Monto")]
        public ErrorDto<FrmPreaEstudiov2GarantiaMontoResponse> Prea_frmPreaEstudiov2_Garantia_Monto(
            int codEmpresa,
            [FromBody] FrmPreaEstudiov2GarantiaMontoRequest request)
        {
            return _bl.Prea_frmPreaEstudiov2_Garantia_Monto(codEmpresa, request);
        }

        /// <summary>
        /// Calcula Monto/Tasa/Plazo de un Fondo de Ahorros (y su lista de contratos).
        /// </summary>
        [HttpPost("Prea_frmPreaEstudiov2_Fondo_Calcular")]
        public ErrorDto<FrmPreaEstudiov2FondoCalcularResponse> Prea_frmPreaEstudiov2_Fondo_Calcular(
            int codEmpresa,
            [FromBody] FrmPreaEstudiov2FondoCalcularRequest request)
        {
            return _bl.Prea_frmPreaEstudiov2_Fondo_Calcular(codEmpresa, request);
        }

        /// <summary>
        /// Lista de sub-expedientes (fiadores) ligados a un expediente principal
        /// (combo cboSubExpediente en VB6).
        /// </summary>
        [HttpGet("Prea_frmPreaEstudiov2_SubExpedientes_Consultar")]
        public ErrorDto<FrmPreaEstudiov2SubExpedientesResponse> Prea_frmPreaEstudiov2_SubExpedientes_Consultar(
            int codEmpresa,
            [FromQuery] string expedientePadre)
        {
            return _bl.Prea_frmPreaEstudiov2_SubExpedientes_Consultar(codEmpresa, expedientePadre);
        }

        /// <summary>
        /// Genera/valida el número de un nuevo sub-expediente (fiador), equivalente a
        /// seleccionar "Nuevo SubExpediente" en cboSubExpediente en VB6.
        /// </summary>
        [HttpPost("Prea_frmPreaEstudiov2_SubExpediente_Generar")]
        public ErrorDto<FrmPreaEstudiov2SubExpedienteGenerarResponse> Prea_frmPreaEstudiov2_SubExpediente_Generar(
            int codEmpresa,
            [FromQuery] string expediente)
        {
            return _bl.Prea_frmPreaEstudiov2_SubExpediente_Generar(codEmpresa, expediente);
        }

        [HttpPost("Prea_frmPreaEstudiov2_Hipotecario_SumarAvaluoCfia")]
        public ErrorDto<FrmPreaEstudiov2HipotecarioResponse> Prea_frmPreaEstudiov2_Hipotecario_SumarAvaluoCfia(
            int codEmpresa,
            [FromBody] FrmPreaEstudiov2HipotecarioSumarAvaluoRequest request)
        {
            return _bl.Prea_frmPreaEstudiov2_Hipotecario_SumarAvaluoCfia(codEmpresa, request);
        }

        [HttpPost("Prea_frmPreaEstudiov2_Hipotecario_CambiarEstado")]
        public ErrorDto<FrmPreaEstudiov2HipotecarioResponse> Prea_frmPreaEstudiov2_Hipotecario_CambiarEstado(
            int codEmpresa,
            [FromBody] FrmPreaEstudiov2HipotecarioCambiarEstadoRequest request)
        {
            return _bl.Prea_frmPreaEstudiov2_Hipotecario_CambiarEstado(codEmpresa, request);
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
        /// Borra una deducción (sbDeducciones_Borrar en VB6).
        /// </summary>
        [HttpPost("Prea_frmPreaEstudiov2_Deducciones_Borrar")]
        public ErrorDto<FrmPreaEstudiov2DeduccionesResponse> Prea_frmPreaEstudiov2_Deducciones_Borrar(
            int codEmpresa,
            [FromBody] FrmPreaEstudiov2DeduccionesBorrarRequest request)
        {
            return _bl.Prea_frmPreaEstudiov2_Deducciones_Borrar(codEmpresa, request);
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

        [HttpPost("Prea_frmPreaEstudiov2_Creditos_Registrar")]
        public ErrorDto<FrmPreaEstudiov2CreditosResponse> Prea_frmPreaEstudiov2_Creditos_Registrar(
            int codEmpresa,
            [FromBody] FrmPreaEstudiov2CreditoTransitoRegistrarRequest request)
        {
            return _bl.Prea_frmPreaEstudiov2_Creditos_Registrar(codEmpresa, request);
        }

        [HttpPost("Prea_frmPreaEstudiov2_Creditos_Borrar")]
        public ErrorDto<FrmPreaEstudiov2CreditosResponse> Prea_frmPreaEstudiov2_Creditos_Borrar(
            int codEmpresa,
            [FromBody] FrmPreaEstudiov2CreditoTransitoBorrarRequest request)
        {
            return _bl.Prea_frmPreaEstudiov2_Creditos_Borrar(codEmpresa, request);
        }

        [HttpDelete("Prea_frmPreaEstudiov2_Creditos_BorrarFila")]
        public ErrorDto<FrmPreaEstudiov2CreditosResponse> Prea_frmPreaEstudiov2_Creditos_BorrarFila(
            int codEmpresa,
            [FromBody] FrmPreaEstudiov2CreditoTransitoBorrarFilaRequest request)
        {
            return _bl.Prea_frmPreaEstudiov2_Creditos_BorrarFila(codEmpresa, request);
        }

        /// <summary>
        /// Consulta las refundiciones del expediente.
        /// </summary>
        [HttpGet("Prea_frmPreaEstudiov2_Refundiciones_Consultar")]
        public ErrorDto<FrmPreaEstudiov2RefundicionesResponse> Prea_frmPreaEstudiov2_Refundiciones_Consultar(
            int codEmpresa,
            [FromQuery] string cod_preanalisis,
            [FromQuery] string cod_garantia)
        {
            return _bl.Prea_frmPreaEstudiov2_Refundiciones_Consultar(codEmpresa, cod_preanalisis, cod_garantia);
        }

        [HttpPost("Prea_frmPreaEstudiov2_Refundiciones_Actualizar")]
        public ErrorDto<FrmPreaEstudiov2RefundicionesResponse> Prea_frmPreaEstudiov2_Refundiciones_Actualizar(
            int codEmpresa,
            [FromQuery] string cod_garantia,
            [FromBody] FrmPreaEstudiov2RefundicionesActualizarRequest request)
        {
            return _bl.Prea_frmPreaEstudiov2_Refundiciones_Actualizar(codEmpresa, request, cod_garantia);
        }

        [HttpPost("Prea_frmPreaEstudiov2_Refundiciones_ToggleAplica")]
        public ErrorDto<FrmPreaEstudiov2RefundicionesResponse> Prea_frmPreaEstudiov2_Refundiciones_ToggleAplica(
            int codEmpresa,
            [FromQuery] string cod_garantia,
            [FromBody] FrmPreaEstudiov2RefundicionToggleRequest request)
        {
            return _bl.Prea_frmPreaEstudiov2_Refundiciones_ToggleAplica(codEmpresa, request, cod_garantia);
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

        [HttpPost("Prea_frmPreaEstudiov2_Fianzas_Actualizar")]
        public ErrorDto<FrmPreaEstudiov2FianzasResponse> Prea_frmPreaEstudiov2_Fianzas_Actualizar(
            int codEmpresa,
            [FromBody] FrmPreaEstudiov2FianzasActualizarRequest request)
        {
            return _bl.Prea_frmPreaEstudiov2_Fianzas_Actualizar(codEmpresa, request);
        }

        [HttpPost("Prea_frmPreaEstudiov2_Fianzas_ToggleAplica")]
        public ErrorDto<FrmPreaEstudiov2FianzasResponse> Prea_frmPreaEstudiov2_Fianzas_ToggleAplica(
            int codEmpresa,
            [FromBody] FrmPreaEstudiov2FianzaToggleRequest request)
        {
            return _bl.Prea_frmPreaEstudiov2_Fianzas_ToggleAplica(codEmpresa, request);
        }

        /// <summary>
        /// Consulta los desembolsos del expediente.
        /// </summary>
        [HttpGet("Prea_frmPreaEstudiov2_Desembolsos_Consultar")]
        public ErrorDto<FrmPreaEstudiov2DesembolsosResponse> Prea_frmPreaEstudiov2_Desembolsos_Consultar(
            int codEmpresa,
            [FromQuery] string cod_preanalisis,
            [FromQuery] string usuario)
        {
            return _bl.Prea_frmPreaEstudiov2_Desembolsos_Consultar(codEmpresa, cod_preanalisis, usuario);
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
            [FromQuery] int id_desembolso,
            [FromQuery] string usuario)
        {
            return _bl.Prea_frmPreaEstudiov2_Desembolsos_Eliminar(codEmpresa, cod_preanalisis, id_desembolso, usuario);
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

        [HttpPost("Prea_frmPreaEstudiov2_Etiqueta_Agregar")]
        public ErrorDto<FrmPreaEstudiov2HistorialResponse> Prea_frmPreaEstudiov2_Etiqueta_Agregar(
            int codEmpresa,
            [FromBody] FrmPreaEstudiov2EtiquetaAgregarRequest request)
        {
            return _bl.Prea_frmPreaEstudiov2_Etiqueta_Agregar(codEmpresa, request);
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
        /// Guarda un archivo adjunto al expediente.
        /// </summary>
        [HttpPost("Prea_frmPreaEstudiov2_Adjunto_Guardar")]
        public ErrorDto<string> Prea_frmPreaEstudiov2_Adjunto_Guardar(
            int codEmpresa,
            [FromBody] FrmPreaEstudiov2AdjuntoGuardarRequest request)
        {
            return _bl.Prea_frmPreaEstudiov2_Adjunto_Guardar(codEmpresa, request);
        }

        /// <summary>
        /// Elimina un archivo adjunto del expediente.
        /// </summary>
        [HttpPost("Prea_frmPreaEstudiov2_Adjunto_Eliminar")]
        public ErrorDto<string> Prea_frmPreaEstudiov2_Adjunto_Eliminar(
            int codEmpresa,
            [FromBody] FrmPreaEstudiov2AdjuntoEliminarRequest request)
        {
            return _bl.Prea_frmPreaEstudiov2_Adjunto_Eliminar(codEmpresa, request);
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
        /// Guarda la lista de incapacidades del expediente (sbIncapacidades_Guardar en VB6).
        /// </summary>
        [HttpPost("Prea_frmPreaEstudiov2_Incapacidades_Guardar")]
        public ErrorDto<List<FrmPreaEstudiov2IncapacidadDto>> Prea_frmPreaEstudiov2_Incapacidades_Guardar(
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

        [HttpPost("Prea_frmPreaEstudiov2_Extras_Borrar")]
        public ErrorDto<string> Prea_frmPreaEstudiov2_Extras_Borrar(
            int codEmpresa,
            [FromBody] FrmPreaEstudiov2ExtraBorrarRequest request)
        {
            return _bl.Prea_frmPreaEstudiov2_Extras_Borrar(codEmpresa, request);
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

        /// <summary>
        /// Consulta el tab Prendario: log de exámenes y datos de la prenda
        /// (sbPrendario_Load en VB6).
        /// </summary>
        [HttpGet("Prea_frmPreaEstudiov2_Prendario_Consultar")]
        public ErrorDto<FrmPreaEstudiov2PrendarioConsultarResponse> Prea_frmPreaEstudiov2_Prendario_Consultar(
            int codEmpresa,
            [FromQuery] string cod_preanalisis)
        {
            return _bl.Prea_frmPreaEstudiov2_Prendario_Consultar(codEmpresa, cod_preanalisis);
        }

        /// <summary>
        /// Aplica un estado a los exámenes de prenda (btnP_Examenes_Click en VB6).
        /// </summary>
        [HttpPost("Prea_frmPreaEstudiov2_Prendario_Estado")]
        public ErrorDto<FrmPreaEstudiov2PrendarioEstadoResponse> Prea_frmPreaEstudiov2_Prendario_Estado(
            int codEmpresa,
            [FromBody] FrmPreaEstudiov2PrendarioEstadoRequest request)
        {
            return _bl.Prea_frmPreaEstudiov2_Prendario_Estado(codEmpresa, request);
        }
    }
}
