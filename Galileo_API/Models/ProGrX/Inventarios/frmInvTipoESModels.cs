namespace PgxAPI.Models.INV
{
    public class TipoEsDto
    {
        public string Cod_Entsal { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Cod_Cuenta { get; set; } = string.Empty;
        public string CtaDesc { get; set; } = string.Empty;
        public bool Activo { get; set; } = false;
        public bool Mancomunado { get; set; } = false;
    }

    public class TipoESList
    {
        public int Total { get; set; }
        public List<TipoEsDto> Lista { get; set; } = new List<TipoEsDto>();
    }

    public class TipoESFiltros
    {
        public int? pagina { get; set; }
        public int? paginacion { get; set; }
        public string? filtro { get; set; }
    }
}