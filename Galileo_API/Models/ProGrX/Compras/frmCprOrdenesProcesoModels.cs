namespace PgxAPI.Models.CPR
{
    public class ProveedorOrdenesData
    {
        public int cod_proveedor { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public string cod_orden { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime cotiza_fecha { get; set; }
        public string cotiza_usuario { get; set; } = string.Empty;
        public DateTime adjudica_fecha { get; set; }
        public string adjudica_usuario { get; set; } = string.Empty;
        public string? notas { get; set; }
    }

    public class ProveedorPinData
    {
        public string estado { get; set; } = string.Empty;
        public string proceso { get; set; } = string.Empty;
        public int cod_proveedor { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public int pin_autorizacion { get; set; }
        public string causa_desc { get; set; } = string.Empty;
    }

    public class OrdenProceso
    {
        public string cod_orden { get; set; } = string.Empty;
        public string genera_user { get; set; } = string.Empty;
        public string funcion { get; set; } = string.Empty;
        public string cod_proveedor { get; set; } = string.Empty;
    }

}
