namespace PgxAPI.Models.CxP
{
    public class ProveedoresPagosLista
    {
        public int Total { get; set; }
        public List<ProveedorPagos> Proveedores { get; set; } = new List<ProveedorPagos>();
    }

    public class ProveedorPagos
    {
        public string Cod_Proveedor { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Divisa { get; set; } = string.Empty;
        public string Cedjuridica { get; set; } = string.Empty;
        public int Cod_Banco { get; set; }
        public string Cuenta_Default { get; set; } = string.Empty;
    }

    public class FacturaPendiente_Pago
    {
        public int Npago { get; set; }
        public int Cod_Proveedor { get; set; }
        public string Cod_Factura { get; set; } = string.Empty;
        public DateTime Fecha_Vencimiento { get; set; }
        public decimal Monto { get; set; }
        public string Proveedor { get; set; } = string.Empty;
        public decimal Cargos { get; set; }
        public bool Apl_Cargo_Flotante { get; set; }
        public string Cod_Divisa { get; set; } = string.Empty;
        public decimal Tipo_Cambio { get; set; }
        public decimal Importe_Divisa_Real { get; set; }
        public string Usuario { get; set; } = string.Empty;

        public int Cod_Cargo { get; set; }

        public decimal Neto { get; set; }

        public string Datakey { get; set; } = string.Empty;
    }

    public class FactPen_Req
    {
        public int Proveedor { get; set; }
        public string Divisa { get; set; } = string.Empty;
        public string Corte { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
    }

    public class Detalle
    {
        public int Credito { get; set; }
        public DateTime Ultimo_Pago { get; set; }
        public decimal Saldo { get; set; }
        public decimal Car_Per_Saldo { get; set; }
        public decimal Car_Per_Porc { get; set; }
    }

    public class Autorizado
    {
        public string Nombre { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
    }

    public class Fusion
    {
        public int Cod_Proveedor_Fus { get; set; }
        public int Cod_Proveedor { get; set; }
        public string Proveedor { get; set; } = string.Empty;
    }

    public class InfoCuenta
    {
        public int Id_Banco { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public decimal Tipo_Cambio { get; set; }
        public string Cod_Divisa { get; set; } = string.Empty;
        public string CtaConta { get; set; } = string.Empty;
    }

    public class CuentaBancaria
    {
        public string Cuenta_Interna { get; set; } = string.Empty;
        public string Cuenta_Desc { get; set; } = string.Empty;
        public string IdX { get; set; } = string.Empty;
        public string ItmX { get; set; } = string.Empty;
        public int Prioridad { get; set; }
    }

    public class CargoPorcentual
    {
        public int Cod_Proveedor { get; set; }
        public decimal Procentaje { get; set; }
    }

    public class TesTransAsientoDTO
    {
        public int NSolicitud { get; set; }
        public string Cuenta_Contable { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string DebeHaber { get; set; } = string.Empty;
        public int Linea { get; set; }
        public string Cod_Unidad { get; set; } = string.Empty;
        public string Cod_Cc { get; set; } = string.Empty;
        public string Cod_Divisa { get; set; } = string.Empty;
        public decimal Tipo_Cambio { get; set; }
    }

    public class Anticipo
    {
        public decimal Cargos { get; set; }

        public string Cod_Cargo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Cod_Cuenta { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string Cod_Divisa { get; set; } = string.Empty;
    }

    public class Tes_TransaccionesDTO
    {
        public int Id_Banco { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public string Beneficiario { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime Fecha_Solicitud { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string Estadoi { get; set; } = string.Empty;
        public string Modulo { get; set; } = string.Empty;
        public string Submodulo { get; set; } = string.Empty;
        public string Cta_Ahorros { get; set; } = string.Empty;
        public string Detalle1 { get; set; } = string.Empty;
        public string Detalle2 { get; set; } = string.Empty;
        public string Referencia { get; set; } = string.Empty;
        public string Op { get; set; } = string.Empty;
        public bool Genera { get; set; }
        public bool Actualiza { get; set; }
        public string Cod_Unidad { get; set; } = string.Empty;
        public string Cod_Concepto { get; set; } = string.Empty;
        public string User_Solicita { get; set; } = string.Empty;
        public string Autoriza { get; set; } = string.Empty;
        public DateTime? Fecha_Autorizacion { get; set; }
        public string User_Autoriza { get; set; } = string.Empty;
        public string Tipo_Beneficiario { get; set; } = string.Empty;
        public decimal Tipo_Cambio { get; set; }
        public string Cod_Divisa { get; set; } = string.Empty;
    }

    public class DesembolsoNetos
    {
        public int Cod_Proveedor { get; set; }
        public string Cedjur { get; set; } = string.Empty;
        public decimal Neto { get; set; }
        public decimal Divisa_Real_Neto { get; set; }
    }

    public class PagoProvUpdate
    {
        public int Tesoreria { get; set; }
        public string User_Traslada { get; set; } = string.Empty;
        public string Pago_Tercero { get; set; } = string.Empty;
        public int Cod_Proveedor { get; set; }
        public bool IsPagoTerceroChecked { get; set; }

    }

    public class CargoPer
    {
        public int Cod_Cargo { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string Cod_Cuenta { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string Cod_Divisa { get; set; } = string.Empty;
    }

    public class ProveedorInfoEjecucion
    {
        public string CedJur { get; set; } = string.Empty;
        public int Cod_Proveedor { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string Cod_Cuenta { get; set; } = string.Empty;
        public string Cod_Divisa { get; set; } = string.Empty;
        public string CtaDivDifIng { get; set; } = string.Empty;
        public string CtaDivDifGst { get; set; } = string.Empty;
        public decimal Tipo_Cambio { get; set; }
        public DateTime Fecha { get; set; }
    }

}

