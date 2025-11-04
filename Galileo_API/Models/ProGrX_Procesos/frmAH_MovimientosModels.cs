namespace PgxAPI.Models.AH
{
    public class MovimientosPatrimonioDto
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string cod_institucion { get; set; } = string.Empty;
        public string institucion { get; set; } = string.Empty;
        public DateTime fecha { get; set; }
        public string monto { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string cod_caja { get; set; } = string.Empty;
        public string cod_concepto { get; set; } = string.Empty;
        public decimal fechaproc { get; set; }
        public string tcon { get; set; } = string.Empty;
        public string ncon { get; set; } = string.Empty;
        public string tipo_aporte { get; set; } = string.Empty;
        public string tipo_aporte_id { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string concepto { get; set; } = string.Empty;
        public string cod_oficina { get; set; } = string.Empty;
        public string oficina { get; set; } = string.Empty;
        public string fecha_emision { get; set; } = string.Empty;
        public string id_seq { get; set; } = string.Empty;
        public string sectordesc { get; set; } = string.Empty;
    }

    public class DocumentosTransaccionSifDto
    {
        public string idx { get; set; } = string.Empty;
        public string itmx { get; set; } = string.Empty;
    }

    public class MovimientosPatrimonioFiltros
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
        public string documento { get; set; } = string.Empty;
        public string? cedula { get; set; }
    }
}