namespace Galileo.Models.GEN
{
    public class CcCaLineasLista
    {
        public int total { get; set; }
        public List<CcCaLineasData> lista { get; set; } = new List<CcCaLineasData>();
    }
    public class CcCaLineasData
    {
        public string cod_linea { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string cod_plan { get; set; } = string.Empty;
        public required bool activo { get; set; }
        public required bool isNew { get; set; }
    }


    public class CcCaCatalogoLineasData
    {
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string existe { get; set; } = string.Empty;
        public bool activo => existe != "-1";
    }

    public class CcCaAsignacionGuardarRequest
    {
        public required int codEmpresa { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string tipoOrigen { get; set; } = string.Empty;
        public string codigoOrigen { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public required bool activo { get; set; }
    }


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
