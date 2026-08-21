namespace Galileo_API.Models.ProGrX_Conciliacion
{
    public sealed class RastreoMovDocInicializaData
    {
        public DateTime? fecha_servidor { get; set; }
    }

    public sealed class RastreoMovDocConsultaRequest
    {
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_corte { get; set; }
        public int cantidad_lineas { get; set; } = 1000;
        public bool mostrar_todas_cuentas { get; set; } = true;
        public string cuenta_inicio { get; set; } = string.Empty;
        public string cuenta_corte { get; set; } = string.Empty;
    }

    public sealed class RastreoMovDocResumenData
    {
        public int total { get; set; } = 0;
        public string cod_cuenta_mask { get; set; } = string.Empty;
        public string cuenta_desc { get; set; } = string.Empty;
        public decimal debito { get; set; } = 0;
        public decimal credito { get; set; } = 0;
        public string ubicacion { get; set; } = string.Empty;
        public string cod_unidad { get; set; } = string.Empty;
        public string cod_centro_costo { get; set; } = string.Empty;
        public string cod_divisa { get; set; } = string.Empty;
        public decimal tipo_cambio { get; set; } = 0;
    }

    public sealed class RastreoMovDocDetalleData
    {
        public DateTime? registro_fecha { get; set; }
        public string tipo_documento { get; set; } = string.Empty;
        public string cod_transaccion { get; set; } = string.Empty;
        public string cod_cuenta { get; set; } = string.Empty;
        public decimal debito { get; set; } = 0;
        public decimal credito { get; set; } = 0;
        public string concepto_desc { get; set; } = string.Empty;
        public string cliente { get; set; } = string.Empty;
        public string documento { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
        public string cod_unidad { get; set; } = string.Empty;
        public string cod_centro_costo { get; set; } = string.Empty;
        public string cod_divisa { get; set; } = string.Empty;
        public decimal tipo_cambio { get; set; } = 0;
        public string cod_oficina { get; set; } = string.Empty;
        public string referencia_01 { get; set; } = string.Empty;
        public string referencia_02 { get; set; } = string.Empty;
        public string referencia_03 { get; set; } = string.Empty;
    }
}