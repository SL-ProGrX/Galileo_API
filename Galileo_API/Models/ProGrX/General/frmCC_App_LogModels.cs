namespace PgxAPI.Models.GEN
{
    public class EstadisticaData
    {
        public string hit_cod { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public int hits { get; set; }
        public DateTime fecha_inicio { get; set; }
        public DateTime fecha_corte { get; set; }
    }

    public class Estadistica_DetalleData
    {
        public string cliente_id { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string app_name { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public DateTime fecha { get; set; }
    }

    public class Estadistica_AnalisisData
    {
        public string cliente_id { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public int ingresos { get; set; }
        public string app_name { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public DateTime fecha { get; set; }
        public string tipo { get; set; } = string.Empty;
    }
}
