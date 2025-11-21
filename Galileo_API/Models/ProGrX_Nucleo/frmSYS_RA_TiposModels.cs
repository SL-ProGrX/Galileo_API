namespace Galileo.Models.ProGrX_Nucleo
{
    public class SysRaTiposLista
    {
        public int total { get; set; }
        public List<SysRaTiposData> lista { get; set; } = new List<SysRaTiposData>();
    }

    public class SysRaTiposData
    {
        public string? tipo_id { get; set; }
        public string? descripcion { get; set; }
        public int activo { get; set; }
        public bool activob => activo == 1;
        public DateTime? registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public bool isNew { get; set; } = false;
    }
}