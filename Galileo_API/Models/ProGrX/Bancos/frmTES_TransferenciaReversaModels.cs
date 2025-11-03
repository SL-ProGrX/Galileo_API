namespace PgxAPI.Models.ProGrX.Bancos
{
    public class transferenciaSolicitudData
    {
        public int? nsolicitud { get; set; }
        public int? id_banco { get; set; }
        public string? codigo { get; set; }
        public string? beneficiario { get; set; }
        public float? monto { get; set; }
        public Nullable<DateTime> fecha_emision { get; set; }
        public string? cta_ahorros { get; set; }
        public string? documento { get; set; }
        public string? ndocumento { get; set; }
        public string? cod_plan { get; set; }
    }

    public class transferenciaReversaAplicaModel
    {
        public Nullable<DateTime> fecha_emision { get; set; }
        public string? clave { get; set; }
        public string? observaciones { get; set; }
        public string? usuario { get; set; }
        public int? id_banco { get; set; }
        public string? ndocumento { get; set; }
        public string? tipo { get; set; } 
        public List<TransferenciaEncabezadoModel> lista { get; set; } = new List<TransferenciaEncabezadoModel>();

    }

    public class TransferenciaEncabezadoModel
    {
        public int? nsolicitud { get; set; }
        public string? codigo { get; set; }
        public string? beneficiario { get; set; }
        public float? monto { get; set; }
        public Nullable<DateTime> fecha { get; set; }
        public string? cuenta { get; set; }
        public string? ndocumento { get; set; }
    }

    public class TransferenciaDetalleModel
    {
        public int? nsolicitud { get; set; }
        public string? cedula { get; set; }
        public string? nombre { get; set; }
        public float? monto { get; set; }
        public Nullable<DateTime> fecha_emision { get; set; }
        public string? cta_Ahorros { get; set; }
        public string? ndocumento { get; set; }
        public string? estado_transac_desc { get; set; }
        public string? tesoreria_id_new { get; set; }
    }

    public class tesAutorizacionesDTO
    {
        public string? NOMBRE { get; set; }
        public string? NOTAS { get; set; }
        public string? CLAVE { get; set; }
        public string? ESTADO { get; set; }
        public long? RANGO_GEN_INICIO { get; set; }
        public long? RANGO_GEN_CORTE { get; set; }
        public long? FIRMAS_GEN_INICIO { get; set; }
        public long? FIRMAS_GEN_CORTE { get; set; }
    }

    public class tesReversionData
    {
        public int id_reversion { get; set; }
        public string? autorizado { get; set; }
        public string? user_genera { get; set; }
        public Nullable<DateTime> fecha_genera { get; set; }
        public string? observaciones { get; set; }
        public string? documento { get; set; }
        public int id_banco { get; set; }
        public string? tipo { get; set; }
    }

}
