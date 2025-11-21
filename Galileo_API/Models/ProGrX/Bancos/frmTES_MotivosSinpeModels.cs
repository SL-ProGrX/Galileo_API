namespace Galileo.Models.ProGrX.Bancos
{
    public class TesMotivosSinpeLista
    {
        public int total { get; set; }
        public List<TesMotivosSinpeDto> lista { get; set; } = new List<TesMotivosSinpeDto>();
    }

    public class TesMotivosSinpeDto
    {
        public int cod_motivo { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public string usuario_registro { get; set; } = string.Empty;
        public DateTime fecha_registro { get; set; } = DateTime.Now;
        public string usuario_actualiza { get; set; } = string.Empty;
        public DateTime? fecha_actualiza { get; set; } = null;
        public bool isNew { get; set; } = false;
    }
}
