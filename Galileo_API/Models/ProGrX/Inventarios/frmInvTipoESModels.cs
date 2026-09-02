namespace Galileo.Models.INV
{
    public static class InvTipoEsTiposMovimiento
    {
        public const string Entrada = "E";
        public const string Salida = "S";
        public const string Traslado = "T";
        public const string Requisicion = "R";

        public static bool INV_TipoES_Tipo_Valido(string? tipo)
        {
            return tipo is
                Entrada or
                Salida or
                Traslado or
                Requisicion;
        }
    }

    public abstract class TipoEsRegistroBase
    {
        public string cod_entsal { get; set; } = string.Empty;

        public string descripcion { get; set; } = string.Empty;

        public string tipo { get; set; } = string.Empty;

        public string cod_cuenta { get; set; } = string.Empty;

        public bool activo { get; set; } = false;
    }

    public sealed class TipoEsDto : TipoEsRegistroBase
    {
        public string cta_desc { get; set; } = string.Empty;
    }

    public sealed class TipoEsGuardarRequest : TipoEsRegistroBase
    {
        public string usuario { get; set; } = string.Empty;
    }

    public sealed class TipoEsEliminarRequest
    {
        public string cod_entsal { get; set; } = string.Empty;

        public string usuario { get; set; } = string.Empty;
    }

    public sealed class TipoESList
    {
        public int total { get; set; } = 0;

        public List<TipoEsDto> lista { get; set; } = [];
    }

    public sealed class TipoESFiltros
    {
        public int pagina { get; set; } = 0;

        public int paginacion { get; set; } = 0;

        public string filtro { get; set; } = string.Empty;
    }
}