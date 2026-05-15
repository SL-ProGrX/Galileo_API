namespace Galileo_API.Models.ProGrX_EstudioCrd
{
    public class PreaComiteIdDto
    {
        public int Id_Comite { get; set; }
    }

    public class PreaComiteMiembroDto
    {
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Asignado { get; set; } = string.Empty;
    }

    public class PreaAutorizadorRequestDto
    {
        public string Expediente { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
    }
}
