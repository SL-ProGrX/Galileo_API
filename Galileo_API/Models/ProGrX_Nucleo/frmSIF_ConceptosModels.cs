namespace PgxAPI.Models.ProGrX_Nucleo
{
    public class SifConceptoLista
    {
        public int total { get; set; }
        public List<SifConceptoData> lista { get; set; } = new List<SifConceptoData>();
    }
    public class SifConceptoData
    {
        public string cod_concepto { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string movimiento_tipo { get; set; } = string.Empty;   // D = Debita, A = Acredita, I = Informativa
        public string nivel_acceso { get; set; } = string.Empty;      // U = Usuario, S = Sistema
        public bool activo { get; set; } = false;
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; }
        public bool isNew { get; set; } = false;

        // Relación con Documentos asignados
        public List<SifConceptoDocumentoData> documentos { get; set; } = new();
    }
    public class SifConceptoDocumentoData
    {
        public string tipo_documento { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool asignado { get; set; } = false;
    }
}
