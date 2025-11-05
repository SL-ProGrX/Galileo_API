namespace PgxAPI.Models.ProGrX.Bancos
{
    public class TesBancosDocData
    {
        public string tipo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string movimiento { get; set; } = string.Empty;
        public string tipo_asiento { get; set; } = string.Empty;
        public string generacion { get; set; } = string.Empty;
        public string asiento_transac { get; set; } = string.Empty;
        public string asiento_formato { get; set; } = string.Empty;
        public string asiento_banco { get; set; } = string.Empty;
        public string asiento_mascara { get; set; } = string.Empty;
        public string tipoX { get; set; } = string.Empty;
    }

    public class TesBancoDocTipoData
    {
        public string tipo { get; set; } = string.Empty;
        public string id_banco { get; set; } = string.Empty;
        public bool reg_autorizacion { get; set; } = false;
        public bool reg_emision { get; set; } = false;
        public bool doc_auto { get; set; } = false;
        public int consecutivo { get; set; } = 0;
        public string comprobante { get; set; } = string.Empty;
        public bool mod_consec { get; set; } = false;
        public int cuenta_min { get; set; } = 0;
        public int cuenta_max { get; set; } = 0;
        public int consecutivo_det { get; set; } = 0;
        public Nullable<DateTime> registro_fecha { get; set; } = null;
        public string registro_usuario { get; set; } = string.Empty;

    }

    public class TesBancoDocDto
    {
        public string tipo { get; set; } = string.Empty;
        public string id_banco { get; set; } = string.Empty;
        public bool reg_autorizacion { get; set; } = false;
        public bool reg_emision { get; set; } = false;
        public bool doc_auto { get; set; } = false;
        public int consecutivo { get; set; } = 0;
        public string comprobante { get; set; } = string.Empty;
        public bool mod_consec { get; set; } = false;
        public int cuenta_min { get; set; } = 0;
        public int cuenta_max { get; set; } = 0;
        public int consecutivo_det { get; set; } = 0;
        public Nullable<DateTime> registro_fecha { get; set; } = null;
        public string registro_usuario { get; set; } = string.Empty;
        public Nullable<DateTime> actualiza_fecha { get; set; } = null;
        public string actualiza_usuario { get; set; } = string.Empty;
    }
}