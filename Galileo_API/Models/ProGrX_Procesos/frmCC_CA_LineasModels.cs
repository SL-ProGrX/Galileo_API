namespace Galileo.Models.GEN
{
    public class CcCaLineasActivasData
    {
        public string ItmX { get; set; } = string.Empty;
    }

    public class CcCaCodigosAsignadosData
    {
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public int existe { get; set; }
    }

    public class PrmCaLineasDtInsert
    {
        public string cod_linea { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; }
    }

    public class PrmCaLineasData
    {
        public string cod_linea { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string cod_plan { get; set; } = string.Empty;
        public bool activo { get; set; }
    }

    public class PrmCaLineaUpsert
    {
        public string cod_linea { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string cod_plan { get; set; } = string.Empty;
        public bool activo { get; set; }
        public DateTime registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
    }
}