namespace PgxAPI.Models.CxP
{
    public class Divisa
    {
        public string Cod_Divisa { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    public class FacturaDto
    {
        public string Cod_Factura { get; set; } = string.Empty;
        public int Cod_Proveedor { get; set; }
        public string Cod_Forma_Pago { get; set; } = string.Empty;
        public DateTime? Fecha { get; set; }
        public char? Estado { get; set; }
        public string Notas { get; set; } = string.Empty;
        public char? Cxp_Estado { get; set; }
        public decimal? Total { get; set; }
        public DateTime? Vence { get; set; }
        public DateTime? Asiento_Fecha { get; set; }
        public char? Asiento_Generado { get; set; }
        public DateTime? Creacion_Fecha { get; set; }
        public string Creacion_User { get; set; } = string.Empty;
        public short Plantilla { get; set; }
        public DateTime? Anula_Fecha { get; set; }
        public string Anula_User { get; set; } = string.Empty;
        public DateTime? Anula_Asiento_Fecha { get; set; }
        public string Cod_Divisa { get; set; } = string.Empty;
        public decimal? Tipo_Cambio { get; set; }
        public decimal? Importe_Divisa_Real { get; set; }
        public decimal? Impuesto_Ventas { get; set; }

        // Adicionales
        public string Proveedor { get; set; } = string.Empty;  // 'cxp_proveedores' 
        public string DivisaProv { get; set; } = string.Empty; // 'cxp_proveedores' 
        public decimal? Saldo { get; set; }
        public string DivisaFactura { get; set; } = string.Empty; //  'CntX_Divisas' 
    }

    public class AsientoFactura
    {
        public int Linea { get; set; }
        public int Cod_Contabilidad { get; set; }
        public string Cod_Cuenta_Mask { get; set; } = string.Empty;
        public string Cod_Cuenta { get; set; } = string.Empty;
        public string Cuenta { get; set; } = string.Empty;
        public string Debehaber { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string Cod_Unidad { get; set; } = string.Empty;
        public string Unidad { get; set; } = string.Empty;
        public string Cod_Centro_Costo { get; set; } = string.Empty;
        public string CentroCosto { get; set; } = string.Empty;
        public int Cod_Proveedor { get; set; }
        public string Cod_Factura { get; set; } = string.Empty;
        public string Cod_Divisa { get; set; } = string.Empty;
        public string Divisa { get; set; } = string.Empty;
        public decimal Tipo_Cambio { get; set; }
        public decimal Credito { get; set; }
        public decimal Debito { get; set; }
        public string Divisa_Desc { get; set; } = string.Empty;
        public string Centro_Desc { get; set; } = string.Empty;
        public string DataKey { get; set; } = string.Empty;
    }

    public class FacturaCambioNo
    {
        public string Cod_Factura { get; set; } = string.Empty;
        public int Cod_Proveedor { get; set; }
        public string Cod_FacturaNew { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
    }

    public class FacturaImpuesto
    {
        public string Cod_Factura { get; set; } = string.Empty;
        public int Cod_Proveedor { get; set; }
        public decimal Impuesto_Ventas { get; set; }
        public string Usuario { get; set; } = string.Empty;

    }

    public class ParametrosIva
    {
        public string Cod_Parametro { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Valor { get; set; } = string.Empty;
        public string Desc_Cuenta { get; set; } = string.Empty;
        public string Cod_Cuenta_Mask { get; set; } = string.Empty;
    }

    public class DivisaLocal
    {
        public string Cod_Divisa { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    public class ProveedorFactura
    {
        public int Cod_Proveedor { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string Cod_Divisa { get; set; } = string.Empty;
        public string Divisa_Local { get; set; } = string.Empty;
        public string Cod_Cuenta { get; set; } = string.Empty;
        public string Cod_Cuenta_Mask { get; set; } = string.Empty;
        public string Desc_Cuenta { get; set; } = string.Empty;
    }

    public class FacturaAntSig
    {
        public string Cod_Factura { get; set; } = string.Empty;
    }

    public class FacturaAnular
    {
        public string Cod_Factura { get; set; } = string.Empty;
        public int Cod_Proveedor { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }

    public class FacturaPlantillaLista
    {
        public int Total { get; set; }
        public List<FacturaPlantilla> Plantillas { get; set; } = new List<FacturaPlantilla>();
    }

    public class FacturaPlantilla
    {
        public string Cod_Plantilla { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    public class FacturaLista
    {
        public int Total { get; set; }
        public List<Factura> Facturas { get; set; } = new List<Factura>();
    }

    public class Factura
    {
        public string Cod_Factura { get; set; } = string.Empty;
        public int Cod_Proveedor { get; set; }
        public string Proveedor { get; set; } = string.Empty;
        public string Notas { get; set; } = string.Empty;
        public decimal Total_Factura { get; set; }
        public DateTime? Fecha { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string DataKey { get; set; } = string.Empty;
    }

    public class CuentaProveedor
    {
        public string Cod_Cuenta { get; set; } = string.Empty;
        public string Cod_Cuenta_Mask { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string DivisaProv { get; set; } = string.Empty;
    }

    public class PagoContado
    {
        public int NPago { get; set; }
        public int Cod_Proveedor { get; set; }
        public string Cod_Factura { get; set; } = string.Empty;
        public DateTime Fecha_Vencimiento { get; set; }
        public decimal Monto { get; set; }
        public int Frecuencia { get; set; }
        public int Tipo_Transac { get; set; }
        public string User_Traslada { get; set; } = string.Empty;
        public DateTime Fecha_Traslada { get; set; }
        public int Tesoreria { get; set; }
        public string Pago_Tercero { get; set; } = string.Empty;
        public int Apl_Cargo_Flotante { get; set; }
        public int Pago_Anticipado { get; set; }
        public string Forma_Pago { get; set; } = string.Empty;
        public decimal Importe_Divisa_Real { get; set; }
        public decimal Tipo_Cambio { get; set; }
        public string Cod_Divisa { get; set; } = string.Empty;
    }
}