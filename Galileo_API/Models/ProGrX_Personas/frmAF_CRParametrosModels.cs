namespace PgxAPI.Models.ProGrX_Personas
{
    public class AF_CRParametrosData
    {
        public int id { get; set; }
        public int dias_vence { get; set; }
        public bool liq_pat_control { get; set; }
        public DateTime fecha_limite { get; set; }
        public string tipo_vencimiento { get; set; } = string.Empty;
        public bool utiliza_zonas { get; set; }
        public bool activar_control { get; set; }
        public bool isNew { get; set; } = false;
    }
}
