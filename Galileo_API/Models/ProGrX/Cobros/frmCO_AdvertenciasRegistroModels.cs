namespace Galileo.Models.ProGrX.Cobros
{
    public class CoAdvertenciasRegistroLista
    {
        public List<CoAdvertenciasRegistroData> lista { get; set; } = new();
        public int total { get; set; } = 0;
    }

    public class CoAdvertenciasRegistroData
    {
        public string cod_advertencia { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public required int linea { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public DateTime? fecha_vence { get; set; }
        public string resolucion_notas { get; set; } = string.Empty;
        public DateTime? resolucion_fecha { get; set; }
        public string resolucion_usuario { get; set; } = string.Empty;
        public string advertenciad_desc { get; set; } = string.Empty;
    }
    public class CoAdvertenciasRegistroSociosData
    {
        public string? cedula_colilla { get; set; }
        public string? cedula_real { get; set; }
        public string? nombre { get; set; }
    }
}