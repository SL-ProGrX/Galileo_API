namespace Galileo.Models.ProGrX_Activos_Fijos
{
    public class ActivosPeriodosDataLista
    {
        public int total { get; set; }
        public List<ActivosFijosPeriodosData> lista { get; set; } = new List<ActivosFijosPeriodosData>();

    }

    public class ActivosFijosPeriodosData
    { 
        public int mes { get; set; }        
        public int anio { get; set; }
        public DateTime periodocorte { get; set; }
        public string periodo  { get; set; } = string.Empty;
    }

}