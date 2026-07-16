namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrCtaCatalogoCuenta
    {
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal? Impuesto { get; set; }

        public string CtaNIntc { get; set; } = string.Empty;
        public string CtaNIntc_Mask { get; set; } = string.Empty;
        public string CtaNIntc_Desc { get; set; } = string.Empty;

        public string CtaNIntm { get; set; } = string.Empty;
        public string CtaNIntm_Mask { get; set; } = string.Empty;
        public string CtaNIntm_Desc { get; set; } = string.Empty;

        public string CtaNAmort { get; set; } = string.Empty;
        public string CtaNAmort_Mask { get; set; } = string.Empty;
        public string CtaNAmort_Desc { get; set; } = string.Empty;

        public string CtaOIntc { get; set; } = string.Empty;
        public string CtaOIntc_Mask { get; set; } = string.Empty;
        public string CtaOIntc_Desc { get; set; } = string.Empty;

        public string CtaOIntm { get; set; } = string.Empty;
        public string CtaOIntm_Mask { get; set; } = string.Empty;
        public string CtaOIntm_Desc { get; set; } = string.Empty;

        public string CtaOAmort { get; set; } = string.Empty;
        public string CtaOAmort_Mask { get; set; } = string.Empty;
        public string CtaOAmort_Desc { get; set; } = string.Empty;

        public string CtaCIntc { get; set; } = string.Empty;
        public string CtaCIntc_Mask { get; set; } = string.Empty;
        public string CtaCIntc_Desc { get; set; } = string.Empty;

        public string CtaCIntm { get; set; } = string.Empty;
        public string CtaCIntm_Mask { get; set; } = string.Empty;
        public string CtaCIntm_Desc { get; set; } = string.Empty;

        public string CtaCAmort { get; set; } = string.Empty;
        public string CtaCAmort_Mask { get; set; } = string.Empty;
        public string CtaCAmort_Desc { get; set; } = string.Empty;

        public string CtaPuente { get; set; } = string.Empty;
        public string CtaPuente_Mask { get; set; } = string.Empty;
        public string CtaPuente_Desc { get; set; } = string.Empty;

        public string CtaCarProducto { get; set; } = string.Empty;
        public string CtaCarProducto_Mask { get; set; } = string.Empty;
        public string CtaCarProducto_Desc { get; set; } = string.Empty;

        public string CtaProdAcum { get; set; } = string.Empty;
        public string CtaProdAcum_Mask { get; set; } = string.Empty;
        public string CtaProdAcum_Desc { get; set; } = string.Empty;

        public string CtaIntAdelantado { get; set; } = string.Empty;
        public string CtaIntAdelantado_Mask { get; set; } = string.Empty;
        public string CtaIntAdelantado_Desc { get; set; } = string.Empty;

        public string CtaPsDeudora { get; set; } = string.Empty;
        public string CtaPsDeudora_Mask { get; set; } = string.Empty;
        public string CtaPsDeudora_Desc { get; set; } = string.Empty;

        public string CtaPsAcreadora { get; set; } = string.Empty;
        public string CtaPsAcreadora_Mask { get; set; } = string.Empty;
        public string CtaPsAcreadora_Desc { get; set; } = string.Empty;

        public int? PsRegistra { get; set; }

        public string CtaCargosAnticipo { get; set; } = string.Empty;
        public string CtaCargosAnticipo_Mask { get; set; } = string.Empty;
        public string CtaCargosAnticipo_Desc { get; set; } = string.Empty;

        public string CtaIva { get; set; } = string.Empty;
        public string CtaIva_Mask { get; set; } = string.Empty;
        public string CtaIva_Desc { get; set; } = string.Empty;
    }

    public class CrCtaCatalogoCuentasGuardarRequest
    {
        public string Codigo { get; set; } = string.Empty;
        public string CtaNIntCor { get; set; } = string.Empty;
        public string CtaNIntMor { get; set; } = string.Empty;
        public string CtaNPrincipal { get; set; } = string.Empty;
        public string CtaOIntCor { get; set; } = string.Empty;
        public string CtaOIntMor { get; set; } = string.Empty;
        public string CtaOPrincipal { get; set; } = string.Empty;
        public string CtaCIntCor { get; set; } = string.Empty;
        public string CtaCIntMor { get; set; } = string.Empty;
        public string CtaCPrincipal { get; set; } = string.Empty;
        public string CtaPuente { get; set; } = string.Empty;
        public string CtaPagoAnticipado { get; set; } = string.Empty;
        public string CtaIva { get; set; } = string.Empty;
        public int IIva { get; set; }
        public string CtaIntCobAdelantado { get; set; } = string.Empty;
        public string CtaPaEfectos { get; set; } = string.Empty;
        public string CtaPaCartera { get; set; } = string.Empty;
        public int IPaSuspenso { get; set; }
        public string CtaPsDeudora { get; set; } = string.Empty;
        public string CtaPsAcreedora { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
    }

    public class CrCtaCatalogoCuentasGuardarResultado
    {
        public int Aplica { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }
}
