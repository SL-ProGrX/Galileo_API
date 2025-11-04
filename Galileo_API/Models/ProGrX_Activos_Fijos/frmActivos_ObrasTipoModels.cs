namespace PgxAPI.Models.ProGrX_Activos_Fijos
{
    public class ActivosObrasTipoDesemDataLista
    {
        public int total { get; set; }
        public List<ActivosObrasTipoDesemData> lista { get; set; } = new List<ActivosObrasTipoDesemData>();
    }

    public class ActivosObrasTipoDesemData
    {
        public string cod_desembolso { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool activo { get; set; }
        public bool isNew { get; set; } = false;
    }

    public class ActivosObrasTipoDataLista
    {
        public int total { get; set; }
        public List<ActivosObrasTipoData> lista { get; set; } = new List<ActivosObrasTipoData>();
    }
    
    public class ActivosObrasTipoData
    {
        public string cod_tipo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool activo { get; set; }
        public bool isNew { get; set; } = false;
    }
}