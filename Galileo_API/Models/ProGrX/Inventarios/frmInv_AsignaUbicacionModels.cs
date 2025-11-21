namespace Galileo.Models.INV
{
    public class AsignaUbicacionDto
    {
        public int cod_asignaubicacion { get; set; }
        public string cod_bodega { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string responsable { get; set; } = string.Empty;
        public string documento { get; set; } = string.Empty;
        public string cod_unidad { get; set; } = string.Empty;
        public DateTime fecha { get; set; }
        public string genera_user { get; set; } = string.Empty;
        public DateTime fecha_user { get; set; }
        public string autoriza_user { get; set; } = string.Empty;
        public DateTime autoriza_fecha { get; set; }
        public string procesa_user { get; set; } = string.Empty;
        public DateTime procesa_fecha { get; set; }
        public string descripcion_bodega { get; set; } = string.Empty;

    }

    public class AsignaUbicacionDetalleDto
    {
        public int cod_asignaubicacion { get; set; }
        public int linea { get; set; }
        public string cod_producto { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public float existencia { get; set; }
        public string ubicacion { get; set; } = string.Empty;
    }
}