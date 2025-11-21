namespace Galileo.Models.PRES
{
    public class PresPeriodoRequest
    {
        public int Inicio_Anio { get; set; }
        public int Inicio_Mes { get; set; }
        public int Corte_Anio { get; set; }
        public int Corte_Mes { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}