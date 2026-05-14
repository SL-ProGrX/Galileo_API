namespace Galileo.Models.ProGrX.Fondos
{
    public class FndRendimientoRequestDto
    {
        public required int operadora { get; set; }
        public required string plan { get; set; }
        public required string usuario { get; set; }
        public required DateTime fecha_corte { get; set; }
        public required decimal tasa { get; set; }
        public required decimal tcp { get; set; }
        public string? aplicacion { get; set; }
    }

    public class FndRendimientoResultadoDto
    {
        public decimal rendimiento { get; set; }
        public int casos { get; set; }
        public int procesados { get; set; }
        public int pendientes { get; set; }
    }

    public class FndPlanDatosDto
    {
        public required string descripcion { get; set; }
        public required DateTime rend_corte { get; set; }
        public required decimal ult_tasa { get; set; }
        public required decimal tasa_base { get; set; }

        public bool utiliza_tasa_fluctuante { get; set; }
        public bool utiliza_tbp { get; set; }

        public decimal tbp { get; set; }
        public decimal tcp { get; set; }

        // Campos adicionales que Angular normalmente ocupa
        public int cod_operadora { get; set; }
        public required string cod_plan { get; set; }

        public DateTime fecha_server { get; set; }     // si lo devuelve el SP
    }
}