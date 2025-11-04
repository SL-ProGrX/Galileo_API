namespace PgxAPI.Models.INV
{
    public class ConsultaMovimientoBodegaCDdto
    {
        public string Cod_Bodega { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    public class MovimientosDtoList
    {
        public int Total { get; set; }
        public List<MovimientosDto> Movimientos { get; set; } = new List<MovimientosDto>();
    }

    public class MovimientosDto
    {
        public DateTime fecha { get; set; }
        public string producto { get; set; } = string.Empty;
        public string tipox { get; set; } = string.Empty;
        public string origen { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public int existencia { get; set; } = 0;
        public int cantidad { get; set; } = 0;
        public int existenciax { get; set; } = 0;
        public int precio { get; set; } = 0;
        public int totalsinimp { get; set; } = 0;
        public int impventas { get; set; } = 0;
        public int impconsumo { get; set; } = 0;
        public int totalconimp { get; set; } = 0;
        public string bodega { get; set; } = string.Empty;
        public string bodegaenlace { get; set; } = string.Empty;
    }

    public class MovimientosInventariosFiltros
    {
        public string? beneficio_id { get; set; }
        public string? beneficiario_nombre { get; set; }
        public string? solicita_id { get; set; }
        public string? solicita_nombre { get; set; }
        public string? estado_persona { get; set; }
        public string? institucion { get; set; }
        public string? usuario_registra { get; set; }
        public string? usuario_autoriza { get; set; }
        public string? unidad { get; set; }
        public string? oficina { get; set; }
        public string? estado { get; set; }
        public string? fecha { get; set; }
        public string? fecha_inicio { get; set; }
        public string? fecha_corte { get; set; }
        public string? Bodega { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string? cod_Producto { get; set; }
        public string? cod_Bodega { get; set; }
        public string? vfiltro { get; set; }
        public int? pagina { get; set; }
        public int? paginacion { get; set; }
    }
}