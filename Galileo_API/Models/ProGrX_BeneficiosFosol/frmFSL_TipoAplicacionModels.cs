namespace PgxAPI.Models.FSL
{
    public class TiposCausaData
    {
        public string cod_causa { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string montoBase { get; set; } = string.Empty;
        public string tipoTabla { get; set; } = string.Empty;
        public bool activa { get; set; }
    }

    public class ListaPlanesData
    {
        public string cod_plan { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public bool activo { get; set; }
    }

    public class CausasDataLista
    {
        public int total { get; set; }
        public List<TiposCausaData> causas { get; set; } = new List<TiposCausaData>();
    }

    public class PlanDataInsert
    {
        public string cod_plan { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string tipo_desembolso { get; set; } = string.Empty;
        public bool activo { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
    }

    public class CausaDataInsert
    {
        public string cod_causa { get; set; } = string.Empty;
        public string cod_plan { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string monto_base { get; set; } = string.Empty;
        public string tipo_tabla { get; set; } = string.Empty;
        public bool activa { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
    }

    public class PlanesDataLista
    {
        public int total { get; set; }
        public List<ListaPlanesData> planes { get; set; } = new List<ListaPlanesData>();
    }

}
