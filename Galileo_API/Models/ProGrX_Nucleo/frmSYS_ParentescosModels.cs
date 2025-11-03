namespace PgxAPI.Models.ProGrX_Nucleo
{
    public class SysParentescosLista
    {
        public int total { get; set; }
        public List<SysParentescosData> lista { get; set; } = new List<SysParentescosData>();
    }

    public class SysParentescosData
    {
        public string cod_parentesco { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool activo { get; set; } = false;
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; }
        public bool isNew { get; set; } = false;
    }

}
