namespace PgxAPI.Models.AH
{


    public class SifDocumentosDTO
    {
        public string idx { get; set; } = string.Empty;
        public string itmx { get; set; } = string.Empty;

    }

    public class TransaccionSIFDTO
    {
        public string cod_transaccion { get; set; } = string.Empty;
        public string tipo_documento { get; set; } = string.Empty;
        public string cod_concepto { get; set; } = string.Empty;
        public string cliente_identificacion { get; set; } = string.Empty;
        public string documento { get; set; } = string.Empty;
        public string cliente_nombre { get; set; } = string.Empty;
        public string monto { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string traspaso { get; set; } = string.Empty;
        public DateTime traspaso_fecha { get; set; }
        public string traspaso_usuario { get; set; } = string.Empty;
        public DateTime anulacion_fecha { get; set; }
        public string anulacion_usuario { get; set; } = string.Empty;
        public string linea1 { get; set; } = string.Empty;
        public string linea2 { get; set; } = string.Empty;
        public string linea3 { get; set; } = string.Empty;
        public string linea4 { get; set; } = string.Empty;
        public string linea5 { get; set; } = string.Empty;
        public string linea6 { get; set; } = string.Empty;
        public string linea7 { get; set; } = string.Empty;
        public string linea8 { get; set; } = string.Empty;
        public string linea9 { get; set; } = string.Empty;
        public string linea10 { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
        public string modulo { get; set; } = string.Empty;
        public string referencia_01 { get; set; } = string.Empty;
        public string referencia_02 { get; set; } = string.Empty;
        public string referencia_03 { get; set; } = string.Empty;
        public string cod_apertura { get; set; } = string.Empty;
        public string cod_caja { get; set; } = string.Empty;
        public string cod_oficina { get; set; } = string.Empty;
        public string reintegro_monto { get; set; } = string.Empty;
        public DateTime reintegro_fecha { get; set; }
        public string reintegro_solicitud { get; set; } = string.Empty;
        public string linea11 { get; set; } = string.Empty;
        public string traslado_bloqueo { get; set; } = string.Empty;
        public string analista_revision { get; set; } = string.Empty;
        public string analista_recepcion { get; set; } = string.Empty;
        public string caja_am_id { get; set; } = string.Empty;

    }

}
