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

    public static class CcCaRemesaCOnstantes
    {
        public const string vRequestRequerido = "El request es requerido.";
    }
}
