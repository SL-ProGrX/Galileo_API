namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrSeguimientoRetencionesInicializarRequest
    {
        public int operacion { get; set; } = 0;
        public DateTime? fecha_desembolso { get; set; }
        public decimal? pri_deduc { get; set; }
        public int? dia_pago { get; set; }
    }

    public class CrSeguimientoRetencionesPantallaData
    {
        public decimal disponible { get; set; } = 0;
        public bool editable { get; set; } = true;
        public List<CrSeguimientoRetencionesOperacionData> operaciones { get; set; } = new();
        public List<CrSeguimientoRetencionesRefundicionData> refundiciones { get; set; } = new();
    }

    public class CrSeguimientoRetencionesOperacionData
    {
        public int operacion { get; set; } = 0;
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal saldo { get; set; } = 0;
        public decimal mora { get; set; } = 0;
        public decimal cargos { get; set; } = 0;
        public decimal iva { get; set; } = 0;
    }

    public class CrSeguimientoRetencionesRefundicionData
    {
        public int operacion { get; set; } = 0;
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal saldo { get; set; } = 0;
        public decimal mora { get; set; } = 0;
        public decimal cargos { get; set; } = 0;
        public decimal iva { get; set; } = 0;
    }

    public class CrSeguimientoRetencionesGuardarRequest
    {
        public int operacion_base { get; set; } = 0;
        public string codigo_base { get; set; } = string.Empty;
        public int operacion_refunde { get; set; } = 0;
        public string codigo_refunde { get; set; } = string.Empty;
        public decimal saldo { get; set; } = 0;
        public decimal amortizacion { get; set; } = 0;
        public decimal cargos { get; set; } = 0;
        public decimal iva { get; set; } = 0;
        public decimal saldo_original { get; set; } = 0;
        public DateTime? fecha_desembolso { get; set; }
        public decimal? pri_deduc { get; set; }
        public int? dia_pago { get; set; }
    }

    public class CrSeguimientoRetencionesEliminarRequest
    {
        public int operacion_base { get; set; } = 0;
        public int operacion_refunde { get; set; } = 0;
    }

    internal sealed class CrSeguimientoRetencionesOperacionBaseData
    {
        public string primer_cuota { get; set; } = string.Empty;
        public string garantia { get; set; } = string.Empty;
        public decimal montoapr { get; set; } = 0;
        public decimal cuota { get; set; } = 0;
        public decimal int_credito { get; set; } = 0;
        public string convenio { get; set; } = string.Empty;
        public string cod_destino { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public DateTime? fecha_desembolso { get; set; }
        public decimal? pri_deduc { get; set; }
        public int? dia_pago { get; set; }
    }

    internal sealed class CrSeguimientoRetencionesOperacionRow
    {
        public int id_solicitud { get; set; } = 0;
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal amortiza { get; set; } = 0;
        public decimal cuota { get; set; } = 0;
        public int plazo { get; set; } = 0;
        public decimal mora { get; set; } = 0;
        public decimal cargos { get; set; } = 0;
        public decimal iva { get; set; } = 0;
    }

    internal sealed class CrSeguimientoRetencionesRefundicionRow
    {
        public int id_solicitud { get; set; } = 0;
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal monto { get; set; } = 0;
        public decimal mora { get; set; } = 0;
        public decimal cargosdef { get; set; } = 0;
        public decimal iva { get; set; } = 0;
    }
}