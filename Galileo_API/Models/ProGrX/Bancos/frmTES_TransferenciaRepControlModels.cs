namespace PgxAPI.Models.ProGrX.Bancos
{
    public class DropDownCatalogoBancos
    {
        public string idx { get; set; } = string.Empty;
        public string itmx { get; set; } = string.Empty;
    }

    public class TransferenciaRepControl_CatalogoDTO
    {
        public List<DropDownCatalogoBancos> Tipos { get; set; }
        public List<DropDownCatalogoBancos> Formatos { get; set; }
        public List<DropDownCatalogoBancos> Planes { get; set; }
    }
}
