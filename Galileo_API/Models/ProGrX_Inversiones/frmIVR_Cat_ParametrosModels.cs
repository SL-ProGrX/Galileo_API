
namespace Galileo.Models.ProGrX_Inversiones
{
    public sealed class IvrCatParametroDto
    {
        public string cod_parametro { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string valor { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public DateTime? actualiza_fecha { get; set; }
        public string actualiza_usuario { get; set; } = string.Empty;
        public string valor_mascara { get; set; } = string.Empty;
        public string valor_descripcion { get; set; } = string.Empty;
    }

    public sealed class IvrCatParametroActualizarRequest
    {
        public string cod_parametro { get; set; } = string.Empty;
        public string valor { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }
}