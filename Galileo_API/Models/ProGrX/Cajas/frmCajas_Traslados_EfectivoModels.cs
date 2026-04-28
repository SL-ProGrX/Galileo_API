namespace Galileo.Models.ProGrX.Cajas
{
    public class CajasTrasladosEfectivoFiltros
    {
        public required string cod_caja { get; set; }
        public string origen_destino { get; set; } = "D";
        public string movimiento { get; set; } = "";
        public string estado { get; set; } = "P";
        public DateTime fecha_inicio { get; set; }
        public DateTime fecha_corte { get; set; }
        public bool sin_fechas { get; set; } = true;
    }

    public class CajasTrasladosEfectivoDto
    {
        public required int traslado_id { get; set; }
        public required string cod_caja { get; set; }
        public required string registro_usuario { get; set; }
        public required int cod_apertura { get; set; }
        public DateTime? registro_fecha { get; set; }
        public required string cod_divisa { get; set; }
        public decimal? importe { get; set; }
        public decimal? tipo_cambio { get; set; }
        public decimal? monto { get; set; }
        public string? notas { get; set; }

        public string? tipo_descripcion { get; set; }
        public string? estado_descripcion { get; set; }
        public string? d_cod_caja { get; set; }
        public string? estado_usuario { get; set; }
        public string? d_cod_apertura { get; set; }
        public string? estado_fecha { get; set; }
    }

    public class CajasTeResolucionRequest
    {
        public required int apertura_id { get; set; }
        public required string cod_caja { get; set; }
        public required string caja_usuario { get; set; }
        public required string usuario { get; set; } 
        public required string resolucion { get; set; } 
        public List<CajasTrasladosEfectivoDto> lista { get; set; } = new();
    }
}