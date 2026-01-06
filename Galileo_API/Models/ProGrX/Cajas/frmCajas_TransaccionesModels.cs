namespace Galileo_API.Models.ProGrX.Cajas
{
    public class CajasSocioResult
    {
        public string? Cedula { get; set; } = string.Empty;
        public string? CedulaR { get; set; } = string.Empty;
        public string? Nombre { get; set; } = string.Empty;
    }

    public class CajasServicioConsultaParams
    {
        public int CodCaja { get; set; }
        public string? ServicioBusqueda { get; set; }
    }

    public class CajasServicioResult
    {
        public string? Cod_Servicio { get; set; } = string.Empty;
        public string? ServicioDesc { get; set; } = string.Empty;
        public string? Cod_Recaudador { get; set; } = string.Empty;
        public string? RecaudadorDesc { get; set; } = string.Empty;
    }

    public class CajasTransacValidacionResult
    {
        public string? Advertencias { get; set; } = string.Empty;
        public string? Validacion { get; set; } = string.Empty;
    }

    public class CajasTransacValidacionParams
    {
        public string? Caja { get; set; }
        public string? Usuario { get; set; }
        public int Apertura { get; set; }
        public int SesionId { get; set; }
        public string? CodServicio { get; set; }
        public decimal TotalCajas { get; set; }
        public string? Tiquete { get; set; }
    }

    public class CajasServiciosDatosParams
    {
        public string? Cod_Recaudador { get; set; }
        public string? Cod_Servicio { get; set; }
        public decimal Monto { get; set; }
        public string? Cod_Caja { get; set; }
    }

    public class CajasServiciosDatosResult
    {
        public string? Cod_Servicio { get; set; }
        public string? Cod_Recaudador { get; set; }
        public string? Cod_Concepto { get; set; }
        public string? ServicioDesc { get; set; }
        public string? RecaudadorDesc { get; set; }
        public string? Cod_Cuenta { get; set; }
        public string? Cod_Cuenta_Comision { get; set; }
        public string? Cod_Cuenta_IV { get; set; }
        public decimal Mnt_Bruto { get; set; }
        public decimal Comision { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Mnt_Neto { get; set; }
        public decimal Porc_Comision { get; set; }
        public decimal Porc_Impuesto { get; set; }
        public decimal Comision_Minima { get; set; }
        public int Intercambio { get; set; }
        public string? Ef_Cta { get; set; }
        public string? Ef_Codigo { get; set; }
        public string? Cod_Unidad { get; set; }
        public string? Cod_Centro_Costo { get; set; }
        public string? Cabys { get; set; }
        public int Genera_Factura { get; set; }
        public decimal Valor_Transito_Valida { get; set; }
    }

    public class SifTransaccionInsertParams
    {
        public string? Cod_Transaccion { get; set; }
        public string? Tipo_Documento { get; set; }
        public string? Registro_Usuario { get; set; }
        public string? Cliente_Identificacion { get; set; }
        public string? Cliente_Nombre { get; set; }
        public string? Cod_Concepto { get; set; }
        public decimal Monto { get; set; }
        public string? Referencia_01 { get; set; }
        public string? Referencia_02 { get; set; }
        public string? Referencia_03 { get; set; }
        public string? Cod_Oficina { get; set; }
        public string? Linea1 { get; set; }
        public string? Linea2 { get; set; }
        public string? Linea3 { get; set; }
        public string? Linea4 { get; set; }
        public string? Detalle { get; set; }
        public string? Documento { get; set; }
        public string? Cod_Caja { get; set; }
        public int Cod_Apertura { get; set; }
        public int Id_Sesion { get; set; }
    }

    public class CajasServiciosTransacInsertParams
    {
        public string? Cod_Caja { get; set; }
        public int Cod_Apertura { get; set; }
        public string? Cod_Recaudador { get; set; }
        public string? Cod_Servicio { get; set; }
        public string? Tipo_Documento { get; set; }
        public string? Cod_Transaccion { get; set; }
        public string? Num_Referencia { get; set; }
        public decimal Monto { get; set; }
        public decimal Comision { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Neto { get; set; }
        public string? Cod_Divisa { get; set; }
        public decimal Tipo_Cambio { get; set; }
    }

    public class SifDocsAsientoParams
    {
        public string? Tipo_Documento { get; set; }
        public string? Cod_Transaccion { get; set; }
        public decimal Mnt_Bruto { get; set; }
        public string? Cod_Divisa { get; set; }
        public decimal Tipo_Cambio { get; set; }
        public int Cod_Contabilidad { get; set; }
        public string? Cod_Unidad { get; set; }
        public string? Cod_Centro_Costo { get; set; }
        public string? Ef_Cta { get; set; }
        public string? Referencia_01 { get; set; }
        public string? Referencia_02 { get; set; }
        public string? Referencia_03 { get; set; }
        public short? Divisa_Rev { get; set; } = 0;
        public short? No_Reversa { get; set; } = 0;
    }

    public class SifDocsAsientoResult
    {
        public bool Exito { get; set; }
    }

    public class CajasDesglocePagosDocFinalParams
    {
        public string? Cod_Caja { get; set; }
        public int Cod_Apertura { get; set; }
        public string? Tiquete { get; set; }
        public string? Usuario { get; set; }
        public string? Tipo_Documento { get; set; }
        public string? Cod_Transaccion { get; set; }
        public string? Cod_Unidad { get; set; }
        public string? Referencia_01 { get; set; }
        public string? Referencia_02 { get; set; }
    }

    public class CajasIntercambioRegistraParams
    {
        public string? Tipo_Documento { get; set; }
        public string? Cod_Transaccion { get; set; }
        public string? Ef_Codigo { get; set; }
        public decimal Monto { get; set; }
        public string? Ef_Cta { get; set; }
        public string? Cod_Unidad { get; set; }
        public string? Usuario { get; set; }
    }

    public class CajasValoresTransitoRegistraParams
    {
        public string? Tipo_Documento { get; set; }
        public string? Cod_Transaccion { get; set; }
        public string? Cod_Recaudador { get; set; }
        public string? Cod_Servicio { get; set; }
        public string? Cod_Caja { get; set; }
        public int Cod_Apertura { get; set; }
        public string? Usuario { get; set; }
    }

    public class CajasGeneralTEParams
    {
        public string? Tipo_Documento { get; set; }
        public string? Cod_Transaccion { get; set; }
        public string? Tipo { get; set; } = "T";
    }

    public class CajasReciboDigitalParams
    {
        public string? NumeroDocumento { get; set; }
        public string? TipoDocumento { get; set; }
        public string? TipoComprobante { get; set; }
    }
}
