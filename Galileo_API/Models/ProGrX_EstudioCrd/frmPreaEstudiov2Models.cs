namespace Galileo_API.Models.ProGrX_EstudioCrd
{
    #region Carga / Scroll

    public class FrmPreaEstudiov2CargaRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
    }

    public class FrmPreaEstudiov2ScrollRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonRequired]
        public int scroll_code { get; set; }
    }

    public class FrmPreaEstudiov2ScrollResponse
    {
        public string cod_preanalisis { get; set; } = string.Empty;
    }

    public class FrmPreaEstudiov2CargaResponse
    {
        public FrmPreaEstudiov2EstadoDto estado { get; set; } = new();
        public FrmPreaEstudiov2EncabezadoDto encabezado { get; set; } = new();
        public FrmPreaEstudiov2CreditoDto credito { get; set; } = new();
        public FrmPreaEstudiov2SalariosDto salarios { get; set; } = new();
        public FrmPreaEstudiov2CatalogosResponse catalogos { get; set; } = new();
    }

    public class FrmPreaEstudiov2EstadoDto
    {
        public string cod_preanalisis { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string estado_desc { get; set; } = string.Empty;
        public string estado_v2 { get; set; } = string.Empty;
        public string estado_v2_desc { get; set; } = string.Empty;
        public bool editable { get; set; }
        public bool tiene_alerta { get; set; }
        public string mensaje_alerta { get; set; } = string.Empty;
    }

    public class FrmPreaEstudiov2EncabezadoDto
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string sexo { get; set; } = string.Empty;
        public DateTime? fecha_nacimiento { get; set; }
        public int edad { get; set; }
        public string estado_persona { get; set; } = string.Empty;
        public string clasificacion_crediticia { get; set; } = string.Empty;
        public int edad_aplica { get; set; }
        public string edad_justificacion { get; set; } = string.Empty;
    }

    public class FrmPreaEstudiov2CreditoDto
    {
        public string linea { get; set; } = string.Empty;
        public string destino { get; set; } = string.Empty;
        public string garantia { get; set; } = string.Empty;
        public int fiadores { get; set; }
        public string contrato { get; set; } = string.Empty;
        public string no_op_crm { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public decimal tasa { get; set; }
        public int plazo { get; set; }
        public decimal cuota { get; set; }
        public decimal monto_construccion { get; set; }
        public bool poliza_vida { get; set; }
        public bool poliza_incendio { get; set; }
        public bool poliza_prenda { get; set; }
        public bool poliza_desempleo { get; set; }
        public decimal monto_poliza_vida { get; set; }
        public decimal monto_poliza_incendio { get; set; }
        public decimal monto_poliza_prenda { get; set; }
        public decimal monto_poliza_desempleo { get; set; }
        public decimal compromiso { get; set; }
    }

    public class FrmPreaEstudiov2SalariosDto
    {
        public string tipo_salario { get; set; } = string.Empty;
        public DateTime? corte_colilla { get; set; }
        public decimal salario_devengado { get; set; }
        public decimal salario_mensual { get; set; }
        public decimal salario_constancia { get; set; }
        public decimal salario_orden_patronal { get; set; }
        public decimal ingreso_privado { get; set; }
        public decimal ingreso_privado_porc { get; set; }
        public int componente_adicional_id { get; set; }
        public decimal componente_adicional_porc { get; set; }
        public decimal componentes_adicionales { get; set; }
        public decimal total_extras { get; set; }
        public List<FrmPreaEstudiov2SalarioDetalleDto> tabla_salarios { get; set; } = [];
        public List<FrmPreaEstudiov2ExtraDto> extras { get; set; } = [];
        public List<FrmPreaEstudiov2IncapacidadDto> incapacidades { get; set; } = [];
    }

    public class FrmPreaEstudiov2SalarioDetalleDto
    {
        public int orden { get; set; }
        public DateTime? fecha { get; set; }
        public decimal salario_s { get; set; }
        public int mes { get; set; }
        public decimal salario_rh { get; set; }
        public decimal ca { get; set; }
    }

    public class FrmPreaEstudiov2ExtraDto
    {
        public int idx { get; set; }
        public string cod_extras { get; set; } = string.Empty;
        public string tipo_extra { get; set; } = string.Empty;
        public decimal monto { get; set; }
    }

    public class FrmPreaEstudiov2IncapacidadDto
    {
        public int orden { get; set; }
        public DateTime? desde { get; set; }
        public DateTime? hasta { get; set; }
        public int dias { get; set; }
    }

    public class FrmPreaEstudiov2CatalogosResponse
    {
        public List<FrmPreaEstudiov2DropdownDto> expedientes { get; set; } = [];
        public List<FrmPreaEstudiov2DropdownDto> lineas { get; set; } = [];
        public List<FrmPreaEstudiov2DropdownDto> destinos { get; set; } = [];
        public List<FrmPreaEstudiov2DropdownDto> garantias { get; set; } = [];
        public List<FrmPreaEstudiov2DropdownDto> tipos_salario { get; set; } = [];
        public List<FrmPreaEstudiov2DropdownDto> componentes_adicionales { get; set; } = [];
        public List<FrmPreaEstudiov2DropdownDto> comites { get; set; } = [];
        public List<FrmPreaEstudiov2DropdownDto> bancos { get; set; } = [];
    }

    public class FrmPreaEstudiov2DropdownDto
    {
        public string item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    #endregion

    #region Guardar

    public class FrmPreaEstudiov2GuardarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string sexo { get; set; } = string.Empty;
        public DateTime? fecha_nacimiento { get; set; }
        public string linea { get; set; } = string.Empty;
        public string destino { get; set; } = string.Empty;
        public string garantia { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonRequired]
        public int fiadores { get; set; }
        public string contrato { get; set; } = string.Empty;
        public string no_op_crm { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonRequired]
        public decimal monto { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public decimal tasa { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public int plazo { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public decimal cuota { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public decimal monto_construccion { get; set; }
        public string tipo_salario { get; set; } = string.Empty;
        public DateTime? corte_colilla { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public decimal salario_devengado { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public decimal salario_mensual { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public decimal salario_constancia { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public decimal salario_orden_patronal { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public decimal ingreso_privado { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public decimal ingreso_privado_porc { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public int componente_adicional_id { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public decimal componente_adicional_porc { get; set; }
        public string notas_cumplimiento { get; set; } = string.Empty;
    }

    public class FrmPreaEstudiov2GuardarResponse
    {
        public string cod_preanalisis { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string estado_desc { get; set; } = string.Empty;
        public string mensaje { get; set; } = string.Empty;
    }

    #endregion

    #region Salarios

    public class FrmPreaEstudiov2SalariosGuardarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
        public string tipo_salario { get; set; } = string.Empty;
        public DateTime? corte_colilla { get; set; }
        public decimal salario_devengado { get; set; }
        public decimal salario_mensual { get; set; }
        public decimal salario_constancia { get; set; }
        public decimal salario_orden_patronal { get; set; }
        public decimal ingreso_privado { get; set; }
        public decimal ingreso_privado_porc { get; set; }
        public int componente_adicional_id { get; set; }
        public decimal componente_adicional_porc { get; set; }
    }

    public class FrmPreaEstudiov2ExtraGuardarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
        public string cod_extras { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonRequired]
        public decimal monto { get; set; }
    }

    #endregion

    #region Deducciones

    public class FrmPreaEstudiov2DeduccionesResponse
    {
        public List<FrmPreaEstudiov2DeduccionesDetalleDto> deducciones { get; set; } = [];
        public decimal total_deducciones { get; set; }
    }

    public class FrmPreaEstudiov2DeduccionesDetalleDto
    {
        public int orden { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public string tipo { get; set; } = string.Empty;
    }

    public class FrmPreaEstudiov2DeduccionesAgregarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonRequired]
        public decimal monto { get; set; }
    }

    #endregion

    #region Créditos (tránsito)

    public class FrmPreaEstudiov2CreditosResponse
    {
        public List<FrmPreaEstudiov2CreditoTransitoDto> cancelados { get; set; } = [];
        public List<FrmPreaEstudiov2CreditoTransitoDto> por_cobrar { get; set; } = [];
        public decimal total_cancelados { get; set; }
        public decimal total_por_cobrar { get; set; }
    }

    public class FrmPreaEstudiov2CreditoTransitoDto
    {
        public int id_solicitud { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public decimal capital { get; set; }
        public decimal cuota { get; set; }
        public int cuotas_pendientes { get; set; }
        public decimal saldo { get; set; }
    }

    #endregion

    #region Refundiciones

    public class FrmPreaEstudiov2RefundicionesResponse
    {
        public List<FrmPreaEstudiov2RefundicionDto> refundiciones { get; set; } = [];
        public decimal total_cuotas { get; set; }
        public decimal total_refunde { get; set; }
        public decimal total_mora { get; set; }
    }

    public class FrmPreaEstudiov2RefundicionDto
    {
        public int orden { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public decimal cuota { get; set; }
        public decimal refunde { get; set; }
        public decimal mora { get; set; }
    }

    #endregion

    #region Fianzas

    public class FrmPreaEstudiov2FianzasResponse
    {
        public List<FrmPreaEstudiov2FianzaDto> fianzas { get; set; } = [];
        public decimal total_saldos { get; set; }
        public decimal total_cuotas { get; set; }
    }

    public class FrmPreaEstudiov2FianzaDto
    {
        public int orden { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public decimal saldo { get; set; }
        public decimal cuota { get; set; }
    }

    #endregion

    #region Desembolsos

    public class FrmPreaEstudiov2DesembolsosResponse
    {
        public List<FrmPreaEstudiov2DesembolsoDto> desembolsos { get; set; } = [];
        public List<FrmPreaEstudiov2DropdownDto> bancos { get; set; } = [];
    }

    public class FrmPreaEstudiov2DesembolsoDto
    {
        public int id_desembolso { get; set; }
        public string tipo { get; set; } = string.Empty;
        public string banco { get; set; } = string.Empty;
        public string cuenta { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public string concepto { get; set; } = string.Empty;
    }

    public class FrmPreaEstudiov2DesembolsoGuardarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string cod_banco { get; set; } = string.Empty;
        public string cuenta { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonRequired]
        public decimal monto { get; set; }
        public string concepto { get; set; } = string.Empty;
    }

    #endregion

    #region Historial

    public class FrmPreaEstudiov2HistorialResponse
    {
        public List<FrmPreaEstudiov2HistorialDto> ejecutivos { get; set; } = [];
        public List<FrmPreaEstudiov2HistorialDto> general { get; set; } = [];
    }

    public class FrmPreaEstudiov2HistorialDto
    {
        public DateTime fecha { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string accion { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
    }

    #endregion

    #region Adjuntos

    public class FrmPreaEstudiov2AdjuntoDto
    {
        public int id_adjunto { get; set; }
        public string nombre_archivo { get; set; } = string.Empty;
        public DateTime fecha { get; set; }
        public string usuario { get; set; } = string.Empty;
    }

    #endregion

    #region Resolución

    public class FrmPreaEstudiov2ResolucionResponse
    {
        public string comite { get; set; } = string.Empty;
        public string sesion { get; set; } = string.Empty;
        public string autorizador { get; set; } = string.Empty;
        public string resolucion { get; set; } = string.Empty;
        public string observaciones { get; set; } = string.Empty;
        public List<FrmPreaEstudiov2HistorialDto> historial { get; set; } = [];
    }

    #endregion

    #region Observaciones

    public class FrmPreaEstudiov2ObservacionesRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string observaciones { get; set; } = string.Empty;
    }

    #endregion

    #region Comité

    public class FrmPreaEstudiov2ComiteAsignarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
        public string comite { get; set; } = string.Empty;
    }

    public class FrmPreaEstudiov2ComiteAsignarResponse
    {
        public string comite { get; set; } = string.Empty;
        public bool asignado { get; set; }
        public string mensaje { get; set; } = string.Empty;
    }

    #endregion

    #region Copiar

    public class FrmPreaEstudiov2CopiarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis_origen { get; set; } = string.Empty;
    }

    public class FrmPreaEstudiov2CopiarResponse
    {
        public string cod_preanalisis_nuevo { get; set; } = string.Empty;
        public string mensaje { get; set; } = string.Empty;
    }

    #endregion

    #region Solicitar

    public class FrmPreaEstudiov2SolicitarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
    }

    public class FrmPreaEstudiov2SolicitarResponse
    {
        public string cod_preanalisis { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string estado_desc { get; set; } = string.Empty;
        public string mensaje { get; set; } = string.Empty;
    }

    #endregion

    #region Incapacidades

    public class FrmPreaEstudiov2IncapacidadGuardarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonRequired]
        public DateTime desde { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public DateTime hasta { get; set; }
    }

    #endregion

    #region Hipotecario

    public class FrmPreaEstudiov2HipotecarioRequest
    {
        public string cod_preanalisis { get; set; } = string.Empty;
        public long id_solicitud { get; set; } = 0;
        public string usuario { get; set; } = string.Empty;
    }

    public class FrmPreaEstudiov2HipotecarioResponse
    {
        public decimal monto_avaluo_factor_cfia { get; set; }
        public bool habilita_montos_hipoteca { get; set; }
        public bool habilita_sumar_avaluo_cfia { get; set; }
        public bool habilita_garantia_hipoteca { get; set; }
        public bool habilita_asignar_ingenieros { get; set; }
        public bool habilita_cambio_estado { get; set; }
        public string mensaje_bloqueo { get; set; } = string.Empty;
    }

    #endregion

    #region Abandonar

    public class FrmPreaEstudiov2AbandonarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
    }

    public class FrmPreaEstudiov2AbandonarResponse
    {
        public string cod_preanalisis { get; set; } = string.Empty;
        public string estado { get; set; } = "B";
        public string estado_desc { get; set; } = "Abandonado";
        public string mensaje { get; set; } = "Se ha ABANDONADO el expediente correctamente.";
    }

    #endregion
}
