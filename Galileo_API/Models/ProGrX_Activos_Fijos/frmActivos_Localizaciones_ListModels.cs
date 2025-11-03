namespace PgxAPI.Models.ProGrX_Activos_Fijos
{
    public class frmActivos_Localizaciones_ListModels
    {
        public class ActivosLocalizacionesLista
        {
            public int total { get; set; }
            public List<ActivosLocalizacionesData> lista { get; set; } = new List<ActivosLocalizacionesData>();
        }

        public class ActivosLocalizacionesData
        {
            public string cod_localiza { get; set; } = string.Empty;
            public string descripcion { get; set; } = string.Empty;
            public bool activo { get; set; } = false;
            public string registro_usuario { get; set; } = string.Empty;
            public string modifica_usuario { get; set; } = string.Empty;
            public string registro_fecha { get; set; } = string.Empty;
            public string modifica_fecha { get; set; } = string.Empty;
            public bool isNew { get; set; } = false;
        }
    }
}
