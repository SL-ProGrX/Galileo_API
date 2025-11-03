namespace PgxAPI.Models.ProGrX.Clientes
{
    public class AF_TiposSociedadesDTO
    {
        public string cod_Sociedad { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool activa { get; set; }
    }

    public class AF_TiposSociedadesLista
    {
        public int total { get; set; }
        public List<AF_TiposSociedadesDTO> lista { get; set; } = new List<AF_TiposSociedadesDTO>();
    }
}
