namespace PgxAPI.Models.CxP
{
    public class CxPControlPagosParametros
    {
        public string? cboFecha { get; set; }
        public string fechaInicio { get; set; } = string.Empty;
        public string fechaCorte { get; set; } = string.Empty;
        public string tipo_Cancelacion { get; set; } = string.Empty;
        public bool cboProveedor { get; set; }
        public int? codProveedor { get; set; }
        public string? cboEstado { get; set; }
        public string? factura { get; set; }
        public string? documento { get; set; }
        public string? noSolicitud { get; set; }
    }

    public class ControlPagosData
    {
        public string npago { get; set; } = string.Empty;
        public string cod_proveedor { get; set; } = string.Empty;
        public DateTime fecha_vencimiento { get; set; }
        public string monto { get; set; } = string.Empty;
        public string proveedor { get; set; } = string.Empty;
        public string tesoreria { get; set; } = string.Empty;
        public string banco { get; set; } = string.Empty;
        public string nDocumento { get; set; } = string.Empty;
        public string cod_Factura { get; set; } = string.Empty;
    }

    public class ControlPagosResumenData
    {
        public string cod_proveedor { get; set; } = string.Empty;
        public string proveedor { get; set; } = string.Empty;
        public string pagos { get; set; } = string.Empty;
        public string monto { get; set; } = string.Empty;
    }

}
