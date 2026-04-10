namespace Galileo_API.Models.ProGrX.CuentasxCobrar
{
    public class CxCCuentasSgtRebajoCrdPantallaDto
    {
        public int operacion { get; set; }
        public string cedula { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public decimal ingresosTotales { get; set; }
        public decimal rebajosTotales { get; set; }
        public decimal disponible { get; set; }
        public List<CxCCuentaRebajoCrdDto> creditosDeudor { get; set; } = [];
        public List<CxCCuentaRebajoCrdDto> movimientosRegistrados { get; set; } = [];
    }

    public class CxCCuentaRebajoCrdDto
    {
        public int? operacion { get; set; } = 0;
        public int? id_Solicitud { get; set; } = 0;
        public string codigo { get; set; } = string.Empty;
        public string garantiaX { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal saldo { get; set; } = 0;
        public decimal int_Cor { get; set; } = 0;
        public decimal int_Mor { get; set; } = 0;
        public decimal principal { get; set; } = 0;
        public decimal cargos { get; set; } = 0;
        public decimal poliza { get; set; } = 0;
        public decimal monto { get; set; } = 0;
        public int cta_Pendientes { get; set; } = 0;
    }

    public class CxCCuentasSgtRebajoCrdGuardarDto
    {
        public int Operacion { get; set; } = 0;
        public int Id_Solicitud { get; set; } = 0;
        public decimal Monto { get; set; } = 0;
        public decimal Saldo { get; set; } = 0;
        public decimal Principal { get; set; } = 0;
        public decimal Int_Cor { get; set; } = 0;
        public decimal Int_Mor { get; set; } = 0;
        public decimal Cargos { get; set; } = 0;
        public decimal Poliza { get; set; } = 0;
        public int Cta_Pendientes { get; set; } = 0;
    }

    public class CxCCuentasSgtRebajoCrdEliminarDto
    {
        public int Operacion { get; set; } = 0;
        public int Id_Solicitud { get; set; } = 0;
    }

    public class CxCCuentasSgtRebajoCrdActualizarDto
    {
        public int Operacion { get; set; } = 0;
        public int Cta_Pendientes { get; set; } = 0;
    }
}
