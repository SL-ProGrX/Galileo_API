namespace Galileo_API.Models.ProGrX_EstudioCrd
{
    public class PrendaGastoDto
    {
        public int Id_Param { get; set; }
        public decimal Monto_Min { get; set; }
        public decimal Monto_Max { get; set; }
        public decimal Gastos { get; set; }
        public decimal Honorarios { get; set; }
        public decimal Total { get; set; }
        public int Asigna { get; set; }
    }

    public class PreaAsignaHonorariosPrenRequestDto
    {
        public string Preanalisis { get; set; } = string.Empty;
        public int? IdParam { get; set; }
        public string Proceso { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
    }

    public class PreaAsignaHonorariosPrenResultDto
    {
        public string Resultado { get; set; } = string.Empty;
    }
}
