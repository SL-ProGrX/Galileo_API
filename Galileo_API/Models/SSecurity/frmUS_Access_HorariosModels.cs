namespace Galileo.Models.Security
{
    public class HorarioDto
    {
        public int IdEmpresa { get; set; }
        public string CodHorario { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string UsuarioRegistro { get; set; } = string.Empty;
        public TimeSpan LunInicio { get; set; }
        public TimeSpan LunCorte { get; set; }
        public TimeSpan MarInicio { get; set; }
        public TimeSpan MarCorte { get; set; }
        public TimeSpan MieInicio { get; set; }
        public TimeSpan MieCorte { get; set; }
        public TimeSpan JueInicio { get; set; }
        public TimeSpan JueCorte { get; set; }
        public TimeSpan VieInicio { get; set; }
        public TimeSpan VieCorte { get; set; }
        public TimeSpan SabInicio { get; set; }
        public TimeSpan SabCorte { get; set; }
        public TimeSpan DomInicio { get; set; }
        public TimeSpan DomCorte { get; set; }
        public DateTime FechaModificacion { get; set; }
        public string UsuarioModifica { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }
}
