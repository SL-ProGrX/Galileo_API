namespace PgxAPI.Models.INV
{
    public class InvControlActivosLista
    {
        public int total { get; set; } = 0;
        public List<InvControlActivosDto> lista { get; set; } = new List<InvControlActivosDto>();
    }

    public class InvControlActivosDto
    {
        public int id_control { get; set; }
        public string cod_producto { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public float costo_total { get; set; }
        public float costo_unitario { get; set; }
        public string? factura { get; set; }
        public string cod_compra { get; set; } = string.Empty;
        public Nullable<DateTime> fecha_compra { get; set; }
        public int cod_proveedor { get; set; }
        public string? cod_bodega { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string? numero_placa { get; set; } = string.Empty;
        public string? cod_localizacion { get; set; } = string.Empty;
        public string? marca { get; set; } = string.Empty;
        public string? modelo { get; set; } = string.Empty;
        public string? serie { get; set; } = string.Empty;
        public string? observaciones { get; set; } = string.Empty;
        public string? cod_uen { get; set; } = string.Empty;
        public string? id_responsable { get; set; } = string.Empty;
        public string? cod_requesicion { get; set; } = string.Empty;
        public string? entrega_usuario { get; set; } = string.Empty;
        public Nullable<DateTime> entrega_fecha { get; set; }
        public string? activo_usuario { get; set; } = string.Empty;
        public Nullable<DateTime> activo_fecha { get; set; }
        public string? registro_usuario { get; set; } = string.Empty;
        public Nullable<DateTime> registro_fecha { get; set; }

        public string? departamento { get; set; } = string.Empty;
        public string? seccion { get; set; } = string.Empty;
    }

    public class InvControlActivosFiltros
    {
        public int pagina { get; set; } = 0;
        public int paginacion { get; set; } = 30;
        public string? filtro { get; set; }
    }

    public class InvCntrActvivosCombos
    {
        public string item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class InvDatosActivos
    {
        public string? met_depreciacion { get; set; }
        public int vida_util { get; set; }
        public string? tipo_vida_util { get; set; }
    }
}