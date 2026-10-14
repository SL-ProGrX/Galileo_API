namespace Galileo_API.Models.ProGrX.Convenios
{
    public class CrConvenioParametroData
    {
        public string cod_parametro { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string valor { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public string modifica_usuario { get; set; } = string.Empty;
        public DateTime? modifica_fecha { get; set; }
        public string valor_mask { get; set; } = string.Empty;
        public string cuenta_descripcion { get; set; } = string.Empty;
    }

    public class CrConvenioParametroActualizarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_parametro { get; set; } = string.Empty;
        public string valor { get; set; } = string.Empty;
    }
}
