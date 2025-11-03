namespace PgxAPI.Models.PRES
{
    public class Pres_PeriodoRequest
    {
        public int Inicio_Anio { get; set; }
        public int Inicio_Mes { get; set; }
        public int Corte_Anio { get; set; }
        public int Corte_Mes { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}
