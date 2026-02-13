namespace Galileo_API.Models.ProGrX.CuentasxCobrar
{
    public class FrmCxCCuentasSgtAutorizacionModels
    {
        public class CuentasSgtAutorizacionDto
        {
            public int Operacion { get; set; } 
            public string Cod_concepto { get; set; } = string.Empty;
            public string Cedula { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public decimal Monto { get; set; } 
            public int? Dias_plazo { get; set; } 
            public decimal? Tasa_Corriente { get; set; }
            public decimal? Cuota { get; set; }
            public string Cod_Contrato { get; set; } = string.Empty;
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
            public string Cod_factura { get; set; } = string.Empty;
        }

    }
}
