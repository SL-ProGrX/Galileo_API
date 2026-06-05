namespace Galileo.Models.ProGrX_Procesos
{
    public class ExcedenteAplicadorDto
    {
        public required string usuario { get; set; }
        public bool activo { get; set; }
        public bool carga { get; set; }
        public bool real { get; set; }
        public bool proyectado { get; set; }
        public bool prorrateado { get; set; }
    }
}