namespace PgxAPI.Models.ProGrX_Nucleo
{
    public class SysEducacionLista
    {
        public int total { get; set; }
        public List<SysEducacionData> lista { get; set; } = new List<SysEducacionData>();
    }

    public class SysEducacionData
    {
        public string cod_educ { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public int activa { get; set; }
        public bool activab => activa == 1;
        public bool isNew { get; set; } = false;
    }

    public class SysEducacionDetalleData
    {
        public string cod_educ { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public int asignado { get; set; }
        public bool asignadob => asignado == 1;
     

    }

}
