using Galileo.Models;

namespace Galileo_API.Models.ProGrX_Procesos
{
    public class CcCaRemesasCatalogosResponse
    {
        public List<DropDownListaGenericaModel> lineas { get; set; } = new();
        public List<DropDownListaGenericaModel> entidades { get; set; } = new();
        public List<DropDownListaGenericaModel> procesos { get; set; } = new();
        public List<DropDownListaGenericaModel> cuotas { get; set; } = new();
        public List<DropDownListaGenericaModel> filtros { get; set; } = new();
    }

    public class CcCaRemesasEnvioConsultaRequest
    {
        public int cod_remesa { get; set; } = 0;
        public string cod_entidad { get; set; } = string.Empty;
        public DateTime fecha_vence { get; set; } = DateTime.MinValue;
        public int cuotas { get; set; } = 0;
    }

    public class CcCaRemesasEnvioConsultaData
    {
        public string cedula { get; set; } = string.Empty;
        public long id_solicitud { get; set; } = 0;
        public string nombre { get; set; } = string.Empty;
        public decimal compromiso { get; set; } = 0;
        public string tarjeta_mask { get; set; } = string.Empty;
        public string tarjeta_vence { get; set; } = string.Empty;
        public string tarjeta_tipo { get; set; } = string.Empty;
        public string fecult { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public string correo { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string tarjeta_numero { get; set; } = string.Empty;
    }

    public class CcCaRemesasRecibeDetalleData
    {
        public long referencia { get; set; } = 0;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal monto_cuota { get; set; } = 0;
        public string tarjeta_mask { get; set; } = string.Empty;
        public string estado_desc { get; set; } = string.Empty;
        public string transaccion { get; set; } = string.Empty;
    }

    public class CcCaRemesasEnvioPendienteData
    {
        public long numero_generacion { get; set; }
    }

    public class CcCaRemesasEnvioRegistrarRequest
    {
        public int cod_remesa { get; set; } = 0;
        public string cod_entidad { get; set; } = string.Empty;
        public DateTime fecha_vence { get; set; } = DateTime.MinValue;  
        public string proceso { get; set; } = string.Empty;
        public long numero_generacion { get; set; } = 0;
        public List<CcCaRemesasEnvioConsultaData> seleccionados { get; set; } = new();
    }

    public class CcCaRemesasArchivoBancoRow
    {
        public string Formato { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string Referencia { get; set; } = string.Empty;
        public string Tarjeta { get; set; } = string.Empty;
        public DateTime Fecha_Vence { get; set; } = DateTime.MinValue;
        public decimal Monto { get; set; } = 0;
        public DateTime Fecha_Transaccion { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string NUMERO_AFILIADO { get; set; } = string.Empty;
    }

    public class CcCaRemesasAutorizacionExcelData
    {
        public string documento { get; set; } = string.Empty;
        public string transaccion { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
    }

    public class CcCaRemesasRecibeAutorizacionesRequest
    {
        public long numero_generacion { get; set; } = 0;
        public string usuario { get; set; } = string.Empty;
        public List<CcCaRemesasAutorizacionExcelData> autorizaciones { get; set; } = new();
    }

    public class CcCaRemesasRecibeAplicaRequest
    {
        public long numero_generacion { get; set; } = 0;
        public string usuario { get; set; } = string.Empty;
        public string tipo_documento { get; set; } = "CA";
        public string numero_documento { get; set; } = string.Empty;
        public int lote { get; set; } = 50;
    }

    public class CcCaRemesasRecibeAplicaResponse
    {
        public int pendientes { get; set; } = 0;
    }

    public class CcCaRemesasRecibeAsientoRequest
    {
        public string tipo_documento { get; set; } = "CA";
        public string numero_documento { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public long numero_generacion { get; set; } = 0;
    }

    public class CcCaRemesasRecibeImprimeReciboRequest
    {
        public string numero_documento { get; set; } = string.Empty;
        public string tipo_documento { get; set; } = "CA";
        public string usuario { get; set; } = string.Empty;
        public bool reimprimir { get; set; } = false;
    }

    public class CcCaRemesasConsultasRequest
    {
        public int? numero_generacion { get; set; }
        public int? cod_linea { get; set; }
        public string? cod_entidad { get; set; }
        public DateTime fecha_inicio { get; set; } = DateTime.MinValue;
        public DateTime fecha_fin { get; set; } = DateTime.MinValue;
        public string tipo_cuota { get; set; } = "T";
        public string estado { get; set; } = "T";
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
    }

    public class CcCaRemesasConsultasData
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public long id_solicitud { get; set; } = 0;
        public int cod_linea { get; set; } = 0;
        public string linea_desc { get; set; } = string.Empty;
        public decimal monto_cuota { get; set; } = 0;
        public string tipo_desc { get; set; } = string.Empty;
        public string fecult { get; set; } = string.Empty;
        public string tarjeta_mask { get; set; } = string.Empty;
        public string tarjeta_vence { get; set; } = string.Empty;
        public string tarjeta_tipo { get; set; } = string.Empty;
        public string estado_desc { get; set; } = string.Empty;
        public long referencia { get; set; } = 0;
        public string transaccion { get; set; } = string.Empty;
        public string fecha_generacion { get; set; } = string.Empty;
        public decimal recaudado { get; set; } = 0;
        public string entidad_desc { get; set; } = string.Empty;
        public string remesa_desc { get; set; } = string.Empty;
        public string correo { get; set; } = string.Empty;
        public long numero_generacion { get; set; } = 0;
    }

    public class CcCaRemesasConsultasResponse
    {
        public List<CcCaRemesasConsultasData> detalle { get; set; } = new();
        public decimal total_compromiso { get; set; } = 0;
        public decimal total_recaudado { get; set; } = 0;
        public int total_casos { get; set; } = 0;
    }

    public static class CcCaRemesaCOnstantes
    {
        public const string vRequestRequerido = "El request es requerido.";
        public const string vNumeroGeneracionRequerido = "El número de generación es requerido.";
        public const string vUsuarioRequerido = "El usuario es requerido.";
    }
}
