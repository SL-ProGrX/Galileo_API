namespace PgxAPI.Models.CPR
{

    public class Cpr_Valora_EsquemaDTOList
    {
        public int Total { get; set; }
        public List<Cpr_Valora_EsquemaDTO> esquemas { get; set; } = new List<Cpr_Valora_EsquemaDTO>();
    }

    public class Cpr_Valora_EsquemaDTO
    {
        public string val_id { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool activo { get; set; }
    }

    public class Cpr_Valora_ItemsDTOList
    {
        public int Total { get; set; }
        public List<Cpr_Valora_ItemsDTO> items { get; set; } = new List<Cpr_Valora_ItemsDTO>();
    }

    public class Cpr_Valora_ItemsDTO
    {
        public string val_item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal peso { get; set; }
    }
}
