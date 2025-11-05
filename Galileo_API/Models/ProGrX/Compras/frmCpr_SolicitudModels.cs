namespace PgxAPI.Models.CPR
{
    public class CprSolicitudFiltro
    {
        public int? pagina { get; set; }
        public int? paginacion { get; set; }
        public string? filtro { get; set; }
        public List<string>? solicitante { get; set; }
        public List<string>? encargado { get; set; }
    }

    public class CprSolicitudLista
    {
        public int total { get; set; }
        public List<CprSolicitudDto>? solicitudes { get; set; } = new List<CprSolicitudDto>();
    }

    public class CprSolicitudDto
    {
        public int? cpr_id { get; set; }
        public int? cpr_id_madre { get; set; } = 0;
        public string? documento { get; set; }
        public string? cod_unidad { get; set; }
        public string? cod_unidad_solicitante { get; set; }
        public string? detalle { get; set; }
        public bool? i_solicitud_multiple { get; set; } = false;
        public Nullable<DateTime> registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public Nullable<DateTime> modifica_fecha { get; set; }
        public string? modifica_usuario { get; set; }
        public Nullable<DateTime> autoriza_fecha { get; set; }
        public string? autoriza_nota { get; set; }
        public string? presupuesto_estado { get; set; }
        public Nullable<DateTime> presupuesto_fecha { get; set; }
        public string? presupuesto_usuario { get; set; }
        public bool? i_plan_compras { get; set; } = false;
        public bool? i_presupuestado { get; set; } = false;
        public bool? i_contrato_requiere { get; set; } = false;
        public float? monto { get; set; } = 0;
        public string? estado { get; set; }
        public string? val_id { get; set; }
        public string? recomendacion { get; set; }
        public Nullable<DateTime> adjudica_fecha { get; set; }
        public string? adjudica_usuario { get; set; }
        public string? adjudica_orden { get; set; }
        public int? adjudica_proveedor { get; set; } = 0;
        public string? int_sitio_entrega { get; set; }
        public string? int_forma_pago { get; set; }
        public string? int_correo_factura { get; set; }
        public string? int_observaciones { get; set; }
        public int? plazo_entrega_dias_hab { get; set; } = 0;
        public string? autoriza_usuario { get; set; }
        public string? divisa { get; set; } = string.Empty;
        public string? tipo_orden { get; set; } = string.Empty;
        public string? int_tipo_pago { get; set; } = string.Empty;
        public int? com_dir_cod_proveedor { get; set; }
        public string? com_dir_des_proveedor { get; set; }
        public int? porc_multa { get; set; }
        public bool? terminos_condiciones { get; set; } = false;
        public string? horario { get; set; }
        public Nullable<DateTime> recepcion_ofertas { get; set; }
        public string? encargado_usuario { get; set; }
    }

    public class CprSolicitudBsLista
    {
        public int total { get; set; }
        public List<CprSolicitudBsDto> solicitudes { get; set; } = new List<CprSolicitudBsDto>();
    }

    public class CprSolicitudBsDto
    {
        public int cpr_id { get; set; }
        public string cod_producto { get; set; } = string.Empty;
        public string cod_unidad { get; set; } = string.Empty;
        public float monto { get; set; } = 0;
        public int cantidad { get; set; } = 0;
        public int cantidad_entregada { get; set; } = 0;
        public float total { get; set; } = 0;
        public string? pres_estado { get; set; }
        public Nullable<DateTime> pres_fecha { get; set; }
        public string? pres_usuario { get; set; }
        public Nullable<DateTime> registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public Nullable<DateTime> modifica_fecha { get; set; }
        public string? modifica_usuario { get; set; }
        public string? descripcion { get; set; }
        public string? unidad_descripcion { get; set; }
        public string? unidad { get; set; }
        public int? comp_dir_cod_proveedor { get; set; }
        public string? comp_dir_documento { get; set; }
        public string? cod_bodega { get; set; }
        public float? iva_porc { get; set; }
        public float? iva_monto { get; set; }
        public float? desc_porc { get; set; }
        public float? desc_monto { get; set; }
    }

    public class CprValoracionLista
    {
        public string item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class CprUensLista
    {
        public string item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string? cntx_unidad { get; set; } = string.Empty;
        public string? cntx_centro_costo { get; set; } = string.Empty;
    }

    public class CprSolicitudProvDto
    {
        public int cpr_id { get; set; }
        public int proveedor_codigo { get; set; }
        public string? proveedor_estado { get; set; }
        public string? estado { get; set; }
        public string? notas { get; set; }
        public float? sub_total { get; set; }
        public float? descuento { get; set; }
        public float? impuestos { get; set; }
        public float? total { get; set; }
        public string? cod_divisa { get; set; }
        public float? tipo_cambio { get; set; }
        public Nullable<DateTime> cotiza_fecha { get; set; }
        public string? cotiza_usuario { get; set; }
        public string? valora_estado { get; set; }
        public Nullable<DateTime> valora_fecha { get; set; }
        public string? valora_usuario { get; set; }
        public string? valora_puntaje { get; set; }
        public Nullable<DateTime> adjudica_fecha { get; set; }
        public string? adjudica_usuario { get; set; }
        public string? adjudica_orden { get; set; }
        public Nullable<DateTime> registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public Nullable<DateTime> modifica_fecha { get; set; }
        public string? modifica_usuario { get; set; }
        public string? cod_rechazo { get; set; }
        public bool? plazo_entrega { get; set; }
        public string? garantia_desc { get; set; }
        public float? garantia_monto { get; set; }
        public string? descripcion { get; set; }
    }

    public class CprSolicitudPrvBs
    {
        public int cpr_id { get; set; }
        public string cod_producto { get; set; } = string.Empty;
        public int proveedor_codigo { get; set; } = 0;
        public string? codigo { get; set; }
        public float? monto { get; set; } = 0;
        public int? cantidad { get; set; } = 0;
        public float? iva_porc { get; set; } = 0;
        public float? iva_monto { get; set; } = 0;
        public float? desc_porc { get; set; } = 0;
        public float? desc_monto { get; set; } = 0;
        public float? total { get; set; } = 0;
        public float? valora_puntaje { get; set; } = 0;
        public Nullable<DateTime> valora_fecha { get; set; }
        public string? valora_usuario { get; set; }
        public string? valora_notas { get; set; }
        public Nullable<DateTime> registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public Nullable<DateTime> modifica_fecha { get; set; }
        public string? modifica_usuario { get; set; }
        public string estado { get; set; } = string.Empty;
        public string no_cotizacion { get; set; } = string.Empty;
        public string? descripcion { get; set; }
    }

    public class CprSolicitudCotizacionPrvBs
    {
        public int? cpr_id { get; set; }
        public string? cod_producto { get; set; } = string.Empty;
        public string? codigo { get; set; } = string.Empty;
        public int? proveedor_codigo { get; set; } = 0;
        public float? monto { get; set; } = 0;
        public int? cantidad { get; set; } = 0;
        public float? iva_porc { get; set; } = 0;
        public float? iva_monto { get; set; } = 0;
        public float? desc_porc { get; set; } = 0;
        public float? desc_monto { get; set; } = 0;
        public float? total { get; set; } = 0;
        public Nullable<DateTime> registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public string? descripcion { get; set; }
        public string? no_cotizacion { get; set; }
        public string? estado { get; set; }
        public int? id_cotizacion { get; set; }
        public Nullable<DateTime> cotiza_fecha { get; set; }
        public string? cotiza_numero { get; set; }
        public int? seleccionado { get; set; }
        public bool? sel { get; set; }
        public string? marca { get; set; }
        public string? modelo { get; set; }
        public int? id_cotizacion_linea { get; set; }
        public int? garantia { get; set; }
        public Nullable<DateTime> plazo { get; set; }
        public string? unidad { get; set; }
        public decimal tipo_cambio { get; set; }
        public int? plazo_entrega { get; set; }
    }

    public class CprSolicitudCotizacionPrvBsLista
    {
        public int Total { get; set; }
        public List<CprSolicitudCotizacionPrvBs> cotizaciones { get; set; } = new List<CprSolicitudCotizacionPrvBs>();
    }

    public class CprSolicitudProvValItemData
    {
        public int id_valoracion { get; set; } = 0;
        public string val_item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public float peso { get; set; } = 0;
        public int nota { get; set; } = 0;
        public float puntaje { get; set; } = 0;
    }
    
    public class CprSolicitudSeguimientoDto
    {
        public int? cpr_id { get; set; }
        public int? cpr_id_madre { get; set; } = 0;
        public Nullable<DateTime> registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public Nullable<DateTime> modifica_fecha { get; set; }
        public string? modifica_usuario { get; set; }
        public Nullable<DateTime> autoriza_fecha { get; set; }
        public string? autoriza_nota { get; set; }
        public string? presupuesto_usuario { get; set; }
        public Nullable<DateTime> adjudica_fecha { get; set; }
        public string? adjudica_usuario { get; set; }
        public string? adjudica_orden { get; set; }
        public int? adjudica_proveedor { get; set; } = 0;
        public string? autoriza_usuario { get; set; }
        public string? detalle_seguimiento { get; set; }
    }

    public class CprProveedorDto
    {
        public string descripcion { get; set; } = string.Empty;
        public string cedjur { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
    }

    public class CprValoracionCotizacionData
    {
        public int id_valoracion { get; set; }
        public string val_item { get; set; } = string.Empty;
        public int cpr_id { get; set; }
        public int proveedor_codigo { get; set; }
        public float puntaje { get; set; }
        public float peso { get; set; }
        public float nota { get; set; }
        public Nullable<DateTime> registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public Nullable<DateTime> modifica_fecha { get; set; }
        public string? modifica_usuario { get; set; }
    }

    public class CprSolicitusValoracionGuardar
    {
        public List<CprSolicitudPrvBs> productos { get; set; } = new List<CprSolicitudPrvBs>();
        public List<CprSolicitudProvValItemData> valoracion { get; set; } = new List<CprSolicitudProvValItemData>();
        public CprSolicitudPrvBs cotizacion { get; set; } = new CprSolicitudPrvBs();
    }

    public class CprParametrosValBusqueda
    {
        public string val_id { get; set; } = string.Empty;
        public int crp_id { get; set; }
        public int proveedor { get; set; }
    }

    public class CprSolicitudAdjudicaConsulta
    {
        public int crp_id { get; set; }
        public int proveedor_codigo { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public string no_cotizacion { get; set; } = string.Empty;
        public float suma_total { get; set; } = 0;
        public float valora_puntaje { get; set; } = 0;
        public string valora_estado { get; set; } = string.Empty;
        public bool adjudica_ind { get; set; } = false;
        public string adjudica_orden { get; set; } = string.Empty;
    }

    public class CprSolicitusCotizacionGuardar
    {
        public List<CprSolicitudCotizacionPrvBs> listacotizacion { get; set; } = new List<CprSolicitudCotizacionPrvBs>();
        public int proveedor_codigo { get; set; }
        public string? cotiza_numero { get; set; }
        public string? no_cotizacion { get; set; }
        public int? garantia { get; set; }
        public decimal tipo_cambio { get; set; }
        public Nullable<DateTime> plazo { get; set; }
    }

    public class CprSolicitusCotizacionGuardarDetalle
    {
        public List<CprSolicitudCotizacionPrvBs> listacotizacion { get; set; } = new List<CprSolicitudCotizacionPrvBs>();
        public CprSolicitudPrvBs cotizacion { get; set; } = new CprSolicitudPrvBs();
        public List<CprSolicitudCotizacionItemData> cotizaciond { get; set; } = new List<CprSolicitudCotizacionItemData>();
        public int proveedor_codigo { get; set; }
        public string? no_cotizacion { get; set; }
    }

    public class CprSolicitudCotizacionItemData
    {
        public int cpr_id { get; set; }
        public string cod_producto { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public int proveedor_codigo { get; set; } = 0;
        public float? monto { get; set; } = 0;
        public int? cantidad { get; set; } = 0;
        public float? iva_porc { get; set; } = 0;
        public float? iva_monto { get; set; } = 0;
        public float? desc_porc { get; set; } = 0;
        public float? desc_monto { get; set; } = 0;
        public float? total { get; set; } = 0;
        public Nullable<DateTime> registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public string? descripcion { get; set; }
        public string? no_cotizacion { get; set; }
        public string? estado { get; set; }
    }

    public class CprSolicitudAdjudicaGuardar
    {
        public int cpr_id { get; set; }
        public CprSolicitudAdjudicaConsulta proveedor { get; set; } = new CprSolicitudAdjudicaConsulta();
        public List<CprSolicitudAdjudicaProductosDto> productos { get; set; } = new List<CprSolicitudAdjudicaProductosDto>();
        public string usuario { get; set; } = string.Empty;
    }

    public class CprSolicitudAdjudicaProductosDto
    {
        public string cod_producto { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public float monto { get; set; } = 0;
        public int cantidad { get; set; } = 0;
        public float desc_monto { get; set; } = 0;
        public float iva_monto { get; set; } = 0;
        public float total { get; set; } = 0;
        public bool? adjudica_ind { get; set; } = false;
        public bool? ocupado { get; set; } = false;
    }

    public class CprSolicitudProvCotiza
    {
        public int id_cotizacion { get; set; }
        public int cpr_id { get; set; }
        public int proveedor_codigo { get; set; }
        public Nullable<DateTime> cotiza_fecha { get; set; }
        public string? cotiza_numero { get; set; }
        public Nullable<DateTime> registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public Nullable<DateTime> modifica_fecha { get; set; }
        public string? modifica_usuario { get; set; }

    }

    public class SolicitudMontosDto
    {
        public float MontoMaximo { get; set; }
        public float MontoAdjudicado { get; set; }
        public float? MontoOrden { get; set; }
    }

    public class EncargadosDto
    {
        public string? core_usuario { get; set; }
        public int cod_unidad { get; set; }
        public string? nombre { get; set; }
    }
}