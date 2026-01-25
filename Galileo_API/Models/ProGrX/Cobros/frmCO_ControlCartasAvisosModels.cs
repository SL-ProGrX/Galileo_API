namespace Galileo.Models.ProGrX.Cobros
{
    public class CoControlCartasAvisosLista
    {
        public List<CoControlCartasAvisosData> lista { get; set; } = new();
        public int total { get; set; } = 0;
    }

    public class CoControlCartasAvisosData
    {
        public DateTime fecha_asignacion { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public bool mantener { get; set; }
        public decimal mora { get; set; }
        public int cuotamora { get; set; }
        public bool selecctionado { get; set; } = false;
    }
}