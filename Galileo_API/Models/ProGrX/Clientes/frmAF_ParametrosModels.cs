namespace PgxAPI.Models.ProGrX.Clientes
{
    public class AF_ParametrosDTO
    {
        public string cod_parametro { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string valor { get; set; } = string.Empty;
    }

    public class AF_ParametrosLista
    {
        public int total { get; set; }
        public List<AF_ParametrosDTO> lista { get; set; } = new List<AF_ParametrosDTO>();
    }
}
