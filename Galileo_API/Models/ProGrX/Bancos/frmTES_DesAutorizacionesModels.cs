namespace PgxAPI.Models.TES
{
    public class TesDesAutorizacionesFiltros
    {
        public int id_banco { get; set; }
        public string tipo_doc { get; set; } = string.Empty;
        public bool duplicados { get; set; }
        public bool todas_fechas { get; set; }
        public DateTime fecha_inicio { get; set; }
        public DateTime fecha_corte { get; set; }
        public bool todas_solicitudes { get; set; }
        public int? solicitud_inicio { get; set; }
        public int? solicitud_corte { get; set; }
        public bool casos_bloqueados { get; set; }
        public int tipo_autorizacion { get; set; }
        public decimal monto_inicio { get; set; }
        public decimal monto_fin { get; set; }
        public string detalle { get; set; } = string.Empty;
        public string appid { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }
}
