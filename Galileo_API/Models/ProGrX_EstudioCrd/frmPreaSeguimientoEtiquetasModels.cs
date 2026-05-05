namespace Galileo_API.Models.ProGrX_Hipotecario
{
    public class PreaSeguimientoEtiquetasInfoDto
    {
        public string titulo { get; set; } = string.Empty;
        public string operacion { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public string identificacion { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
    }

    public class PreaSeguimientoEtiquetasLista
    {
        public int total { get; set; }
        public List<PreaSeguimientoEtiquetasData> lista { get; set; } = new();
    }

    public class PreaSeguimientoEtiquetasData
    {
        public short linea { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public string tag_codigo { get; set; } = string.Empty;
        public string etiqueta { get; set; } = string.Empty;
        public string asignado_a { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
    }

    public class PreaSeguimientoEtiquetasAplicarDto
    {
        public int id_solicitud { get; set; }
        public string cod_preanalisis { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public string tag_codigo { get; set; } = string.Empty;
        public string asignado_a { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public bool aviso_email { get; set; }
        public string emails { get; set; } = string.Empty;
    }
}