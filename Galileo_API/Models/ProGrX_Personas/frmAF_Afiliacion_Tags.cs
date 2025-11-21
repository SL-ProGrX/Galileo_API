namespace Galileo.Models.ProGrX_Personas
{
    public class AfiAfiliacionControlDto
    {
        public int consec { get; set; }
        public string? cedula { get; set; }
        public string? nombre { get; set; }
        public DateTime fecha_ingreso { get; set; }
        public string? tipo_desc { get; set; }
        public string? promotor_desc { get; set; }
        public string? oficina_desc { get; set; }
        public DateTime? fecha { get; set; }
        public string? usuario { get; set; }
    }

    public class AfBoletasAfiliacion
    {
        public int consec { get; set; }
        public string? cedula { get; set; }
        public string? nombre { get; set; }
        public string? tipo_desc { get; set; }
    }

    public class AfiEtiquetaDto
    {
        public int id { get; set; }
        public string? tag_desc { get; set; }
        public string? fecha_etiqueta { get; set; }
        public string? usuario_etiqueta { get; set; }
        public string? observacion { get; set; }
        public string? tipo_desc { get; set; }
        public string? cedula { get; set; }
        public string? nombre { get; set; }
    }
}