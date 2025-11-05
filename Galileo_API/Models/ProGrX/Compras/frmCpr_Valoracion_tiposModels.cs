namespace PgxAPI.Models.CPR
{
    public class CprValoraEsquemaDtoList
    {
        public int Total { get; set; }
        public List<CprValoraEsquemaDto> esquemas { get; set; } = new List<CprValoraEsquemaDto>();
    }

    public class CprValoraEsquemaDto
    {
        public string val_id { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool activo { get; set; }
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
        public decimal peso { get; set; }
    }
}