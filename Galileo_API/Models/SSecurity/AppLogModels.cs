namespace PgxAPI.Models
{
    public class AppLog
    {
        public string Hit_Cod { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int Hits { get; set; }
        public string Fecha_Inicio { get; set; } = string.Empty;
        public DateTime Fecha_Corte { get; set; }
    }

    public class AppLogRequest
    {
        public string Fecha_Inicio { get; set; } = string.Empty;
        public string Fecha_Corte { get; set; } = string.Empty;
        public int Empresa { get; set; }
    }

    public class ErrorAppLogDTO
    {
        public int Code { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
