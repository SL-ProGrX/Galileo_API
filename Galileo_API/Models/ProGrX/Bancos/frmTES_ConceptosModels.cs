namespace PgxAPI.Models.ProGrX.Bancos
{
    public class TesConceptosLista
    {
        public int total { get; set; }
        public List<TesConceptosData> lista { get; set; } = new List<TesConceptosData>();
    }

    public class TesConceptosData
    {
        public string cod_concepto { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool activo { get; set; } = false;
        public string cod_cuenta_mask { get; set; } = string.Empty;
        public bool auto_registro { get; set; } = false;
        public bool dp_tramite_apl { get; set; } = false;
    }
}