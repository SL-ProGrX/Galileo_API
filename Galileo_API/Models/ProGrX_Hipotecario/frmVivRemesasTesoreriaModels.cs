namespace Galileo_API.Models.ProGrX_Hipotecario
{
    public class RemesasTesoreriaObtenerDto
    {
        public int Remesa { get; set; }
        public string? RegistroUsuario { get; set; }
        public DateTime? RegistroFecha { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaCorte { get; set; }
        public string? Notas { get; set; }
        public string? Estado { get; set; }
        public int Casos { get; set; }
        public decimal Monto { get; set; }
    }
}
