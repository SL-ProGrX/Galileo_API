namespace PgxAPI.Models.SYS
{
    public class Core_UENs_Filtros
    {
        public int? pagina { get; set; }
        public int? paginacion { get; set; }
        public string? filtro { get; set; }
    }
    public class Core_UENs_DTOList
    {
        public int Total { get; set; }
        public List<Core_UENs_DTO> uens { get; set; } = new List<Core_UENs_DTO>();
    }

    public class Core_UENs_DTO
    {
        public string cod_unidad { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string cntx_unidad { get; set; } = string.Empty;
        public string? cntx_centro_costo { get; set; } = string.Empty;
        public string? unidad_principal { get; set; } = string.Empty;    
        public bool activa { get; set; }
        public bool btn { get; set; }
    }

    public class Core_Usuarios_DTO
    {
        public string core_usuario { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string usuario_ref { get; set; } = string.Empty;
        public bool activo { get; set; }
        public DateTime registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public bool asignado { get; set; }
    }

    public class Core_Roles_DTO
    {
        public string core_usuario { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string usuario_ref { get; set; } = string.Empty;
        public bool activo { get; set; }
        public DateTime registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public bool rol_solicita { get; set; }
        public bool rol_consulta { get; set; }
        public bool rol_autoriza { get; set; }
        public bool rol_encargado { get; set; }

        public bool rol_lider { get; set; }
    }

    public class Unidades_DTOList
    {
        public int Total { get; set; }
        public List<Unidades_DTO> unidades { get; set; } = new List<Unidades_DTO>();
    }

    public class Unidades_DTO
    {
        public string unidad { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class CentroCosto_DTOList
    {
        public int Total { get; set; }
        public List<CentroCosto_DTO> centrocostos { get; set; } = new List<CentroCosto_DTO>();
    }

    public class CentroCosto_DTO
    {
        public string centrocosto { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class UensListaDatos
    {
        public string item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string cntX_Centro_Costo { get; set; } = string.Empty;
        public string cntX_Unidad { get; set; } = string.Empty;
    }
}
