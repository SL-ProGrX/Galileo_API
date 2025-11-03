namespace PgxAPI.Models.ProGrX.Clientes
{
    public class AF_TiposIdsDTO
    {
        public int tipo_Id { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public string tipo_Personeria_Desc { get; set; } = string.Empty;
        public int largo_Minimo { get; set; }
        public string mascara { get; set; } = string.Empty;
        public int codigo_Sugef { get; set; }
        public int codigo_Pin { get; set; }
        public int codigo_Hacienda { get; set; }
        public int codigo_Sinpe { get; set; }
    }

    public class AF_TiposIdsLista
    {
        public int total { get; set; }
        public List<AF_TiposIdsDTO> lista { get; set; } = new List<AF_TiposIdsDTO>();
    }
}
