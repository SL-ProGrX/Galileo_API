namespace PgxAPI.Models.ProGrX.Clientes
{
    public class AfTiposSociedadesDto
    {
        public string cod_Sociedad { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool activa { get; set; }
    }

    public class AfTiposSociedadesLista
    {
        public int total { get; set; }
        public List<AfTiposSociedadesDto> lista { get; set; } = new List<AfTiposSociedadesDto>();
    }
}