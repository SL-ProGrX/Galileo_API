namespace Galileo_API.Models.ProGrX_EstudioCrd
{
    public class CrdPreaTiposSalariosData
    {
        public string tipo_salario { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public int prioridad { get; set; } = 0;
        public int meses { get; set; } = 0;
        public bool modifica_devengado { get; set; } = false;
        public bool modifica_rebajo_extras { get; set; } = false;
        public bool modifica_extras_fijas { get; set; } = false;
        public string operacion { get; set; } = string.Empty;
        public bool activo { get; set; } = false;
    }
}