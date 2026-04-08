namespace Galileo_API.Models.ProGrX.CuentasxCobrar
{
    public class CxCCuentasSgtRebajosInternosPantallaDto
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

    public class CxCCuentasSgtRebajosInternosGuardarDto
    {
        public int Operacion { get; set; } = 0;
        public int Operacion_Aplicada { get; set; } = 0;
        public decimal Monto { get; set; } = 0;
        public decimal Saldo { get; set; } = 0;
        public decimal Principal { get; set; } = 0;
        public decimal Int_Cor { get; set; } = 0;
        public decimal Int_Mor { get; set; } = 0;
        public decimal Cargos { get; set; } = 0;
        public int Dias { get; set; } = 0;
        public int Dias_Mora { get; set; } = 0;
        public bool AplicarCargoReposicion { get; set; } = false;
    }

    public class CxCCuentasSgtRebajosInternosEliminarDto
    {
        public int Operacion { get; set; } = 0;
        public int Operacion_Aplicada { get; set; } = 0;
    }
}
