namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrSeguimientoTagsUsuarioDto
    {
        public string usuario { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
    }

    public class CrSeguimientoTagsOperacionDto
    {
        public long id_solicitud { get; set; }
        public string codigo { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string oficina { get; set; } = string.Empty;
        public bool seleccionado { get; set; }
    }

    public class CrSeguimientoTagsLista
    {
        public int total { get; set; }
        public List<CrSeguimientoTagsListaData> lista { get; set; } = new();
    }

    public class CrSeguimientoTagsListaData
    {
        public long id_solicitud { get; set; }
        public DateTime? fechaforp { get; set; }
        public DateTime? fechasol { get; set; }

        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;

        public decimal montoapr { get; set; }
        public decimal cuota { get; set; }
        public int plazo { get; set; }
        public decimal int_tasa { get; set; }

        public string estadosol { get; set; } = string.Empty;
        public string estado_desc { get; set; } = string.Empty;

        public string documentacion { get; set; } = string.Empty;
        public int analistas_revision { get; set; }
        public DateTime? en_espera_fecha { get; set; }

        public bool seleccionado { get; set; }
    }

    public class CrSeguimientoTagsListaFiltroDto
    {
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_fin { get; set; }

        public string estado { get; set; } = "Todos";
        public string documentacion { get; set; } = "Todos";

        public bool solo_revisados { get; set; }
        public bool solo_espera { get; set; }

        public string texto { get; set; } = string.Empty;
    }

    public class CrSeguimientoTagsAplicarRequest
    {
        public string tag_codigo { get; set; } = string.Empty;
        public string observacion { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public List<CrSeguimientoTagsOperacionAplicarDto> operaciones { get; set; } = new();
    }

    public class CrSeguimientoTagsOperacionAplicarDto
    {
        public long id_solicitud { get; set; }
        public string codigo { get; set; } = string.Empty;
    }

    public class CrSeguimientoTagsAplicarResult
    {
        public int total_procesadas { get; set; }
        public int total_errores { get; set; }
        public List<CrSeguimientoTagsAplicarErrorDto> errores { get; set; } = new();
    }

    public class CrSeguimientoTagsAplicarErrorDto
    {
        public long id_solicitud { get; set; }
        public string error { get; set; } = string.Empty;
    }
}