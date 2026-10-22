namespace Galileo.Models.ProGrX_Inversiones
{
    public sealed class IvrRecTitulosCatalogoData
    {
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public int dias { get; set; }
    }

    public sealed class IvrRecTitulosInicialData
    {
        public DateTime fecha_servidor { get; set; }
        public string divisa_local { get; set; } = string.Empty;
        public List<IvrRecTitulosCatalogoData> instrumentos { get; set; } = [];
        public List<IvrRecTitulosCatalogoData> recursos { get; set; } = [];
        public List<IvrRecTitulosCatalogoData> emisores { get; set; } = [];
        public List<IvrRecTitulosCatalogoData> administradores { get; set; } = [];
        public List<IvrRecTitulosCatalogoData> portafolios { get; set; } = [];
        public List<IvrRecTitulosCatalogoData> clasificaciones { get; set; } = [];
        public List<IvrRecTitulosCatalogoData> reservas { get; set; } = [];
        public List<IvrRecTitulosCatalogoData> periodicidades { get; set; } = [];
        public List<IvrRecTitulosCatalogoData> divisas { get; set; } = [];
    }

    public sealed class IvrRecTitulosBusquedaData
    {
        public long titulo_id { get; set; }
        public string operacion { get; set; } = string.Empty;
        public string serie { get; set; } = string.Empty;
        public string estado_desc { get; set; } = string.Empty;
        public string instrumento_desc { get; set; } = string.Empty;
        public string administrador_desc { get; set; } = string.Empty;
        public string recurso_desc { get; set; } = string.Empty;
    }

    public sealed class IvrRecTitulosNavegacionData
    {
        public long titulo_id { get; set; }
    }

    public sealed class IvrRecTitulosData
    {
        public long titulo_id { get; set; }
        public string estado { get; set; } = string.Empty;
        public string estado_desc { get; set; } = string.Empty;
        public string ib_documento { get; set; } = string.Empty;
        public string tipo_instrumento { get; set; } = string.Empty;
        public string instrumento_idx { get; set; } = string.Empty;
        public string instrumento_desc { get; set; } = string.Empty;
        public string cod_emisor { get; set; } = string.Empty;
        public string emisor_desc { get; set; } = string.Empty;
        public string cod_administrador { get; set; } = string.Empty;
        public string administrador_desc { get; set; } = string.Empty;
        public string cod_categoria { get; set; } = string.Empty;
        public string categoria_desc { get; set; } = string.Empty;
        public string cod_recurso { get; set; } = string.Empty;
        public string recurso_desc { get; set; } = string.Empty;
        public string cod_portafolio { get; set; } = string.Empty;
        public string portafolio_desc { get; set; } = string.Empty;
        public string cod_reserva { get; set; } = string.Empty;
        public string reserva_desc { get; set; } = string.Empty;
        public string cod_divisa { get; set; } = string.Empty;
        public string divisa_desc { get; set; } = string.Empty;
        public string cod_periodicidad { get; set; } = string.Empty;
        public string periodicidad_desc { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
        public string ejecutivo_cuenta { get; set; } = string.Empty;
        public string operacion { get; set; } = string.Empty;
        public string serie { get; set; } = string.Empty;
        public string isin { get; set; } = string.Empty;
        public string cusip { get; set; } = string.Empty;
        public int base_intereses { get; set; }
        public bool calcula_intereses { get; set; }
        public decimal tipo_cambio { get; set; }
        public decimal valor_facial { get; set; }
        public decimal valor_transado { get; set; }
        public decimal costo_neto { get; set; }
        public decimal comision_puesto { get; set; }
        public decimal comision_bnv { get; set; }
        public decimal iva { get; set; }
        public decimal tasa_inicial { get; set; }
        public decimal rend_neto { get; set; }
        public decimal rend_nominal { get; set; }
        public decimal tasa_efectiva { get; set; }
        public decimal tir { get; set; }
        public int ndias_inversion { get; set; }
        public int interes_acum_dias { get; set; }
        public decimal interes_acum_monto { get; set; }
        public DateTime? fecha_pago { get; set; }
        public decimal pyd_base { get; set; }
        public decimal precio { get; set; }
        public decimal participacion_numero { get; set; }
        public decimal participacion_valor { get; set; }
        public decimal monto_principal { get; set; }
        public DateTime fecha_compra { get; set; }
        public DateTime? fecha_vencimiento { get; set; }
        public decimal isr_monto { get; set; }
        public decimal valor_libros { get; set; }
        public DateTime? fecha_corte { get; set; }
    }

    public sealed class IvrRecTitulosTransaccionData
    {
        public long transac_id { get; set; }
        public string tipo_mov_desc { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; }
        public decimal importe { get; set; }
        public string cta_mask { get; set; } = string.Empty;
        public string tipo_doc_desc { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
        public long? tes_nsolicitud { get; set; }
        public string tes_estado_desc { get; set; } = string.Empty;
        public string cod_divisa { get; set; } = string.Empty;
        public decimal tipo_cambio { get; set; }
        public decimal monto { get; set; }
    }

    public sealed class IvrRecTitulosComisionData
    {
        public string cod_comision { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public string cod_cuenta_mask { get; set; } = string.Empty;
    }

    public sealed class IvrRecTitulosFlujoData
    {
        public DateTime corte { get; set; }
        public string tipo { get; set; } = string.Empty;
        public int dias { get; set; }
        public decimal monto { get; set; }
        public int acum_dias { get; set; }
        public decimal acum_monto { get; set; }
        public decimal saldo { get; set; }
        public decimal inversion { get; set; }
        public decimal monto_diario { get; set; }
        public decimal interes_calculado { get; set; }
        public decimal interes_aplicado { get; set; }
        public decimal acum_monto_neto { get; set; }
        public decimal cupon { get; set; }
        public decimal isr_aplicado { get; set; }
        public decimal isr_saldo { get; set; }
        public decimal isr_acumulado { get; set; }
    }

    public sealed class IvrRecTitulosCierreData
    {
        public DateTime corte { get; set; }
        public decimal valor_facial { get; set; }
        public decimal valor_libros { get; set; }
        public decimal valor_mercado { get; set; }
        public decimal tasa_actual { get; set; }
        public string pyd_tipo { get; set; } = string.Empty;
        public decimal pyd_mes_monto { get; set; }
        public decimal pyd_monto_acumulado { get; set; }
        public decimal pyd_saldo { get; set; }
        public int dias_mes { get; set; }
        public decimal dias_mes_monto { get; set; }
        public int dias_acumulados { get; set; }
        public decimal dias_acum_monto { get; set; }
    }

    public sealed class IvrRecTitulosFondoData
    {
        public long fi_transac { get; set; }
        public string estado { get; set; } = string.Empty;
        public int seq_id { get; set; }
        public string tipo_mov { get; set; } = string.Empty;
        public string tipo_mov_desc { get; set; } = string.Empty;
        public DateTime fecha { get; set; }
        public decimal saldo_actual { get; set; }
        public decimal mnt_principal { get; set; }
        public decimal mnt_intereses { get; set; }
        public decimal n_participaciones { get; set; }
        public decimal v_participaciones { get; set; }
    }

    public sealed class IvrRecTitulosCuponData
    {
        public long cupon_id { get; set; }
        public int seq_id { get; set; }
        public DateTime fecha_inicio { get; set; }
        public DateTime fecha_corte { get; set; }
        public decimal mnt_interes { get; set; }
        public decimal mnt_interes_apl { get; set; }
        public decimal mnt_principal { get; set; }
        public string documento { get; set; } = string.Empty;
        public string referencia { get; set; } = string.Empty;
    }

    public sealed class IvrRecTitulosAsientoData
    {
        public string tipo_asiento { get; set; } = string.Empty;
        public string num_asiento { get; set; } = string.Empty;
        public DateTime fecha { get; set; }
        public string referencia { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime? traslado_fecha { get; set; }
        public string traslado_usuario { get; set; } = string.Empty;
    }

    public sealed class IvrRecTitulosAsientoDetalleData
    {
        public int linea_id { get; set; }
        public string cod_cuenta_mask { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal monto_debito { get; set; }
        public decimal monto_credito { get; set; }
        public string cod_divisa { get; set; } = string.Empty;
        public decimal tipo_cambio { get; set; }
        public string num_asiento { get; set; } = string.Empty;
        public string documento { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
        public string referencia { get; set; } = string.Empty;
    }

    public sealed class IvrRecTitulosGuardarRequest
    {
        public long titulo_id { get; set; }
        public string tipo_instrumento { get; set; } = string.Empty;
        public string cod_instrumento { get; set; } = string.Empty;
        public string cod_categoria { get; set; } = string.Empty;
        public string cod_emisor { get; set; } = string.Empty;
        public string cod_administrador { get; set; } = string.Empty;
        public string cod_portafolio { get; set; } = string.Empty;
        public string cod_recurso { get; set; } = string.Empty;
        public string cod_reserva { get; set; } = string.Empty;
        public string cod_divisa { get; set; } = string.Empty;
        public string cod_periodicidad { get; set; } = string.Empty;
        public int base_intereses { get; set; }
        public string operacion { get; set; } = string.Empty;
        public string serie { get; set; } = string.Empty;
        public string isin { get; set; } = string.Empty;
        public string cusip { get; set; } = string.Empty;
        public string ejecutivo_cuenta { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
        public DateTime fecha_compra { get; set; }
        public DateTime? fecha_vencimiento { get; set; }
        public decimal tipo_cambio { get; set; }
        public decimal tasa_inicial { get; set; }
        public decimal precio { get; set; }
        public decimal valor_facial { get; set; }
        public decimal valor_transado { get; set; }
        public decimal comision_puesto { get; set; }
        public decimal comision_bnv { get; set; }
        public decimal iva { get; set; }
        public string ib_documento { get; set; } = string.Empty;
        public decimal rend_neto { get; set; }
        public decimal rend_nominal { get; set; }
        public decimal tir { get; set; }
        public decimal tasa_efectiva { get; set; }
        public decimal isr_monto { get; set; }
        public decimal participacion_valor { get; set; }
        public decimal participacion_numero { get; set; }
        public bool calcula_intereses { get; set; }
        public string usuario { get; set; } = string.Empty;
    }

    public sealed class IvrRecTitulosTransaccionEliminarRequest
    {
        public long titulo_id { get; set; }
        public long transac_id { get; set; }
    }

    public sealed class IvrRecTitulosAccionRequest
    {
        public long titulo_id { get; set; }
        public string usuario { get; set; } = string.Empty;
        public decimal tasa_efectiva { get; set; }
    }

    public sealed class IvrRecTitulosGuardarData
    {
        public long titulo_id { get; set; }
        public string mensaje { get; set; } = string.Empty;
    }
}
