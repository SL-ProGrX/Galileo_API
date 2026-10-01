namespace Galileo.Models.FSL
{
    public sealed class FslComiteDto
    {
        public string cod_comite { get; set; } =
            string.Empty;

        public string descripcion { get; set; } =
            string.Empty;

        public int numero_resolutores { get; set; } = 0;

        public bool activo { get; set; } = false;

        public string registro_usuario { get; set; } =
            string.Empty;

        public DateTime? registro_fecha { get; set; }
    }

    public sealed class FslComiteGuardarRequest
    {
        public string cod_comite { get; set; } =
            string.Empty;

        public string descripcion { get; set; } =
            string.Empty;

        public int numero_resolutores { get; set; } = 0;

        public bool activo { get; set; } = false;

        public string usuario { get; set; } =
            string.Empty;
    }

    public sealed class FslComiteMiembroDto
    {
        public string cedula { get; set; } =
            string.Empty;

        public string nombre { get; set; } =
            string.Empty;

        public string usuario_vinculado { get; set; } =
            string.Empty;

        public string cod_comite { get; set; } =
            string.Empty;

        public DateTime? registro_fecha { get; set; }

        public string registro_usuario { get; set; } =
            string.Empty;

        public DateTime? salida_fecha { get; set; }

        public string salida_usuario { get; set; } =
            string.Empty;

        public bool activo { get; set; } = false;
    }

    public sealed class FslComiteMiembroGuardarRequest
    {
        public string cod_comite { get; set; } =
            string.Empty;

        public string cedula { get; set; } =
            string.Empty;

        public string nombre { get; set; } =
            string.Empty;

        public string usuario_vinculado { get; set; } =
            string.Empty;

        public bool activo { get; set; } = false;

        public string usuario { get; set; } =
            string.Empty;
    }

    public class FslComitePaginacionFiltros
    {
        public int pagina { get; set; } = 0;

        public int paginacion { get; set; } = 30;

        public string filtro { get; set; } =
            string.Empty;

        public string sort_field { get; set; } =
            string.Empty;

        public int sort_order { get; set; } = 1;
    }

    public sealed class FslComiteMiembrosFiltros :
        FslComitePaginacionFiltros
    {
        public string cod_comite { get; set; } =
            string.Empty;
    }
}