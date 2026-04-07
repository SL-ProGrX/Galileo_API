namespace Galileo_API.Models.ProGrX.CuentasxCobrar
{
    public class CxCCuentasSGTRebajosInternosPantallaDto
    {
        public int operacion { get; set; }
        public string cedula { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public decimal ingresosTotales { get; set; }
        public decimal rebajosTotales { get; set; }
        public decimal disponible { get; set; }
        public List<CxCCuentaRebajoInternoDto> cuentasDeudor { get; set; } = [];
        public List<CxCCuentaRebajoInternoDto> movimientosRegistrados { get; set; } = [];
    }

    public class CxCCuentaRebajoInternoDto
    {
        public int? operacion { get; set; } = 0;
        public int? operacion_Aplicada { get; set; } = 0;
        public string cod_Concepto { get; set; } = string.Empty;
        public string cod_Contrato { get; set; } = string.Empty;
        public string conceptoDesc { get; set; } = string.Empty;
        public decimal saldo { get; set; } = 0;
        public decimal int_Cor { get; set; } = 0;
        public decimal int_Mor { get; set; } = 0;
        public decimal cargos { get; set; } = 0;
        public decimal principal { get; set; } = 0;
        public int dias { get; set; } = 0;
        public int dias_Mora { get; set; } = 0;
        public string num_Documento { get; set; } = string.Empty;
        public string contratoDesc { get; set; } = string.Empty;
        public decimal monto { get; set; } = 0;
    }
}
