namespace PgxAPI.Models.ProGrX_Nucleo
{
    public class frmSYS_Origen_RecursosModels
    {
        public class SysOrigen_RecursosLista
        {
            public int total { get; set; }
            public List<SysOrigen_RecursosData> lista { get; set; } = new List<SysOrigen_RecursosData>();
        }

        public class SysOrigen_RecursosData
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
