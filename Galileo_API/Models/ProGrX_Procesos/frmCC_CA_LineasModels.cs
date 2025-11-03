namespace PgxAPI.Models.GEN
{
    public class CC_CA_Lineas_ActivasData
    {
        public string ItmX { get; set; } = string.Empty;
    }

    public class CC_CA_CodigosAsignadosData
    {
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public int existe { get; set; }
    }

    public class PRM_CA_Lineas_DtInsert
    {
        public string cod_linea { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; }
    }

    public class PRM_CA_LineasData
    {
        public string cod_linea { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string cod_plan { get; set; } = string.Empty;
        public bool activo { get; set; }
    }

    public class PRM_CA_LineaUpsert
    {
        public string cod_linea { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string cod_plan { get; set; } = string.Empty;
        public bool activo { get; set; }
        public DateTime registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
    }
}
