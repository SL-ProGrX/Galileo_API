namespace PgxAPI.Models.AH
{
    public class AutorizacionesPatrimonioDto
    {
        public string id_autorizacion { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string monto_calculado { get; set; } = string.Empty;
        public string monto_solicitado { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public DateTime resuelve_fecha { get; set; }
        public string resuelve_usuario { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime aplica_fecha { get; set; }
        public string aplica_usuario { get; set; } = string.Empty;
        public string tcon { get; set; } = string.Empty;
        public string ncon { get; set; } = string.Empty;
    }

    public class FiltrosAutorizacionesPatrimonioDto
    {
        public string cedula { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public DateTime fecha_inicio { get; set; }
        public DateTime fecha_corte { get; set; }
    }

    public class PatGestionesPatrimonio
    {
        public string id_autorizacion { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string monto_calculado { get; set; } = string.Empty;
        public string monto_solicitado { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public DateTime resuelve_fecha { get; set; }
        public string resuelve_usuario { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime aplica_fecha { get; set; }
        public string aplica_usuario { get; set; } = string.Empty;
        public string tcon { get; set; } = string.Empty;
        public string ncon { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string estado_desc { get; set; } = string.Empty;
        public string tipo_desc { get; set; } = string.Empty;
        public string monto_dif { get; set; } = string.Empty;
    }
}
