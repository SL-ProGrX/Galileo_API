namespace PgxAPI.Models.ProGrX_Activos_Fijos
{
    public class ActivosPolizasTiposLista
    {
        public int total { get; set; }
        public List<ActivosPolizasTiposData> lista { get; set; } = new List<ActivosPolizasTiposData>();
    }

    public class ActivosPolizasTiposData
    {
        public string tipo_poliza { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool activo { get; set; } = false;
        public string registro_usuario { get; set; } = string.Empty;
        public string modifica_usuario { get; set; } = string.Empty;
        public string registro_fecha { get; set; } = string.Empty;
        public string modifica_fecha { get; set; } = string.Empty;
        public bool isNew { get; set; } = false;
    }
}