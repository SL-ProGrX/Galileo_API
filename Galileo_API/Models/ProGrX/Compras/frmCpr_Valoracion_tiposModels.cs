namespace Galileo.Models.CPR
{
    public sealed class CprValoraConsultaRequest
    {
        public int? Pagina { get; set; }
        public int? Paginacion { get; set; }
        public string? Filtro { get; set; }
        public string? SortField { get; set; }
        public int SortOrder { get; set; } = 1;
    }

    public class CprValoraEsquemaDtoList
    {
        public int Total { get; set; }
        public List<CprValoraEsquemaDto> esquemas { get; set; } = new List<CprValoraEsquemaDto>();
    }

    public class CprValoraEsquemaDto
    {
        public string val_id { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public required bool activo { get; set; }
    }

    public class CprValoraItemsDtoList
    {
        public int Total { get; set; }
        public List<CprValoraItemsDto> items { get; set; } = new List<CprValoraItemsDto>();
    }

    public class CprValoraItemsDto
    {
        public string val_item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public required decimal peso { get; set; }
    }
}
