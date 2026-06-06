namespace Galileo.Models.AH
{
    public class FrmAhExcedentesParametroDto
    {
        public string cod_parametro { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string valor { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public string modifica_usuario { get; set; } = string.Empty;
        public DateTime? modifica_fecha { get; set; }
    }

    public class FrmAhExcedentesParametroActualizarRequest
    {
        public string cod_parametro { get; set; } = string.Empty;
        public string valor { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }
}
