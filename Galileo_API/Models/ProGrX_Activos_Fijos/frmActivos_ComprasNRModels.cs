namespace Galileo.Models.ProGrX_Activos_Fijos
{
    public class ActivosComprasPendientesRegistroData
    {
        public string cod_factura { get; set; } = string.Empty;
        public int linea { get; set; }
        public int cod_proveedor { get; set; }
        public string cod_producto { get; set; } = string.Empty;
        public decimal precio { get; set; }
        public decimal cantidad { get; set; }
        public string proveedor { get; set; } = string.Empty;
        public string producto { get; set; } = string.Empty;
        public DateTime fecha { get; set; }
        public int registrados { get; set; }
        public string bodega { get; set; } = string.Empty;
        public decimal pendientes { get; set; }
    }
}