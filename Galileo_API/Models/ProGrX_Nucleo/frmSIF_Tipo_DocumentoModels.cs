namespace PgxAPI.Models.ProGrX_Nucleo
{
    public class SifTipoDocumentoData
    {

        public string tipo_documento { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string consecutivo { get; set; } = string.Empty;
        public string? tipo_comprobante { get; set; }
        public string? tipo_movimiento { get; set; }
        public string tipo_asiento { get; set; } = string.Empty;
        public string cod_cuenta { get; set; } = string.Empty;
        public int activo { get; set; }
        public bool activob => activo == 1;
        public int asiento_transaccion { get; set; }
        public bool asiento_transaccionb => asiento_transaccion == 1;
        public string asiento_mascara { get; set; } = string.Empty;
        public int asiento_modulo { get; set; }
        public bool asiento_modulob => asiento_modulo == 1;
        public string? formato_salida { get; set; }
        public int impuesto_registra { get; set; }
        public bool impuesto_registrab => impuesto_registra == 1;
        public decimal impuesto_porcentaje { get; set; }
        public string? impuesto_cod_cuenta { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public string archivo_per { get; set; } = string.Empty;
        public int permite_reversion { get; set; }
        public bool permite_reversionb => permite_reversion == 1;
        public int aplica_cierre_especial { get; set; }
        public bool aplica_cierre_especialb => aplica_cierre_especial == 1;
        public int reversion_dias_autorizados { get; set; }
        public string tipo_asiento_desc { get; set; } = string.Empty;
        public string cuenta_mask { get; set; } = string.Empty;
        public string cuenta_desc { get; set; } = string.Empty;
        public string imp_cuenta_mask { get; set; } = string.Empty;
        public string imp_cuenta_desc { get; set; } = string.Empty;
        public string tipo_comprobante_desc { get; set; } = string.Empty;
        public string tipo_movimiento_desc { get; set; } = string.Empty;
    }

    public class SifTipoDocConceptoData
    {
        public string cod_concepto { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string? asignado { get; set; }
        public bool asignadob => !string.IsNullOrEmpty(asignado);
    }
}