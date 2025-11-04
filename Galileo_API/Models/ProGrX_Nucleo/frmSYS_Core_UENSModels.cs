namespace PgxAPI.Models.SYS
{
    public class CoreUeNsFiltros
    {
        public int? pagina { get; set; }
        public int? paginacion { get; set; }
        public string? filtro { get; set; }
    }
    
    public class CoreUeNsDtoList
    {
        public int Total { get; set; }
        public List<CoreUeNsDto> uens { get; set; } = new List<CoreUeNsDto>();
    }

    public class CoreUeNsDto
    {
        public string cod_unidad { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string cntx_unidad { get; set; } = string.Empty;
        public string? cntx_centro_costo { get; set; } = string.Empty;
        public string? unidad_principal { get; set; } = string.Empty;
        public bool activa { get; set; }
        public bool btn { get; set; }
    }

    public class CoreUsuariosDto
    {
        public string core_usuario { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string usuario_ref { get; set; } = string.Empty;
        public bool activo { get; set; }
        public DateTime registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public bool asignado { get; set; }
    }

    public class CoreRolesDto
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

    public class UnidadesDtoList
    {
        public int Total { get; set; }
        public List<UnidadesDto> unidades { get; set; } = new List<UnidadesDto>();
    }

    public class UnidadesDto
    {
        public string unidad { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class CentroCostoDtoList
    {
        public int Total { get; set; }
        public List<CentroCostoDto> centrocostos { get; set; } = new List<CentroCostoDto>();
    }

    public class CentroCostoDto
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