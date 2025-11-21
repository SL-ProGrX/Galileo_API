namespace Galileo.Models.ProGrX_Nucleo
{
    public class FrmSysEstadoCivilModels
    {
        public class SysEstadoCivilLista
        {
            public int total { get; set; }
            public List<SysEstadoCivilData> lista { get; set; } = new List<SysEstadoCivilData>();
        }

        public class SysEstadoCivilData
        {
            public string cod_estado_civil { get; set; } = string.Empty;
            public string descripcion { get; set; } = string.Empty;
            public bool activo { get; set; } = false;
            public DateTime? registro_fecha { get; set; }
            public string registro_usuario { get; set; } = string.Empty;
            public bool isNew { get; set; } = false;
        }
    }
}