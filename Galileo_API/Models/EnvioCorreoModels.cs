namespace Galileo.Models
{
    public class EnvioCorreoModels
    {
        public string Providers { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string EnableSsl { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string cod_smtp { get; set; } = string.Empty;
    }

    public class EmailRequest
    {
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string CopyHide { get; set; } = string.Empty;
        public List<IFormFile> Attachments { get; set; } = new List<IFormFile>();
    }

    public class DocArchivoBeneIntegralDto
    {
        public int? codCliente { get; set; }
        public string? cedula { get; set; }
        public string? cod_beneficio { get; set; } 
        public int? id_beneficio { get; set; }
        public int? consec { get; set; }
        public string? body { get; set; }
        public string? filename { get; set; } 
        public string? filetype { get; set; }
        public string? filecontent { get; set; }
        public string? usuario { get; set; }
    }
}
