namespace Galileo.Models.INV
{
    public abstract class UnidadMedicionBase
    {
        public string cod_unidad { get; set; } = string.Empty;

        public string descripcion { get; set; } = string.Empty;
    }

    public sealed class UnidadMedicion
        : UnidadMedicionBase
    {
    }

    public sealed class UnidadMedicionDto
        : UnidadMedicionBase
    {
        public string hacienda { get; set; } = string.Empty;

        public bool activo { get; set; } = false;

        public string estado { get; set; } = string.Empty;

        public string registro_usuario { get; set; } = string.Empty;

        public DateTime? registro_fecha { get; set; }
    }

    public sealed class UnidadesDataLista
    {
        public int total { get; set; } = 0;

        public List<UnidadMedicionDto> unidades { get; set; } = [];
    }
}