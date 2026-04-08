namespace Galileo_API.Models.ProGrX_Comites
{
    public class AfCdRemesaTesDto
    {
        public int Cod_Remesa { get; set; }
        public DateTime? Fecha { get; set; }
        public string? Usuario { get; set; }
        public DateTime? Fecha_Inicio { get; set; }
        public DateTime? Fecha_Corte { get; set; }
        public string? Notas { get; set; }
        public string? Estado { get; set; }
    }

    public class AfCdRemesaTesSaveDto
    {
        public int Cod_Remesa { get; set; } // 0 para insertar, >0 para actualizar
        public string Usuario { get; set; } = string.Empty;
        public DateTime? Fecha_Inicio { get; set; }
        public DateTime? Fecha_Corte { get; set; }
        public string? Notas { get; set; }
    }
}
