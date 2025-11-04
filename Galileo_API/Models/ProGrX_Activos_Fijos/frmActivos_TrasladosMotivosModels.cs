namespace PgxAPI.Models.ProGrX_Activos_Fijos
{
    public class ActivosTrasladosMotivosDataLista
    {
        public int total { get; set; }
        public List<ActivosTrasladosMotivosData> lista { get; set; } = new List<ActivosTrasladosMotivosData>();
    }
    public class ActivosTrasladosMotivosData
    {
        public string cod_motivo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool activo { get; set; }
        public bool isNew { get; set; } = false;
    }
}