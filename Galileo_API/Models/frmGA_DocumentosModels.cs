namespace Galileo.Models.GA
{
    public class DocumentosArchivoDto
    {
        public string fileid { get; set; } = string.Empty;
        public string typeid { get; set; } = string.Empty;
        public string moduloid { get; set; } = string.Empty;
        public string llave_01 { get; set; } = string.Empty;
        public string llave_02 { get; set; } = string.Empty;
        public string llave_03 { get; set; } = string.Empty;
        public string filename { get; set; } = string.Empty;
        public string filetype { get; set; } = string.Empty;
        public byte[]? filecontent { get; set; }
        public string fechaemision { get; set; } = string.Empty;
        public string vencimiento { get; set; } = string.Empty;
        public string registrofecha { get; set; } = string.Empty;
        public string registrousuario { get; set; } = string.Empty;
        public string empresaid { get; set; } = string.Empty;
        public string enable { get; set; } = string.Empty;
    }

    public class TiposDocumentosArchivosDto
    {
        public string moduloid { get; set; } = string.Empty;
        public string typeid { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string vencimientoapl { get; set; } = string.Empty;
    }

    public class GaDocumento
    {
        public string llave1 { get; set; } = string.Empty;
        public string? llave2 { get; set; }
        public string? llave3 { get; set; }
    }

    public class DocumentoFormData
    {
        public IFormFile? File { get; set; }
        public string? Info { get; set; }
    }
}
