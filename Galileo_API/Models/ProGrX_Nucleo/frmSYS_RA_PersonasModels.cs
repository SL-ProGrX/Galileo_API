namespace PgxAPI.Models.ProGrX_Nucleo
{
    public class SysRaExpedientesData
    {
        public int persona_id { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string estadodesc { get; set; } = string.Empty;
        public string tipo_id { get; set; } = string.Empty;
        public string tipodesc { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public DateTime? vence_fix { get; set; }
        public string notas { get; set; } = string.Empty;
        public bool vence { get; set; }
        public DateTime? vencimiento { get; set; }
    }
    
    public class SysExpedienteFiltroData
    {
        public string? cedula { get; set; } = string.Empty;
        public string? nombre { get; set; } = string.Empty;
        public string? estado { get; set; } = string.Empty;
        public bool vence { get; set; }
        public string? inicioVenc { get; set; } = string.Empty;
        public string? finVenc { get; set; } = string.Empty;
    }
}