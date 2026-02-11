namespace Galileo_API.Models.ProGrX_Activos
{
    public class ARFMonitorFiltroDto
    {
        public string? tipo_fecha { get; set; }   // Activación | Cierre | Registro | Inicio | Finaliza
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_corte { get; set; }
        public string? cod_unidad { get; set; }
        public string? cod_arrendador { get; set; }
        public string? corte { get; set; }
    }

    public class ARFMonitorTablaDto
    {
        public int operacion { get; set; }
        public string arrendatario_desc { get; set; } = "";
        public string unidad_desc { get; set; } = "";
        public string estado_desc { get; set; } = "";
        public string divisa_desc { get; set; } = "";
        public decimal cuota { get; set; }
        public string periodicidad_desc { get; set; } = "";
        public int plazo { get; set; }
        public DateTime fecha_inicio { get; set; }
        public DateTime fecha_finaliza { get; set; }
        public DateTime? pago_proximo { get; set; }
        public DateTime? corte_ultimo { get; set; }
        public decimal depreciacion_acum { get; set; }
        public decimal valor_libros { get; set; }
        public decimal pasivo { get; set; }
        public decimal derecho_uso { get; set; }
        public decimal depreciacion_gasto { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public DateTime? activa_fecha { get; set; }
        public string? activa_usuario { get; set; }
        public string? incremento_tipo_desc { get; set; }
        public decimal? incremento_valor { get; set; }
        public decimal? tasa_descuento { get; set; }
        public decimal? tasa_interes { get; set; }
        public decimal? pago_transito { get; set; }
        public decimal? deposito_garantia_monto { get; set; }
    }
}
