namespace Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels
{
    public class CcProcesoMensualArchivoGeneradoModel
    {
         
            public bool Generado { get; set; } = false;
            public string CodigoPlanillaEnvio { get; set; } = string.Empty;
            public string NombreArchivo { get; set; } = string.Empty;
            public string ContentType { get; set; } = "text/csv";
            public byte[] ArchivoBytes { get; set; } = Array.Empty<byte>();
      

    }
}
