namespace Galileo.Models.ProGrX_Procesos
{
    public class EncargadoExcedenteDto
    {
        public string usuario { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public bool activo { get; set; }

        public string? registro_usuario { get; set; }
        public DateTime? registro_fecha { get; set; }

        public string? modifica_usuario { get; set; }
        public DateTime? modifica_fecha { get; set; }
    }
}