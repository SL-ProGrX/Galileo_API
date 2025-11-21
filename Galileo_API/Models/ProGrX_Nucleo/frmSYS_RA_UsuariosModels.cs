namespace Galileo.Models.ProGrX_Nucleo
{
    public class SysUsuariosData
    {
        public string usuario { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime activa_fecha { get; set; }
        public string activa_usuario { get; set; } = string.Empty;
        public DateTime inactiva_fecha { get; set; }
        public string inactiva_usuario { get; set; } = string.Empty;
        public int activo { get; set; }
        public bool activob => activo == 1;
    }
}