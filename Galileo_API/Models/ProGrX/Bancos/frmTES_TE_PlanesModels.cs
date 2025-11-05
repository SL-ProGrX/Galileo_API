namespace PgxAPI.Models.TES
{
    public class TesBancoPlanesData
    {
        public int id_banco { get; set; }
        public string cod_plan { get; set; } = string.Empty;
        public int numero_te { get; set; }
        public int numero_interno { get; set; }
        public DateTime registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
    }

    public class TesBancosGruposData
    {
        public int id_banco { get; set; }
        public string cod_grupo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string desc_corta { get; set; } = string.Empty;
        public string banco_desc { get; set; } = string.Empty;
        public string banco_desc_corta { get; set; } = string.Empty;
    }
}