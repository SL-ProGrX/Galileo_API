namespace Galileo_API.Models.ProGrX.CuentasPorCobrar
{

    public class CxCFacturaEstadosLista
    {
        public int total { get; set; }
        public List<CxCFacturaEstadosData> lista { get; set; } = new List<CxCFacturaEstadosData>();
    }
    public class CxCFacturaEstadosData
    {
        public string? Factura_Estado { get; set; } = string.Empty;
        public string? Descripcion { get; set; } = string.Empty;
        public string? Proceso { get; set; } = string.Empty;
        public string? Accion { get; set; } = string.Empty; 
        public bool? Activo { get; set; }
        public bool? IsNew { get; set; }
    }
}
