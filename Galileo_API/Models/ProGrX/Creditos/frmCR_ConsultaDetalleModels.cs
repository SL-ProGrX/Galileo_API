namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrConsultaDetalleCompletoDto
    {
        public CrConsultaDetalleEncabezadoDto encabezado { get; set; } = new();
        public CrConsultaDetalleFormalizacionDto formalizacion { get; set; } = new();
        public CrConsultaDetalleAprobacionDto aprobacion { get; set; } = new();
        public CrConsultaDetalleOtrosDto otros { get; set; } = new();
        public CrConsultaDetalleBancosDto bancos { get; set; } = new();
    }

    public class CrConsultaDetalleEncabezadoDto
    {
        public int id_solicitud { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string cod_divisa { get; set; } = string.Empty;
        public string garantia { get; set; } = string.Empty;
        public string garantia_desc { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string estado_desc { get; set; } = string.Empty;
        public string proceso { get; set; } = string.Empty;
        public string proceso_desc { get; set; } = string.Empty;
        public string estado_prestamo { get; set; } = string.Empty;
        public string antiguedad { get; set; } = string.Empty;
        public decimal monto_girado { get; set; }
        public decimal monto_credito { get; set; }
        public decimal saldo_credito { get; set; }
        public decimal cuota { get; set; }
        public decimal interesc { get; set; }
        public decimal amortiza { get; set; }
        public decimal poliza_cuota { get; set; }
        public int mora_cuotas { get; set; }
    }

    public class CrConsultaDetalleFormalizacionDto
    {
        public string cod_destino { get; set; } = string.Empty;
        public string destino_desc { get; set; } = string.Empty;
        public string cod_oficina_r { get; set; } = string.Empty;
        public string oficina_desc { get; set; } = string.Empty;
        public string cod_grupo { get; set; } = string.Empty;
        public string recurso_desc { get; set; } = string.Empty;
        public string cod_actividad { get; set; } = string.Empty;
        public string actividad_desc { get; set; } = string.Empty;
        public string canal_tipo { get; set; } = string.Empty;
        public string canal_desc { get; set; } = string.Empty;
        public string id_comite { get; set; } = string.Empty;
        public string comite_desc { get; set; } = string.Empty;
        public string id_promotor { get; set; } = string.Empty;
        public string promotor_desc { get; set; } = string.Empty;
        public string userfor { get; set; } = string.Empty;
        public DateTime? fechaforp { get; set; }
        public string tdocumento { get; set; } = string.Empty;
        public string ndocumento { get; set; } = string.Empty;
        public string comprobante { get; set; } = string.Empty;
        public int plazo { get; set; }
        public decimal interesv { get; set; }
        public decimal txt_int_mora { get; set; }
        public string tasa_label { get; set; } = string.Empty;
        public string tbp_puntos_add { get; set; } = string.Empty;
        public string pts_add_mora { get; set; } = string.Empty;
        public decimal pts_add_liq { get; set; }
        public string tasa_piso { get; set; } = string.Empty;
        public string prideduc { get; set; } = string.Empty;
        public string fecult { get; set; } = string.Empty;
        public int? anio_primer_abono { get; set; }
        public int? mes_primer_abono { get; set; }
        public int? anio_ultimo_abono { get; set; }
        public int? mes_ultimo_abono { get; set; }
        public int? anio_terminacion { get; set; }
        public int? mes_terminacion { get; set; }
        public string dia_pago_desc { get; set; } = string.Empty;
        public string base_calculo_desc { get; set; } = string.Empty;
        public string cuotas_planilla { get; set; } = string.Empty;
        public string cuotas_directas { get; set; } = string.Empty;
        public string cuotas_anuladas { get; set; } = string.Empty;
        public string fecha_proceso { get; set; } = string.Empty;
    }

    public class CrConsultaDetalleAprobacionDto
    {
        public string tipo_acta { get; set; } = "Estudio Crédito";
        public string tipo_detalle { get; set; } = "RES";
        public string acta { get; set; } = string.Empty;
        public DateTime? acta_fecha { get; set; }
        public List<CrConsultaDetalleResolucionDto> lista { get; set; } = new();
    }

    public class CrConsultaDetalleResolucionDto
    {
        public string acta { get; set; } = string.Empty;
        public string acta_estado { get; set; } = string.Empty;
        public string acta_notas { get; set; } = string.Empty;
        public DateTime? acta_fecha { get; set; }
        public string estado { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
    }

    public class CrConsultaDetalleOtrosDto
    {
        public string cuenta_iban { get; set; } = string.Empty;
        public string iban { get; set; } = string.Empty;
        public string salida_tipo { get; set; } = string.Empty;
        public string salida_desc { get; set; } = string.Empty;
        public string documento_referido { get; set; } = string.Empty;
        public string salida_tipo_desc { get; set; } = string.Empty;
        public string deductora_cod { get; set; } = string.Empty;
        public string deductora_desc { get; set; } = string.Empty;
        public string deductora_desc_corta { get; set; } = string.Empty;
        public string divisa_desc { get; set; } = string.Empty;
        public string currency_sim { get; set; } = string.Empty;
        public int cbr_externo { get; set; }
        public int cobro_fiador { get; set; }
    }

    public class CrConsultaDetalleBancosDto
    {
        public string texto { get; set; } = string.Empty;
    }

    public class CrConsultaDetalleListaResult<T>
    {
        public int total { get; set; }
        public List<T> lista { get; set; } = new();
    }

    public class CrConsultaDetalleMovimientoDto
    {
        public decimal proceso { get; set; }
        public DateTime? fecha_corte { get; set; }
        public int num_cuota { get; set; }
        public DateTime? fecha { get; set; }
        public decimal total { get; set; }
        public decimal intcor { get; set; }
        public decimal intmor { get; set; }
        public decimal principal { get; set; }
        public decimal poliza { get; set; }
        public decimal cargo { get; set; }
        public decimal saldo { get; set; }
        public string tcon2 { get; set; } = string.Empty;
        public string ncon { get; set; } = string.Empty;
        public string concepto { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string cajas { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
    }

    public class CrConsultaDetalleMorosidadDto
    {
        public string proceso { get; set; } = string.Empty;
        public decimal intcor { get; set; }
        public decimal intmor { get; set; }
        public decimal principal { get; set; }
        public decimal poliza { get; set; }
        public decimal cargos { get; set; }
        public decimal total { get; set; }
    }

    public class CrConsultaDetalleCierreDto
    {
        public int anio { get; set; }
        public string mes { get; set; } = string.Empty;
        public decimal saldo_final { get; set; }
        public decimal total_debitos { get; set; }
        public decimal total_creditos { get; set; }
        public string opex { get; set; } = string.Empty;
        public string proceso { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string prideduc { get; set; } = string.Empty;
        public string fecult { get; set; } = string.Empty;
        public decimal tasa { get; set; }
        public int plazo { get; set; }
        public decimal cuota { get; set; }
    }

    public class CrConsultaDetalleCorreccionDto
    {
        public DateTime? fecha { get; set; }
        public string movimiento { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
    }

    public class CrConsultaDetalleFiadorDto
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string institucion { get; set; } = string.Empty;
    }

    public class CrConsultaDetalleRefundicionDto
    {
        public int id_solicitud { get; set; }
        public string codigo { get; set; } = string.Empty;
        public decimal intcor { get; set; }
        public decimal intmor { get; set; }
        public decimal cargo { get; set; }
        public decimal monto { get; set; }
    }

    public class CrConsultaDetalleDesembolsoDto
    {
        public string concepto { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public string cuenta_conta { get; set; } = string.Empty;
        public string retener { get; set; } = string.Empty;
        public string modifica { get; set; } = string.Empty;
    }

    public class CrConsultaDetalleTagDto
    {
        public string tag_codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
    }
}