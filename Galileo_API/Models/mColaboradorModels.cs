namespace PgxAPI.Models
{
    public class RH_BoletaDTO
    {
        public string Nomina { get; set; } = string.Empty;
        public long? NominaId { get; set; }
        public string? EmpleadoId { get; set; } = string.Empty;
        public long? PeriodoId { get; set; }
    }
}
