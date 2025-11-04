namespace PgxAPI.Models.AF
{
    public class AfiBeneTarjetasDataLista
    {
        public int Total { get; set; }
        public List<AfiBeneTarjetasData> Tarjetas { get; set; } = new List<AfiBeneTarjetasData>();
    }

    public class AfiBeneTarjetasData
    {
        public int id_tr { get; set; }
        public string cod_producto { get; set; } = string.Empty;
        public string cod_beneficio { get; set; } = string.Empty;
        public int consec { get; set; }
        public int id_beneficio { get; set; }
        public string cedula { get; set; } = string.Empty;
        public float monto { get; set; }
        public Nullable<DateTime> registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public Nullable<DateTime> activa_fecha { get; set; }
        public string? activa_usuario { get; set; } = string.Empty;
        public string? no_tarjeta { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string? beneficio_desc { get; set; } = string.Empty;
        public int? id_pago { get; set; }
    }

    public class AfiTarjetasFiltros
    {
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_corte { get; set; }
        public int? pagina { get; set; }
        public int? paginacion { get; set; }
        public string? vfiltro { get; set; } = string.Empty;
    }

    public class AfiBeneTarjetasRecargaData
    {
        public long cod_remesa_tr { get; set; }
        public string usuario { get; set; } = string.Empty;
        public List<AfiBeneTarjetasData> tarjetas { get; set; } = new List<AfiBeneTarjetasData>();
    }

    public class AfiBeneProveedorData
    {
        public int cod_proveedor { get; set; }
        public string tipo_pago { get; set; } = string.Empty;
        public string cuenta { get; set; } = string.Empty;
        public int cod_banco { get; set; }
        public string cedjur { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class AfiBeneTarjetasRemesasDataLista 
    {
        public int Total { get; set; }
        public List<AfiBeneTarjetasRemesasData> Beneficios { get; set; } = new List<AfiBeneTarjetasRemesasData>();
    }

    public class AfiBeneTarjetasRemesasData
    {
        public long cod_remesa_tr { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; }
        public DateTime fecha_inicio { get; set; }
        public DateTime fecha_corte { get; set; }
        public string notas { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public int cantidad { get; set; }
        public float monto { get; set; }
        public string? cod_producto { get; set; } = string.Empty;
        public string? cod_producto_inv { get; set; } = string.Empty;
        public int? cod_proveedor { get; set; }
        public string? nombre_proveedor { get; set; } = string.Empty;
    }

    public class DocArchivoBeneRecargaTarjetaDto
    {
        public int codCliente { get; set; }
        public int cod_remesa_tr { get; set; }
        public int cod_proveedor { get; set; }
        public string? body { get; set; }
        public string? usuario { get; set; }
        public List<FileTarjetasDto>? archivos { get; set; }
    }

    public class FileTarjetasDto
    {
        public int? size { get; set; } 
        public string? filename { get; set; } = string.Empty;
        public string? filetype { get; set; } = string.Empty;
        public byte[]? filecontent { get; set; }
    }
}