namespace Galileo.Models.ProGrX.Cajas
{
    public class CajasDivisaDto
    {
        public required string  cod_divisa { get; set; }
        public decimal efectivo { get; set; }
        public decimal documentos { get; set; }
    }

    public class CajaAperturaDetalleDto
    {
        public int cod_apertura { get; set; }
        public required string estado { get; set; } 
        public DateTime? apertura_fecha { get; set; }
        public required string apertura_usuario { get; set; }
        public DateTime? en_uso_fecha { get; set; }
        public required string en_uso_usuario { get; set; }
        public DateTime? apertura_vence { get; set; }
        public bool apertura_compartida { get; set; }
    }

    public class CajasAperturaTeConsultaData
    {
        public int traslado_id { get; set; }
        public required string cod_divisa { get; set; }
        public decimal importe { get; set; }
        public required string cod_caja { get; set; }
        public required string registro_usuario { get; set; }
        public int cod_apertura { get; set; }
        public DateTime? registro_fecha { get; set; }
        public decimal? tipo_cambio { get; set; }
        public decimal? monto { get; set; }
        public string? notas { get; set; }
    }

    public class CajaAperturaRequestDto
    {
        public required string codCaja { get; set; }
        public required string usuario { get; set; } 
        public required string clave { get; set; }
        public List<CajasDivisaDto> saldosIniciales { get; set; } = [];
        public List<CajasAperturaTeConsultaData> trasladosAprovisionamientos { get; set; } = [];
    }

    public class CajaAperturaResponseDto
    {
        public int codApertura { get; set; }
        public required string codCaja { get; set; }
        public required string codCuentaConta { get; set; }
    }
}