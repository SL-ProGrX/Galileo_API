namespace Galileo.Models.ProGrX.Fondos
{
    public class FndSeguridadRango
    {
        public decimal mAutoInicio { get; set; }
        public decimal mAutoCorte { get; set; }
        public bool mAutorizacion { get; set; }
    }

    public class FndRetLiqRebajosData
    {
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string documento { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
        public decimal monto { get; set; }
    }

    public class FndRetLiqConsultaData
    {
        public decimal aportes { get; set; }
        public decimal rendimiento { get; set; }
        public int cod_banco { get; set; }
        public string tipo_Pago { get; set; } = string.Empty;
        public string cuenta_ahorros { get; set; } = string.Empty;
        public DateTime fecha_Inicio { get; set; }
        public int plazo { get; set; }
        public string? plazo_Tipo { get; set; }
        public DateTime fecha_Corte { get; set; }
        public decimal rend_Pendiente { get; set; }
        public decimal multa { get; set; }
        public decimal saldoEnGarantia { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public int giroTerceros { get; set; }
        public bool permiteLiquidar { get; set; } = false;
        public string tipo_Documento { get; set; } = string.Empty;
    }

    public class FndRetLiqRentaGlobalData
    {
        public bool RG_Aplica { get; set; } 
        public decimal RG_Porcentaje { get; set; }
        public decimal Retiro_Acumulado { get; set; } 
        public decimal Retiro_Monto { get; set; }
        public decimal RG_MntNoGravable { get; set; }
        public decimal Retiro_Gravable { get; set; }
        public decimal ISR_MONTO { get; set; }
        public string? Cedula { get; set; }
    }

    public class FiltrosRetLiqAplicar
    {
        public string? TipoDocumento { get; set; }
        public string? Proceso { get; set; }
        public decimal MontoAplicar { get; set; }
        public decimal Rebajos { get; set; }
        public int Operadora { get; set; }
        public string? Plan { get; set; }
        public int Contrato { get; set; }
        public string? Tipo { get; set; }
        public string? Usuario { get; set; }
        public string? Cedula { get; set; }
        public string? Notas { get; set; }
        public decimal TipoCambio { get; set; }
        public int BancoId { get; set; }
        public string? CuentaBancaria { get; set; }
        public string? RetCodigo { get; set; }
        public bool chkPagoTercero { get; set; }
        public string? PTTipo { get; set; }
        public string? PTId { get; set; }
        public string? PTNombre { get; set; }
        public List<FndRetLiqRebajosData>? RebajosLista { get; set; }
    }

    public class FndRetLiqProcesoData
    {
        public int liq_Num { get; set; }
        public decimal montoGiro { get; set; }
        public int tesoreria { get; set; }
    }
}