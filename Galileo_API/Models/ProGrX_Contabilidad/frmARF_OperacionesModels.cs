namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class ArfOperacionBusquedaDto
    {
        public int? operacion { get; set; }
        public string cod_local { get; set; } = string.Empty;
        public string unidad_desc { get; set; } = string.Empty;
        public int? cod_acreedor { get; set; }
        public string arrendatario_desc { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string estado_desc { get; set; } = string.Empty;
    }

    public class ArfOperacionRegistroDto
    {
        public int? operacion { get; set; }
        public string cod_local { get; set; } = string.Empty;
        public string unidad_desc { get; set; } = string.Empty;
        public int? cod_acreedor { get; set; }
        public string arrendatario_desc { get; set; } = string.Empty;
        public string cod_divisa { get; set; } = string.Empty;
        public string divisa_desc { get; set; } = string.Empty;
        public string periodicidad { get; set; } = string.Empty;
        public string periodicidad_desc { get; set; } = string.Empty;
        public decimal? cuota { get; set; }
        public short? plazo { get; set; }
        public decimal? tasa_descuento { get; set; }
        public decimal? tasa_interes { get; set; }
        public string incremento_tipo { get; set; } = string.Empty;
        public string incremento_tipo_desc { get; set; } = string.Empty;
        public decimal? incremento_valor { get; set; }
        public decimal? deposito_garantia_monto { get; set; }
        public bool deposito_garantia_ind { get; set; }
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_finaliza { get; set; }
        public string notas { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string estado_desc { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string activa_usuario { get; set; } = string.Empty;
        public DateTime? activa_fecha { get; set; }
    }

    public class ArfOperacionGuardarRequestDto
    {
        public int? operacion { get; set; }
        public int? cod_acreedor { get; set; }
        public string cod_local { get; set; } = string.Empty;
        public decimal? tasa_descuento { get; set; }
        public decimal? tasa_interes { get; set; }
        public string periodicidad { get; set; } = string.Empty;
        public decimal? cuota { get; set; }
        public short? plazo { get; set; }
        public DateTime fecha_inicio { get; set; }
        public DateTime fecha_finaliza { get; set; }
        public string notas { get; set; } = string.Empty;
        public decimal? deposito_garantia_monto { get; set; }
        public bool deposito_garantia_ind { get; set; }
        public string incremento_tipo { get; set; } = string.Empty;
        public decimal? incremento_valor { get; set; }
        public string cod_divisa { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    public class ArfOperacionActivarRequestDto
    {
        public int? operacion { get; set; }
        public string usuario { get; set; } = string.Empty;
    }

    public class ArfOperacionGuardarResponseDto
    {
        public int? operacion { get; set; }
        public string mensaje { get; set; } = string.Empty;
    }

    public class ArfOperacionPlanDto
    {
        public int? id_seq { get; set; }
        public int? periodo { get; set; }
        public DateTime? corte { get; set; }
        public decimal? valor_nominal { get; set; }
        public decimal? valor_presente { get; set; }
        public decimal? amortizacion { get; set; }
        public decimal? intereses { get; set; }
        public decimal? cuota { get; set; }
        public decimal? monto_pasivo { get; set; }
        public decimal? saldo_capital { get; set; }
        public decimal? recalculo { get; set; }
        public decimal? gasto_depreciacion { get; set; }
        public decimal? depreciacion_acumulada { get; set; }
        public decimal? valor_libros { get; set; }
        public string afecta_resultados { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string cxp_orden { get; set; } = string.Empty;
        public int? operacion { get; set; }
        public decimal? tasa { get; set; }
        public decimal? tasa_descuento { get; set; }
    }

    public class ArfOperacionCierreDto
    {
        public DateTime? corte { get; set; }
        public decimal? cuota { get; set; }
        public decimal? depreciacion_gasto { get; set; }
        public decimal? depreciacion_acum { get; set; }
        public decimal? valor_libros { get; set; }
        public decimal? pasivo { get; set; }
        public DateTime? pago_proximo { get; set; }
        public DateTime? corte_ultimo { get; set; }
    }

    public class ArfOperacionAsientoMainDto
    {
        public int? cod_contabilidad { get; set; }
        public string tipo_asiento { get; set; } = string.Empty;
        public string num_asiento { get; set; } = string.Empty;
        public DateTime? fecha { get; set; }
        public string referencia { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime? traslado_fecha { get; set; }
        public string traslado_usuario { get; set; } = string.Empty;
    }

    public class ArfOperacionAsientoDetalleDto
    {
        public int? id { get; set; }
        public string cod_cuenta { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal? monto_debito { get; set; }
        public decimal? monto_credito { get; set; }
        public string divisa { get; set; } = string.Empty;
        public decimal? tipo_cambio { get; set; }
        public string num_asiento { get; set; } = string.Empty;
        public string documento { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
        public string referencia { get; set; } = string.Empty;
    }

    public class ArfOperacionCambioDto
    {
        public int? id_cambio { get; set; }
        public int? operacion { get; set; }
        public string tipo_cambio { get; set; } = string.Empty;
        public string periodo_afecta { get; set; } = string.Empty;
        public decimal? v_anterior { get; set; }
        public decimal? v_actual { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
    }

    public class ArfOperacionCambioRequestDto
    {
        public int? operacion { get; set; }
        public string usuario { get; set; } = string.Empty;
        public decimal? tasa_descuento { get; set; }
        public decimal? tasa_interes { get; set; }
        public decimal? monto { get; set; }
        public short? plazo { get; set; }
        public string notas { get; set; } = string.Empty;
        public int? periodo { get; set; }
    }

    public class ArfOperacionFiniquitoPreviewDto
    {
        public int? periodo_mes { get; set; }
        public DateTime? periodo_corte { get; set; }
        public decimal? pasivo { get; set; }
        public decimal? valorlibros { get; set; }
        public DateTime? fecha { get; set; }
    }

    public class ArfOperacionFiniquitoRequestDto
    {
        public int? operacion { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public int? periodo { get; set; }
    }
}
