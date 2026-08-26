namespace Galileo_API.Models.ProGrX.CuentasxCobrar
{
    public class CxCPersonaDto
    {
        public string? cedula { get; set; }
        public string? nombre { get; set; }
        public string? categoria_desc { get; set; }
        public int? ind_mensajes { get; set; }
        public int? facturas { get; set; }
        public decimal? facturas_total { get; set; }
    }

    public class CxCPersonasF4FiltroDto
    {
        public required int pagina { get; set; }
        public required int paginacion { get; set; }
        public string? filtro { get; set; }
        public string? sortField { get; set; }
        public required int sortOrder { get; set; }
    }

    public class CxCPersonasF4ListaDto
    {
        public int total { get; set; }
        public List<CxCPersonaDto> lista { get; set; } = [];
    }

    public class CxCCuentaDto
    {
        public long? operacion { get; set; }
        public string? cod_concepto { get; set; }
        public string? concepto_desc { get; set; }
        public DateTime? activa_fecha { get; set; }
        public string? activa_usuario { get; set; }
        public string? num_documento { get; set; }
        public DateTime? fecha_pago { get; set; }
        public decimal? monto { get; set; }
        public decimal? saldo { get; set; }
        public decimal? cuota { get; set; }
        public DateTime? fecha_ultmov { get; set; }
        public string? estado { get; set; }
        public string? nombre_pagador { get; set; }
        public string? cedula_pagador { get; set; }
        public string? cod_contrato { get; set; }
        public string? contrato_desc { get; set; }
        public string? oficinadesc { get; set; }

        public int? mora_dias { get; set; }
        public decimal? mora_monto { get; set; }
        public decimal? mora_int { get; set; }
        public decimal? mora_cargos { get; set; }
        public decimal? mora_principal { get; set; }
        public DateTime? mora_fecha { get; set; }

        public int? warning { get; set; }
        public DateTime? fecha_server { get; set; }
    }

    public class CxCSolicitudDto
    {
        public long? operacion { get; set; }
        public string? cod_concepto { get; set; }
        public string? linea_desc { get; set; }
        public string? cedula { get; set; }
        public DateTime? fecha_sol { get; set; }
        public decimal? monto_sol { get; set; }
        public string? estado_sol { get; set; }
        public string? usuario { get; set; }
    }

    public class CxCPreAnalisisDto
    {
        public long? cod_preanalisis { get; set; }
        public string? tipo { get; set; }
        public string? cod_linea { get; set; }
        public decimal? monto { get; set; }
        public string? estado { get; set; }
        public long? operacion { get; set; }
        public DateTime? fecha_creacion { get; set; }
        public string? usuario { get; set; }
    }

    public class CxCIncobrableDto
    {
        public long? operacion { get; set; }
        public string? cod_concepto { get; set; }
        public decimal? saldo { get; set; }
        public decimal? int_cor { get; set; }
        public decimal? int_mor { get; set; }
        public decimal? cargos { get; set; }
        public decimal? poliza { get; set; }
        public string? estado { get; set; }
        public string? registro_usuario { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string? genera_documento { get; set; }
        public int? cod_incobrable { get; set; }
        public string? notas_registro { get; set; }
    }

    public class CxCFacturaDto
    {
        public long? operacion { get; set; }
        public string? cod_factura { get; set; }
        public DateTime? fecha_pago { get; set; }
        public decimal? monto { get; set; }
        public decimal? adelanto_monto { get; set; }
        public decimal? liberado { get; set; }
        public string? pagador_nombre { get; set; }
        public string? factura_estado_desc { get; set; }
    }

    public class CxCFacturaFiltroDto
    {
        public string? cedula { get; set; }
        public string? cod_factura { get; set; }
        public long? operacion { get; set; }
        public string? tipo_fecha { get; set; }
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_corte { get; set; }
        public string? estado { get; set; }
    }

    public class CxCDesembolsoDto
    {
        public long? operacion { get; set; }
        public decimal? monto { get; set; }
        public string? estado { get; set; }
        public DateTime? fecha_emision { get; set; }
        public string? tipo { get; set; }
        public int? id_giro { get; set; }
        public long? tesoreria_solicitud { get; set; }
        public long? tesoreria_remesa { get; set; }
        public string? banco_desc { get; set; }
        public string? beneficiario { get; set; }
        public string? ndocumento { get; set; }
    }

    public class CxCMensajeDto
    {
        public DateTime? vencimiento { get; set; }
        public string? mensaje { get; set; }
        public DateTime? fecha { get; set; }
        public string? usuario { get; set; }
    }

    public class CxCMensajeAddDto
    {
        public string? cedula { get; set; }
        public string? usuario { get; set; }
        public DateTime? vencimiento { get; set; }
        public string? mensaje { get; set; }
    }

    public class CxCMensajeDeleteDto
    {
        public string? cedula { get; set; }
        public string? usuario { get; set; }
        public DateTime? vencimiento { get; set; }
        public string? mensaje_prefijo { get; set; }
    }

    public class CxCDesembolsoFacturaDto
    {
        public int operacion { get; set; }
        public string? cod_factura { get; set; }
        public decimal monto { get; set; }
        public decimal adelanto_monto { get; set; }
        public decimal liberado { get; set; }
        public string? cod_divisa { get; set; }
        public decimal tipo_cambio { get; set; }
        public int operacion_origen { get; set; }
    }

    public class CxCFacturaEstadoDto
    {
        public string value { get; set; } = string.Empty;
        public string label { get; set; } = string.Empty;
    }

}
