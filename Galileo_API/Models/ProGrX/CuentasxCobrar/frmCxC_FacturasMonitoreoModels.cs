namespace Galileo_API.Models.ProGrX.CuentasxCobrar
{    
    public class CxCFacturasMonitoreoPersonaModel
    {
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
    }

    public class CxCFacturasMonitoreoFiltroDto
    {
        public string? Cod_Factura { get; set; }
        public string? Cliente_Id { get; set; }
        public string? Cliente_Nombre { get; set; }
        public string? Pagador_Id { get; set; }
        public string? Pagador_Nombre { get; set; }
        public List<string>? Contratos { get; set; }
        public List<string>? Conceptos { get; set; }
        public string? Tipo_Fecha { get; set; }
        public DateTime? Fecha_Inicio { get; set; }
        public DateTime? Fecha_Corte { get; set; }
        public string? Estado { get; set; }
        public required bool Adelantadas { get; set; } = false;
    }

    public class CxCFacturasMonitoreoItemDto
    {
        public string? Seleccion { get; set; }
        public long? Operacion { get; set; }
        public string? Cod_Factura { get; set; }
        public decimal? Monto { get; set; }
        public string? Factura_Estado_Desc { get; set; }
        public DateTime? Fecha_Emision { get; set; }
        public string? Cod_Divisa { get; set; }
        public decimal? Tipo_Cambio { get; set; }
        public decimal? Adelanto_Porc { get; set; }
        public decimal? Adelanto_Monto { get; set; }
        public decimal? Pendiente { get; set; }
        public string? Cliente { get; set; }
        public string? Pagador { get; set; }
        public DateTime? Cancela_Fecha { get; set; }
        public string? Remesa_I { get; set; }
        public string? Remesa_II { get; set; }
    }

    public class CxCFacturasMonitoreoDetalleRequestDto
    {
        public required int Operacion { get; set; }
        public string Factura { get; set; } = string.Empty;
        public string Consulta { get; set; } = "G";
    }

    public class CxCFacturasMonitoreoEstadoRequestDto
    {
        public required int Operacion { get; set; }
        public string Factura { get; set; } = string.Empty;
        public string Estado_Confirmacion { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
    }

    public class CxCFacturasMonitoreoEstadoProcesoDto
    {
        public string Factura_Estado { get; set; } = string.Empty;
    }
}
