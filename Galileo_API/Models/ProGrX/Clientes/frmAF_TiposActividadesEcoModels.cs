namespace PgxAPI.Models.ProGrX.Clientes
{
    public class AfTiposActividadesEcoDto
    {
        public string cod_actividad { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string? cod_sub_act { get; set; } = string.Empty;
        public bool activa { get; set; }
    }

    public class AfTiposActividadesEcoLista
    {
        public int total { get; set; }
        public List<AfTiposActividadesEcoDto> lista { get; set; } = new List<AfTiposActividadesEcoDto>();
    }
}