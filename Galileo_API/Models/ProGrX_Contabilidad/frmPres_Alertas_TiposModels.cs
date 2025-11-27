namespace Galileo.Models.PRES
{
    public class AlertasTiposDto
    {
        public string cod_desviacion { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool? activa { get; set; }
        public bool? requiere_justificacion { get; set; }
        public string tipo { get; set; } = string.Empty;
        public decimal? valor_desviacion { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public DateTime? modifica_fecha { get; set; }
        public string modifica_usuario { get; set; } = string.Empty;

    }

    public class AlertasTiposLista
    {
        public int total { get; set; }
        public List<AlertasTiposDto> lista { get; set; } = new List<AlertasTiposDto>();
    }
}