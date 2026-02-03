namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class ArfCuentasDto
    {
        public string id_cuenta { get; set; } = string.Empty;   
        public string cod_unidad { get; set; } = string.Empty;
        public string cod_divisa { get; set; } = string.Empty;
        public string cod_cuenta { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime modifica_fecha { get; set; }
        public string modifica_usuario { get; set; } = string.Empty;
        public string cod_cuenta_mask { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string unidad_desc { get; set; } = string.Empty;
    }

    public class ArfCuentasRegistraRequest
    {
        public string cod_unidad { get; set; } = string.Empty;
        public string cod_divisa { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;

        public string cta_activo { get; set; } = string.Empty;
        public string cta_pasivo { get; set; } = string.Empty;
        public string cta_gasto_interes { get; set; } = string.Empty;
        public string cta_gasto_alquiler { get; set; } = string.Empty;
        public string cta_amort_derecho { get; set; } = string.Empty;
        public string cta_amort_acumulada { get; set; } = string.Empty;
        public string cta_puente { get; set; } = string.Empty;
    }
}
