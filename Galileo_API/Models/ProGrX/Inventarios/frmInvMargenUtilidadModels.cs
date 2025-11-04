namespace PgxAPI.Models.INV
{
    public class LineaDto
    {
        public int cod_prodclas { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }

    public class SubLineaDto
    {
        public int cod_linea_sub { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }

    public class PrecioDto
    {
        public string? cod_precio { get; set; } 
        public string? descripcion { get; set; }
        public bool? cambio_margen { get; set; }
        public int? cod_linea { get; set; }
        public int? cod_sublinea { get; set; }
        public bool? seleccionado { get; set; }
        public int? monto { get; set; }

    }
}