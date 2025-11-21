namespace Galileo.Models.ProGrX_Nucleo
{
    public class FrmSysOrigenRecursosModels
    {
        public class SysOrigenRecursosLista
        {
            public int total { get; set; }
            public List<SysOrigenRecursosData> lista { get; set; } = new List<SysOrigenRecursosData>();
        }

        public class SysOrigenRecursosData
        {
            public string cod_origen_recursos { get; set; } = string.Empty;
            public string descripcion { get; set; } = string.Empty;
            public bool activa { get; set; } = false;
            public DateTime? registro_fecha { get; set; }
            public string registro_usuario { get; set; } = string.Empty;
            public DateTime? actualiza_fecha { get; set; }
            public string actualiza_usuario { get; set; } = string.Empty;
            public bool isNew { get; set; } = false;
        }
    }
}