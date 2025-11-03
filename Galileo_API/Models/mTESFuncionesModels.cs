namespace PgxAPI.Models
{
    public class TesoreriaMaestroModel
    {
        public string vTipoDocumento { get; set; } = string.Empty;
        public long vBanco { get; set; }
        public float vMonto { get; set; }
        public string vCodigo { get; set; } = string.Empty;
        public string vBeneficiario { get; set; } = string.Empty;
        public long vOP { get; set; }
        public string vDetalle1 { get; set; } = string.Empty;
        public long vReferencia { get; set; }
        public string vDetalle2 { get; set; } = string.Empty;
        public string vCuenta { get; set; } = string.Empty;
        public string vFecha { get; set; } = string.Empty;
        public string? vUnidad { get; set; } = "OC";
        public string? vConcepto { get; set; } = "GEN";
        public string? vRef_01 { get; set; } = "";
        public string? vRef_02 { get; set; } = "";
        public string? vRef_03 { get; set; } = "";
        public string? vCodApp { get; set; } = "ProGrX";
        public string? vToken { get; set; } = "";
        public string? vRemesaTipo { get; set; } = "";
        public long? vRemesa { get; set; } = 0;
    }

    public class TesoreriaDetalleModel
    {
        public long vSolicitud { get; set; }
        public string vCtaConta { get; set; } = string.Empty;
        public float vMonto { get; set; }
        public string vDH { get; set; } = string.Empty;
        public int vLinea { get; set; }
        public string vUnidad { get; set; } = "OC";
        public string vCC { get; set; } = "";

    }

    public class TES_TransaccionesDTO
    {
        public int NSOLICITUD { get; set; }
        public int ID_BANCO { get; set; }
        public string TIPO { get; set; } = string.Empty;
        public string CODIGO { get; set; } = string.Empty;
        public string BENEFICIARIO { get; set; } = string.Empty;
        public decimal? MONTO { get; set; }
        public DateTime? FECHA_SOLICITUD { get; set; }
        public string ESTADO { get; set; } = string.Empty;
        public DateTime? FECHA_EMISION { get; set; }
        public DateTime? FECHA_ANULA { get; set; }
        public string ESTADOI { get; set; } = string.Empty;
        public string MODULO { get; set; } = string.Empty;
        public string CTA_AHORROS { get; set; } = string.Empty;
        public string NDOCUMENTO { get; set; } = string.Empty;
        public string DETALLE1 { get; set; } = string.Empty;
        public string DETALLE2 { get; set; } = string.Empty;
        public string DETALLE3 { get; set; } = string.Empty;
        public string DETALLE4 { get; set; } = string.Empty;
        public string DETALLE5 { get; set; } = string.Empty;
        public decimal? REFERENCIA { get; set; }
        public string SUBMODULO { get; set; } = string.Empty;
        public string GENERA { get; set; } = string.Empty;
        public string ACTUALIZA { get; set; } = string.Empty;
        public string UBICACION_ACTUAL { get; set; } = string.Empty;
        public DateTime? FECHA_TRASLADO { get; set; }
        public string UBICACION_ANTERIOR { get; set; } = string.Empty;
        public string ENTREGADO { get; set; } = string.Empty;
        public string AUTORIZA { get; set; } = string.Empty;
        public DateTime? FECHA_ASIENTO { get; set; }
        public DateTime? FECHA_ASIENTO2 { get; set; }
        public string ESTADO_ASIENTO { get; set; } = string.Empty;
        public DateTime? FECHA_AUTORIZACION { get; set; }
        public string USER_AUTORIZA { get; set; } = string.Empty;
        public decimal? OP { get; set; }
        public string DETALLE_ANULACION { get; set; } = string.Empty;
        public string USER_ASIENTO_EMISION { get; set; } = string.Empty;
        public string USER_ASIENTO_ANULA { get; set; } = string.Empty;
        public string COD_CONCEPTO { get; set; } = string.Empty;
        public string COD_UNIDAD { get; set; } = string.Empty;
        public string USER_GENERA { get; set; } = string.Empty;
        public string USER_SOLICITA { get; set; } = string.Empty;
        public string USER_ANULA { get; set; } = string.Empty;
        public string USER_ENTREGA { get; set; } = string.Empty;
        public DateTime? FECHA_ENTREGA { get; set; }
        public string DOCUMENTO_REF { get; set; } = string.Empty;
        public string DOCUMENTO_BASE { get; set; } = string.Empty;
        public string DETALLE { get; set; } = string.Empty;
        public string USER_HOLD { get; set; } = string.Empty;
        public DateTime? FECHA_HOLD { get; set; }
        public DateTime? FIRMAS_AUTORIZA_FECHA { get; set; }
        public string FIRMAS_AUTORIZA_USUARIO { get; set; } = string.Empty;
        public decimal? TIPO_CAMBIO { get; set; }
        public string COD_DIVISA { get; set; } = string.Empty;
        public short? TIPO_BENEFICIARIO { get; set; }
        public string COD_APP { get; set; } = string.Empty;
        public string REF_01 { get; set; } = string.Empty;
        public string REF_02 { get; set; } = string.Empty;
        public string REF_03 { get; set; } = string.Empty;
        public string ID_TOKEN { get; set; } = string.Empty;
        public string REMESA_TIPO { get; set; } = string.Empty;
        public int? REMESA_ID { get; set; }
        public string ASIENTO_NUMERO { get; set; } = string.Empty;
        public string ASIENTO_NUMERO_ANU { get; set; } = string.Empty;
        public int? CONCILIA_ID { get; set; }
        public string CONCILIA_TIPO { get; set; } = string.Empty;
        public DateTime? CONCILIA_FECHA { get; set; }
        public string CONCILIA_USUARIO { get; set; } = string.Empty;
        public string COD_PLAN { get; set; } = string.Empty;
        public short? MODO_PROTEGIDO { get; set; }
    }
}
