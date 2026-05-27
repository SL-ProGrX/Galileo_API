namespace Galileo_API.Models.ProGrX.Credito
{
    public class CrCalculoOperacionResumenData
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string base_calculo { get; set; } = string.Empty;
        public string frecuencia_pago { get; set; } = "M";
        public int dias { get; set; } = 0;
        public int codigo_tipo { get; set; } = 0;
        public decimal monto_solicitado { get; set; } = 0;
        public decimal rango_maximo { get; set; } = 0;
        public string refunde { get; set; } = "N";
        public int operaciones_activas { get; set; } = 0;
    }

    public class CrCalculoOperacionGarantiaAhorroData
    {
        public decimal porcentaje_obrero { get; set; } = 0;
        public decimal porcentaje_patronal { get; set; } = 0;
        public decimal porcentaje_capitaliza { get; set; } = 0;
        public decimal aporte_obrero { get; set; } = 0;
        public decimal aporte_patronal { get; set; } = 0;
        public decimal capitalizacion { get; set; } = 0;
        public decimal disponible_bruto { get; set; } = 0;
    }

    public class CrCalculoOperacionRefundicionData
    {
        public long operacion { get; set; } = 0;
        public string codigo { get; set; } = string.Empty;
        public decimal saldo_real { get; set; } = 0;
        public string garantia_descripcion { get; set; } = string.Empty;
        public decimal mora_intc { get; set; } = 0;
        public decimal mora_intm { get; set; } = 0;
        public decimal mora_principal { get; set; } = 0;
        public decimal cuota { get; set; } = 0;
        public decimal recaudado { get; set; } = 0;
        public int plazo { get; set; } = 0;
        public decimal montoapr { get; set; } = 0;
        public string retencion { get; set; } = "N";
        public string poliza { get; set; } = "N";
        public string aceptarefun { get; set; } = "N";
        public string refunde_tipo { get; set; } = string.Empty;
        public decimal refunde_porc { get; set; } = 0;
        public decimal tiempo_transcurrido { get; set; } = 0;
    }

    public class CrCalculoOperacionCargoData
    {
        public string cod_cargo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public decimal valor { get; set; } = 0;
    }

    public class CrCalculoOperacionDisponibleData
    {
        public string garantia { get; set; } = string.Empty;
        public decimal monto { get; set; } = 0;
        public decimal saldo { get; set; } = 0;
        public decimal disponible { get; set; } = 0;
    }

    public class CrCalculoOperacionPantallaData
    {
        public CrCalculoOperacionResumenData resumen { get; set; } = new();
        public CrCalculoOperacionGarantiaAhorroData garantia_ahorro { get; set; } = new();
        public List<CrCalculoOperacionRefundicionData> refundiciones { get; set; } = new();
    }

    public class CrCalculoOperacionCodigoData
    {
        public CrCalculoOperacionResumenData resumen { get; set; } = new();
        public List<CrCalculoOperacionCargoData> cargos { get; set; } = new();
    }

    public class CrCalculoOperacionRangosData
    {
        public int plazo { get; set; } = 0;
        public decimal tasa { get; set; } = 0;
    }
}