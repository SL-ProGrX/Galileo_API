namespace Galileo.Models.PRES
{
    public class RolesDto
    {
        public string cod_rol { get; set; } = string.Empty;
        public int? cod_contabilidad { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public bool activo { get; set; } = false;
        public bool control { get; set; } = false;
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
    }

    public class RolFiltro
    {
        public int pagina { get; set; }
        public int paginacion { get; set; }
        public string filtro { get; set; } = string.Empty;
    }

    public class RolesLista
    {
        public int total { get; set; }
        public List<RolesDto> lista { get; set; } = new List<RolesDto>();
    }

    public class MiembrosRolDto
    {
        public string cod_contabilidad { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string usuario_nombre { get; set; } = string.Empty;
        public bool? asignado { get; set; }
    }

    public class CuentaRolDto
    {
        public string cod_cuenta { get; set; } = string.Empty;
        public string cod_cuenta_mask { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string cod_divisa { get; set; } = string.Empty;
        public bool? acepta_movimientos { get; set; }
        public string user_registra { get; set; } = string.Empty;
    }

    public class UnidadesRolDto
    {
        public int? cod_contabilidad { get; set; }
        public string cod_unidad { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool? asignado { get; set; }
        public string user_registra { get; set; } = string.Empty;
    }

    public class UnidadesRolesDto
    {
        public int? cod_contabilidad { get; set; }
        public string cod_unidad { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool? asignado { get; set; }
    }

    public class CentroCosto
    {
        public int? cod_contabilidad { get; set; }
        public string? cod_unidad { get; set; }
        public string? cod_centro_costo { get; set; }
        public string? descripcion { get; set; }
        public bool? asignado { get; set; } // 0 o 1
        public string user_registra { get; set; } = string.Empty;
    }
}