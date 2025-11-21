namespace Galileo.Models.SYS
{
    public class SysNacionalidadesLista
    {
        public int total { get; set; }
        public List<SysNacionalidadesData> lista { get; set; } = new List<SysNacionalidadesData>();
    }

    public class SysNacionalidadesData
    {
        public string cod_nacionalidad { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string cod_inter { get; set; } = string.Empty;
        public bool omision { get; set; } = false;
        public bool activo { get; set; } = false;
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public bool isNew { get; set; } = false;
    }
}