namespace Galileo.Models.FSL
{
    public class FslGarantiasData
    {
        public string item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }
    
    public class FslDevolucionesDataLista
    {
        public int Total { get; set; }
        public List<FslDevolucionesData> devoluciones { get; set; } = new List<FslDevolucionesData>();
    }

    public class FslDevolucionesData
    {
        public int cod_devolucion { get; set; }
        public Nullable<DateTime> fecha_inicio { get; set; }
        public Nullable<DateTime> fecha_corte { get; set; }
        public string garantia { get; set; } = string.Empty;
        public string _base { get; set; } = string.Empty;
        public float porcentaje { get; set; }
        public Nullable<DateTime> registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
    }
}