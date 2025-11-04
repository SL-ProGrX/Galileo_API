namespace PgxAPI.Models.ProGrX_Nucleo
{
    public class SysRaCasosData
    {
        public int persona_id { get; set; }
        public string tipo_id { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string estadodesc { get; set; } = string.Empty;
        public string tipodesc { get; set; } = string.Empty;
        public DateTime? fecha_vence { get; set; }
        public string notas { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;

    }

    public class SysCasosFiltroData
    {
        public int persona_id { get; set; }
        public string? cedula { get; set; } = string.Empty;
        public string? nombre { get; set; } = string.Empty;
        public string? estado { get; set; } = string.Empty;
        public string? tipo { get; set; } = string.Empty;
        public bool vence { get; set; }
        public string? inicioVenc { get; set; } = string.Empty;
        public string? finVenc { get; set; } = string.Empty;
    }

    public class SysCasosAutorizacionesData
    {
        public int autorizacion_id { get; set; }
        public int horas { get; set; }
        public DateTime? registro_fecha { get; set; }
        public DateTime? fecha_vence { get; set; }
        public string usuario_autorizado { get; set; } = string.Empty;
        public string usuario_autorizador { get; set; } = string.Empty;
    }
    
    public class SysCasosAccesosData
    {
        public int id_exp_acceso { get; set; }
        public int autorizacion_id { get; set; }
        public DateTime? registro_fecha { get; set; }     
        public string usuario_autorizado { get; set; } = string.Empty;
        public string usuario_autorizador { get; set; } = string.Empty;
    }
}