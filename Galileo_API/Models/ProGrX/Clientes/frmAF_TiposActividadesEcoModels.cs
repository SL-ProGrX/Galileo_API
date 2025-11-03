namespace PgxAPI.Models.ProGrX.Clientes
{
    public class AF_TiposActividadesEcoDTO
    {
        public string cod_actividad { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string? cod_sub_act { get; set; } = string.Empty;
        public bool activa { get; set; }
    }

    public class AF_TiposActividadesEcoLista
    {
        public int total { get; set; }
        public List<AF_TiposActividadesEcoDTO> lista { get; set; } = new List<AF_TiposActividadesEcoDTO>();
    }
}
