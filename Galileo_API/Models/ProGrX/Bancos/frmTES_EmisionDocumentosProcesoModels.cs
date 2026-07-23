namespace Galileo.Models.TES
{
    public sealed class TesEmisionDocumentosProcesoIniciarRequest
    {
        public TesEmisionDocFiltros filtros { get; init; } = new();
        public int banco { get; init; }
        public string plan { get; init; } = string.Empty;
        public IReadOnlyList<int> solicitudes { get; init; } = [];
    }

    public sealed class TesEmisionDocumentosProcesoResult
    {
        public Guid procesoId { get; init; }
        public int banco { get; init; }
        public long? documentoBase { get; init; }
        public string estado { get; init; } = string.Empty;
        public string etapa { get; init; } = string.Empty;
        public int total { get; init; }
        public int procesadas { get; init; }
        public int exitosas { get; init; }
        public int errores { get; init; }
        public int consultasRealizadas { get; init; }
        public decimal porcentaje { get; init; }
        public DateTime fechaInicio { get; init; }
        public DateTime ultimaActividad { get; init; }
        public long tiempoTranscurridoMs { get; init; }
        public string mensaje { get; init; } = string.Empty;
        public string resultadoContexto { get; init; } = string.Empty;
    }

    public sealed class TesEmisionDocumentosArchivoResult
    {
        public Guid archivoId { get; init; }
        public string nombre { get; init; } = string.Empty;
        public string extension { get; init; } = string.Empty;
        public string contentType { get; init; } = string.Empty;
        public long tamano { get; init; }
        public string sha256 { get; init; } = string.Empty;
    }

    public sealed class TesEmisionDocumentosProcesoManifiestoResult
    {
        public Guid procesoId { get; init; }
        public string contexto { get; init; } = string.Empty;
        public IReadOnlyList<TesEmisionDocumentosArchivoResult> archivos { get; init; } =
            Array.Empty<TesEmisionDocumentosArchivoResult>();
    }

    public sealed class TesEmisionDocumentosArchivoValidacion
    {
        public bool EsValido { get; init; }
        public int Paginas { get; init; }
        public long Tamano { get; init; }
        public string Sha256 { get; init; } = string.Empty;
    }

    public sealed class TesEmisionDocumentosProcesoTrabajo
    {
        public int CodEmpresa { get; init; }
        public Guid ProcesoId { get; init; }
    }

    public sealed class TesEmisionDocumentosProcesoTrabajoContexto
    {
        public Guid ProcesoId { get; init; }
        public int CodEmpresa { get; init; }
        public int Banco { get; init; }
        public string Cod_Plan { get; init; } = string.Empty;
        public string Usuario { get; init; } = string.Empty;
        public int Total { get; init; }
    }

    public sealed class TesEmisionDocumentosProcesoDetalleTrabajo
    {
        public int NSolicitud { get; init; }
    }

    public sealed class TesEmisionDocumentosAvancePersistir
    {
        public Guid ProcesoId { get; init; }
        public int Total { get; init; }
        public int Procesadas { get; init; }
        public int Exitosas { get; init; }
        public int Errores { get; init; }
        public int ConsultasRealizadas { get; init; }
        public string Etapa { get; init; } = string.Empty;
    }

    public sealed class TesEmisionDocumentosProcesoContexto
    {
        public Guid proceso_id { get; set; }
        public int cod_empresa { get; set; }
        public string propietario { get; set; } = string.Empty;
        public string solicitud_hash { get; set; } = string.Empty;
        public string filtros { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string etapa { get; set; } = string.Empty;
        public int total { get; set; }
        public int procesadas { get; set; }
        public int exitosas { get; set; }
        public int errores { get; set; }
        public int consultas_realizadas { get; set; }
        public string? mensaje { get; set; }
        public DateTime fecha_inicio { get; set; }
        public DateTime ultima_actividad { get; set; }
    }

    public sealed class TesEmisionDocumentosArchivoPersistir
    {
        public Guid ArchivoId { get; init; }
        public Guid ProcesoId { get; init; }
        public int Orden { get; init; }
        public string Nombre { get; init; } = string.Empty;
        public string Extension { get; init; } = string.Empty;
        public string ContentType { get; init; } = string.Empty;
        public string RutaInterna { get; init; } = string.Empty;
        public long Tamano { get; init; }
        public string Sha256 { get; init; } = string.Empty;
        public int? Paginas { get; init; }
    }

    public sealed class TesEmisionDocumentosArchivoContexto
    {
        public Guid archivo_id { get; set; }
        public Guid proceso_id { get; set; }
        public string nombre { get; set; } = string.Empty;
        public string extension { get; set; } = string.Empty;
        public string content_type { get; set; } = string.Empty;
        public string ruta_interna { get; set; } = string.Empty;
        public long tamano { get; set; }
        public string sha256 { get; set; } = string.Empty;
    }

    public sealed class TesEmisionDocumentosProcesoOptions
    {
        public string Subcarpeta { get; set; } = "TES_EmisionDocumentos";
        public int RetencionDias { get; set; } = 7;
    }

    public sealed class TesEmisionDocumentosArchivoGenerado
    {
        public string Nombre { get; init; } = string.Empty;
        public string ContentType { get; init; } = string.Empty;
        public byte[] Contenido { get; init; } = Array.Empty<byte>();
    }
}
