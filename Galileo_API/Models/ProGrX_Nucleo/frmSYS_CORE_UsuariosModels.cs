namespace PgxAPI.Models.SIF
{

    public class coreUsuariosLista
    {
        public int total { get; set; }
        public List<coreUsuariosData> lista { get; set; } = new List<coreUsuariosData>();
    }

    public class coreUsuariosData
    {
        public string? core_usuario { get; set; }
        public string? nombre { get; set; }
        public string? usuario_ref { get; set; }
        public string? email { get; set; }
        public string? tel_movil { get; set; }
        public bool activo { get; set; }
        public string? registro_usuario { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string? modificacion_usuario { get; set; }
        public DateTime? modificacion_fecha { get; set; }
        public string? notas { get; set; }
    }

    public class coreUsuarioFiltros
    {
        public int? pagina { get; set; }
        public int? paginacion { get; set; }
        public string? filtro { get; set; }
    }

    public class coreMiembrosData
    {
        public string cod_unidad { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool activa { get; set; }
        public string? registro_usuario { get; set; }
        public DateTime? registro_fecha { get; set; }
        public bool? asignado { get; set; }
    }

    public class coreMiembrosRolData
    {
        public string cod_unidad { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool activa { get; set; }
        public string? registro_usuario { get; set; }
        public DateTime? registro_fecha { get; set; }
        public bool rol_solicita { get; set; }
        public bool rol_consulta { get; set; }
        public bool rol_autoriza { get; set; }
        public bool rol_encargado { get; set; }
        public bool rol_lider { get; set; }
    }

    public class CoreMiembro
    {
        public int codEmpresa { get; set; }
        public string uen { get; set; } = string.Empty;
        public string core_usuario { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public bool mov { get; set; }
    }

    public class CoreMiembroRol
    {
        public int codEmpresa { get; set; }
        public string uen { get; set; } = string.Empty;
        public string core_usuario { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public bool rol_solicita { get; set; }
        public bool rol_consulta { get; set; }
        public bool rol_autoriza { get; set; }
        public bool rol_encargado { get; set; }
        public bool rol_lider { get; set; }
    }
}
