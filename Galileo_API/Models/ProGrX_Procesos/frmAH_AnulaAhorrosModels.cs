namespace Galileo.Models.AH
{
    public class FrmAhAnulaAhorrosConsultaResponse
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal obrero { get; set; } = 0;
        public decimal patronal { get; set; } = 0;
        public decimal custodia { get; set; } = 0;
        public decimal capitaliza { get; set; } = 0;
        public string cod_divisa { get; set; } = string.Empty;
        public decimal total { get; set; } = 0;
        public DateTime? fec_ahorro { get; set; } 
        public DateTime? fec_aporte { get; set; }
        public DateTime? fec_custodia { get; set; }
        public DateTime? fec_capitaliza { get; set; }
    }

    public class FrmAhAnulaAhorrosMovimientoResponse
    {
        public string documento_key { get; set; } = string.Empty;
        public DateTime? fecha { get; set; }
        public string fecha_proceso { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal monto { get; set; } = 0;
        public string tcon { get; set; } = string.Empty;
        public string ncon { get; set; } = string.Empty;
        public string cod_concepto { get; set; } = string.Empty;
    }

    public class FrmAhAnulaAhorrosMovimientoSeleccionadoRequest
    {
        public string tcon { get; set; } = string.Empty;
        public string ncon { get; set; } = string.Empty;
        public decimal monto { get; set; } = 0;
    }

    public class FrmAhAnulaAhorrosProcesarRequest
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string tipo_anulacion { get; set; } = "MON";
        public string tipo_rubro { get; set; } = "O";
        public string accion { get; set; } = "C";
        public decimal monto { get; set; } = 0;
        public string notas { get; set; } = string.Empty;
        public List<FrmAhAnulaAhorrosMovimientoSeleccionadoRequest> movimientos { get; set; } = [];
    }

    public class FrmAhAnulaAhorrosProcesarResponse
    {
        public string tipo_documento { get; set; } = string.Empty;
        public string num_documento { get; set; } = string.Empty;
        public decimal monto_aplicado { get; set; } = 0;
        public string mensaje { get; set; } = string.Empty;
        public string? reporte_resultado { get; set; }
    }
}
