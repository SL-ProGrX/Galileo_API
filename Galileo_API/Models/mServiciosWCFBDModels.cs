using Sinpe_CCD;
using Sinpe_PIN;

namespace Galileo.Models
{
    #region Consulta SINPE
    public class ParametrosSinpe
    {
        public string vHost { get; set; } = Environment.MachineName;
        public string? vHostPin { get; set; }
        public string? vIpHost { get; set; }
        public string? vUserCGP { get; set; }
        public int vCanalCGP { get; set; } = 5;
    }

    public class ResponseData
    {
        public bool IsSuccessful { get; set; }
        public string? OperationId { get; set; }
        public List<Errores> Errors { get; set; } = new List<Errores>();
        public Account Account { get; set; } = new Account();
    }

    public class InfoSinpeData
    {
        public string Cedula { get; set; } = string.Empty;
        public string Cuenta { get; set; } = string.Empty;
        public int tipoID { get; set; } = 0;

    }

    public class TesTransaccion
    {
        public int NumeroSolicitud { get; set; } = 0;
        public Nullable<DateTime> FechaEmision { get; set; } = null;
        public Nullable<DateTime> FechaTraslado { get; set; } = null;
        public string? UsuarioGenera { get; set; } = null;
        public bool? estadoSinpe { get; set; }
        public int? IdMotivoRechazo { get; set; }
        public string? CodigoReferencia { get; set; } = null;
        public string? DocumentoBase { get; set; } = null;
        public string contador { get; set; } = "0";

        public string? Detalle1 { get; set; } = null;
        public string? Detalle2 { get; set; } = null;
        public string? Detalle3 { get; set; } = null;
        public string? Detalle4 { get; set; } = null;
        public string? Detalle5 { get; set; } = null;

        public string? Divisa { get; set; } = null;
        public decimal tipoCambio { get; set; } = 0;
        public decimal Monto { get; set; } = 0;

        public string? CorreoNotifica { get; set; } = null;
        public string? CedulaOrigen { get; set; } = null;
        public string? NombreOrigen { get; set; } = null;
        public string? CuentaOrigen { get; set; } = null;
        public E_TipoIdentificacion tipoCedOrigen { get; set; }

        public string? Codigo { get; set; } = null;
        public string? Beneficiario { get; set; } = null;
        public string? Cuenta { get; set; } = null;
        public E_TipoIdentificacion tipoCedDestino { get; set; }

        public string? NDocumento { get; set; } = null;
    }

    public class VInfoSinpe
    {
        public string? Cedula { get; set; } = null;
        public string? CuentaIBAN { get; set; } = null;
        public int tipoID { get; set; } = 0;
    }
    #endregion


    #region Factura Electronica

    public class FeParametrosEncabezado
    {
        public byte CantDeci { get; set; } = 0;
        public short Sucursal { get; set; } = 0;
        public long CodigoActividad { get; set; } = 0;
        public int Terminal { get; set; } = 0;
    }

    public class FeReceptor
    {
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public byte? TipoIdent { get; set; }
        public string Identificacion { get; set; } = string.Empty;
    }

    public class FeDetalles
    {
        public short NumeroLinea { get; set; }
        public object? Codigo { get; set; }
        public object? Nombre { get; set; }
        public List<object>? CodProdServ { get; set; }
        public List<object>? CodTipo { get; set; }
        public object? Cantidad { get; set; }
        public object? UnidadMedida { get; set; }
        public object? UnidadComercial { get; set; }
        public object? Descripcion { get; set; }
        public object? PrecioUnitario { get; set; }
        public List<FactElectronica.FE_JsonDescuentos>? Descuentos { get; set; }
        public List<FactElectronica.FE_JsonImpuestos>? Impuestos { get; set; }
    }

    public class FeDescuentos
    {
        public float MontoDescuento { get; set; } = 0;
    }

    //Valores fijos
    public enum SituacionEnvio : short
    {
        Normal = 1,
        Contingencia = 2,
        Sin_Internet = 3
    }

    public enum Moneda : short
    {
        Colones = 1,
        Dollar = 2,
        Euro = 3
    }

    public enum CondicionVenta : short
    {
        Contado = 1,
        Credito = 2,
        Consignacion = 3
    }

    #endregion
}
