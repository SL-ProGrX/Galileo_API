namespace PgxAPI.Models.CxP
{
    public class ProgramacionPagoDto
    {
        public int Cod_Proveedor { get; set; }
        public string Cod_Factura { get; set; } = string.Empty;
        public decimal? Total { get; set; } = null;
        public string? Cxp_Estado { get; set; } = null;
        public DateTime? Fecha { get; set; } = null;
        public DateTime? Vence { get; set; } = null;
        public DateTime? Fecha_Ingreso { get; set; } = null;
        public string Proveedor { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Forma_Pago { get; set; } = string.Empty;
        public string Cod_Divisa { get; set; } = string.Empty;
        public decimal? Tipo_Cambio { get; set; } = null;
        public decimal? Importe_Divisa_Real { get; set; } = null;
        public string DataKey { get; set; } = string.Empty;
    }

    public class ProgramacionPagoLista
    {
        public int Total { get; set; }
        public List<ProgramacionPagoDto> FacturasPago { get; set; } = new List<ProgramacionPagoDto>();
    }

    public class ConsultaPagosParam
    {
        public bool ConSaldos { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string Forma_Pago { get; set; } = string.Empty;
    }

    public class CargoAdicional
    {
        public string Cod_Cargo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Monto { get; set; }
    }

    public class SaldosInformacion
    {
        public int Credito_Plazo { get; set; }
        public decimal Saldo { get; set; }
        public decimal Saldo_Factura { get; set; }
    }

    public class FacturaDatos
    {
        public string CxP_Estado { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public decimal Imp_Ventas { get; set; }
    }

    public class DetallePago
    {
        public int NPago { get; set; }
        public string Cod_Factura { get; set; } = string.Empty;
        public int Cod_Proveedor { get; set; }
        public decimal Cargo { get; set; }
        public decimal Monto { get; set; }
        public decimal Neto { get; set; }
        public decimal Tesoreria { get; set; }
        public DateTime Fecha_Vencimiento { get; set; }
        public decimal Importe_Divisa_Real { get; set; }
        public string Cod_Divisa { get; set; } = string.Empty;
        public decimal Tipo_Cambio { get; set; }
        public string Forma_Pago { get; set; } = string.Empty;
        public decimal Frecuencia { get; set; }
        public decimal Tipo { get; set; }
        public int Apl_Cargo_Flotante { get; set; }
        public decimal Pago_Anticipado { get; set; }
    }

    public class TesoreriaDetalle
    {
        public string Estado { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Beneficiario { get; set; } = string.Empty;
        public decimal Monto { get; set; }
    }

    public class PagoProvCargo
    {
        public int NPago { get; set; }
        public string Cod_Cargo { get; set; } = string.Empty;
        public int Cod_Proveedor { get; set; }
        public string Cod_Factura { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
        public string Cod_Divisa { get; set; } = string.Empty;
        public decimal Tipo_Cambio { get; set; }
        public string Tipo_Cargo { get; set; } = string.Empty;
        public string Tipo_Proceso { get; set; } = string.Empty;
    }

    public class Disponible
    {
        public int NPago { get; set; }
        public decimal Neto { get; set; }
    }
}