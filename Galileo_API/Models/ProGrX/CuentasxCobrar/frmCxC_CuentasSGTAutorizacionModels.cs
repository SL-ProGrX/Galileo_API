namespace Galileo_API.Models.ProGrX.CuentasxCobrar
{
    public class FrmCxCCuentasSgtAutorizacionModels
    {
        public class CuentasSgtAutorizacionDto
        {
            public int Operacion { get; set; } 
            public string cod_concepto { get; set; } = string.Empty;
            public string cedula { get; set; } = string.Empty;
            public string nombre { get; set; } = string.Empty;
            public decimal Monto { get; set; } 
            public int? Dias_plazo { get; set; } 
            public decimal? Tasa_Corriente { get; set; }
            public decimal? cuota { get; set; }
            public string cod_Contrato { get; set; } = string.Empty;
            public string ContratoDesc { get; set; } = string.Empty;
            public string ConceptoDesc { get; set; } = string.Empty;
            public string Registro_Usuario { get; set; } = string.Empty;
            public DateTime Registro_Fecha { get; set; }
            public string Registro_FechaStr { get; set; } = string.Empty;
            public string? Notas { get; set; }
            public string? NotasDetalle { get; set; }
        }

        public class OperacionFacturasDto
        {
            public int Operacion { get; set; }
            public string  cod_factura { get; set; }
        }

    }
}
