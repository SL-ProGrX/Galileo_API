namespace PgxAPI.Models.ProGrX_Nucleo
{
    public class frmSYS_Estado_CivilModels
    {
        public class SysEstado_CivilLista
        {
            public int total { get; set; }
            public List<SysEstado_CivilData> lista { get; set; } = new List<SysEstado_CivilData>();
        }

        public class SysEstado_CivilData
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
