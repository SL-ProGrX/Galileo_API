namespace PgxAPI.Models.AF
{
    public class OptionabledQuestion
    {
        public int? id_opciones { get; set; }
        public string? item { get; set; }
        public string? descripcion { get; set; }
        public bool? selected { get; set; }
    }

    public class FormQuestion
    {
        public int id_frm_pregunta { get; set; }
        public int? pregunta_orden { get; set; }
        public string? pregunta_titulo { get; set; }
        public string? pregunta_tipo { get; set; }
        public List<OptionabledQuestion>? opciones { get; set; }
        public object? respuesta { get; set; }
        public DateTime? respuestaFecha { get; set; }
        public bool? requerido { get; set; }
        public int? total_opciones { get; set; }
        public string? campo_homologado { get; set; }
    }

    public class Form
    {
        public int id { get; set; }
        public List<FormQuestion>? questions { get; set; }
    }

    public class Formulario
    {
        public int id_form { get; set; }
        public int cod_formulario { get; set; }
        public string cod_beneficio { get; set; } = string.Empty;
        public string frm_titulo { get; set; } = string.Empty;
        public Form formulario { get; set; } = new Form();
        public string registro_usuario { get; set; } = string.Empty;
        public string modifica_usuario { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; }
        public DateTime modifica_fecha { get; set; }
        public bool? activo { get; set; }
        public int? total_preguntas { get; set; }
        public string? campo_homologado { get; set; }
    }

    public class ReporteFormularioDatos
    {
        public string cod_beneficio { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; }
        public string cedula { get; set; } = string.Empty;
        public int pregunta_orden { get; set; }
        public string pregunta_tipo { get; set; } = string.Empty;
        public string pregunta_titulo { get; set; } = string.Empty;
        public string respuesta { get; set; } = string.Empty;
    }

    public class FrmReporteDatos
    {
        public int codCliente { get; set; }
        public int id_frm { get; set; }
        public string? fechaInicio { get; set; }
        public string? fechaFin { get; set; }
        public string? cedula { get; set; }
    }

    public class FrmFiltros
    {
        public int codCliente { get; set; }
        public string? cod_beneficio { get; set; }
        public int? id_beneficio { get; set; }
        public string? socio { get; set; }
        public string? usuario { get; set; }
    }
}
