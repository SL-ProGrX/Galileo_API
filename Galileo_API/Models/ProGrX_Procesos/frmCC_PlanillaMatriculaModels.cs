namespace Galileo_API.Models.ProGrX_Procesos
{
    public class CcPlanillaMatriculaFiltroDto
    {
        public int cod_institucion { get; set; } = 0;

        public bool casos_activos { get; set; } = true;

        public string codigo { get; set; } = string.Empty;

        public string operacion { get; set; } = string.Empty;

        public string doc_referencia { get; set; } = string.Empty;

        public int? proceso { get; set; }

        public string cedula { get; set; } = string.Empty;

        public string nombre { get; set; } = string.Empty;
    }

    public class CcPlanillaMatriculaListaDto
    {
        public int id_referencia { get; set; } = 0;

        public string tipo { get; set; } = string.Empty;

        public int fecha_proceso { get; set; } = 0;

        public string cod_deduccion { get; set; } = string.Empty;

        public int id_solicitud { get; set; } = 0;

        public string operacion { get; set; } = string.Empty;

        public DateTime? formalizacion { get; set; }

        public decimal monto { get; set; } = 0;

        public decimal cuota { get; set; } = 0;

        public int plazo { get; set; } = 0;

        public decimal tasa { get; set; } = 0;

        public string nreferencia_ext { get; set; } = string.Empty;

        public string cedula { get; set; } = string.Empty;

        public string nombre { get; set; } = string.Empty;

        public short b_indica { get; set; } = 0;
    }

    public class CcPlanillaMatriculaListaResultDto
    {
        public int total { get; set; } = 0;

        public List<CcPlanillaMatriculaListaDto> lista { get; set; } = new();
    }

    public class CcPlanillaMatriculaBloquearRequest
    {
        public int id_referencia { get; set; } = 0;
    }

    public class CcPlanillaMatriculaBloqueoMasivoItem
    {
        public string cedula { get; set; } = string.Empty;

        public string numerooperacion { get; set; } = string.Empty;

        public string codigodeduccion { get; set; } = string.Empty;
    }

    public class CcPlanillaMatriculaBloqueoMasivoRequest
    {
        public int cod_institucion { get; set; } = 0;

        public List<CcPlanillaMatriculaBloqueoMasivoItem> items { get; set; } = new();
    }

    public class CcPlanillaMatriculaBloqueoMasivoResultDto
    {
        public int casos_bloqueados { get; set; } = 0;
    }

    public class CcPlanillaMatriculaArchivoTotalRequest
    {
        public int cod_institucion { get; set; } = 0;
    }

    public class CcPlanillaMatriculaArchivoTotalDto
    {
        public string nombre_archivo { get; set; } = string.Empty;

        public string contenido_base64 { get; set; } = string.Empty;

        public string content_type { get; set; } = "text/csv";
    }

    public class CcPlanillaMatriculaCadenaDto
    {
        public string cadena { get; set; } = string.Empty;
    }
}