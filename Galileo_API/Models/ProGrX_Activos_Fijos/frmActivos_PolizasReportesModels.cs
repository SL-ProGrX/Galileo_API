namespace Galileo.Models.ProGrX_Activos_Fijos
{
    public class ActivosPolizasReportesLista
    {
        public int total { get; set; }
        public List<ActivosPolizasReportesData> lista { get; set; } = new();
    }

    public class ActivosPolizasReportesData
    {
        public string cod_poliza { get; set; } = string.Empty;
        public string tipo_poliza { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string? observacion { get; set; }
        public string fecha_inicio { get; set; } = string.Empty;
        public string fecha_vence { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public string? num_poliza { get; set; }
        public string? documento { get; set; }
        public string estado { get; set; } = string.Empty;
        public string? tipo_poliza_desc { get; set; }
        public string? registro_usuario { get; set; }
        public string? registro_fecha { get; set; }
        public string? modifica_usuario { get; set; }
        public string? modifica_fecha { get; set; }

        public bool isNew { get; set; } = false;
    }

}