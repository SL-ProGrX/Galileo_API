namespace Galileo.Models.CxP
{
    public class DocsPendientesTraslado
    {
        public int Facturas_Registradas { get; set; }
        public int Facturas_Anuladas { get; set; }
        public int Cargos_Flotante_Monto { get; set; }
        public int Cargos_Flotante_Porc { get; set; }
        public int Cargos_Directos_Factura { get; set; }
        public int Cargos_Flotantes_CobFactCancel_RetCargo { get; set; }
    }

    public class Desbalanceado
    {
        public string Tipo { get; set; } = string.Empty;
        public string Transaccion { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string Referencia { get; set; } = string.Empty;
        public string Notas { get; set; } = string.Empty;
    }

    public class Periodo
    {
        public int Anio { get; set; }
        public int Mes { get; set; }
        public int Cod_Contabilidad { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime Cierre_Fecha { get; set; }
        public string Cierre_Usuario { get; set; } = string.Empty;
        public DateTime Periodo_Corte { get; set; }
    }

    public class AsientoInfo
    {
        public string vTipoDoc { get; set; } = string.Empty;
        public string vTipoAsiento { get; set; } = string.Empty;
        public string vMascara { get; set; } = string.Empty;
        public string Inicio { get; set; } = string.Empty;
        public string Corte { get; set; } = string.Empty;
        public bool chkBalanceados { get; set; } = false;
        public string Usuario { get; set; } = string.Empty;
    }

    public class TrasladoData
    {
        public string Registro_Fecha { get; set; } = string.Empty;
        public string Cod_Proveedor { get; set; } = string.Empty;
        public string Cod_Transaccion { get; set; } = string.Empty;
        public string AsientoDesc { get; set; } = string.Empty;
        public string AsientoNotas { get; set; } = string.Empty;
        public string Referencia { get; set; } = string.Empty;
        public string Cod_Factura { get; set; } = string.Empty;
        public string CtaProveedor { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string Detalle { get; set; } = string.Empty;
        public string Cod_Cargo { get; set; } = string.Empty;
        public string Cod_Divisa { get; set; } = string.Empty;
        public decimal Tipo_Cambio { get; set; }
        public string CtaCargo { get; set; } = string.Empty;
        public int Id { get; set; }
        public decimal Monto { get; set; }
        public int IdX_Consec { get; set; }
    }
}