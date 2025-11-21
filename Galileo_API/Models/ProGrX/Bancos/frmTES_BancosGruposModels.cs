namespace Galileo.Models.ProGrX.Bancos
{
    public class TesBancosGruposLista
    {
        public int total { get; set; } = 0;
        public List<TesBancosGruposData>? data { get; set; }
    }

    public class TesBancosGruposData
    {
        public string cod_grupo { get; set; } = string.Empty;
        public string desc_corta { get; set; } = string.Empty;
        public string id_sfn { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public int lcta_interna { get; set; } = 0;
        public int lcta_interbancaria { get; set; } = 0;
        public string tcta_utiliza { get; set; } = string.Empty;
        public bool activo { get; set; } = true;
        public object? firma_n1 { get; set; }
        public object? firma_n2 { get; set; }
        public string registro_usuario { get; set; } = string.Empty; 
    }

    public class TesBancosGruposImgData
    {
        public int cod_empresa { get; set; } = 0; // Código de la empresa
        public string cod_grupo { get; set; } = string.Empty;
        public string imagenLogo { get; set; } = string.Empty;
        public int firmaSelect  { get; set; } = 0; // 0 = No hay firma, 1 = Firma N1, 2 = Firma N2
        public object? firma_n1 { get; set; } = null; // Firma N1
        public object? firma_n2 { get; set; } = null; // Firma N2
    }
}