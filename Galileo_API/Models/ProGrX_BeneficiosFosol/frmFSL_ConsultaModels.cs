namespace PgxAPI.Models.FSL
{
    public class FslConsultaListas
    {
        public string item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class fslConsultaFiltros
    {
        public string cod_plan { get; set; } = string.Empty;
        public List<FslConsultaListas> cod_causa { get; set; } = new List<FslConsultaListas>();
        public List<FslConsultaListas> cod_enfermedad { get; set; } = new List<FslConsultaListas>();
        public string estado { get; set; } = string.Empty;
        public string cod_buscarPor { get; set; } = string.Empty;
        public string texto_buscar { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string expediente { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string cod_tipo { get; set; } = string.Empty;
        public string cod_comite { get; set; } = string.Empty;
        public string resueltoMiembro { get; set; } = string.Empty;
        public string apelacionRegistrada { get; set; } = string.Empty;
        public string gestionRegistrada { get; set; } = string.Empty;
        public string estadoPersona { get; set; } = string.Empty;
        public bool adicionales { get; set; }
        //fecha de inicio menos 30 dias de la fecha actual
        public bool fechas { get; set; }
        public DateTime fecha_inicio { get; set; }
        public DateTime fecha_corte { get; set; }
    }

    public class FslConsultaExpedienteDatos
    {
        public long cod_expediente { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public int edad { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; }
        public string estado_desc { get; set; } = string.Empty;
        public string plan_desc { get; set; } = string.Empty;
        public string causa_desc { get; set; } = string.Empty;
        public string enfermedad_desc { get; set; } = string.Empty;
        public string comite_desc { get; set; } = string.Empty;
        public DateTime resolucion_fecha { get; set; }
        public float total_disponible { get; set; }
        public float total_aplicado { get; set; }
        public float total_sobrante { get; set; }
        public string presenta_cedula { get; set; } = string.Empty;
        public string presenta_nombre { get; set; } = string.Empty;

    }
}
