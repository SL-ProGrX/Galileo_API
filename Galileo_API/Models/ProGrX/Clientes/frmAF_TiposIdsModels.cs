namespace Galileo.Models.ProGrX.Clientes
{
    public class AfTiposIdsDto
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

    public class AfTiposIdsLista
    {
        public int total { get; set; }
        public List<AfTiposIdsDto> lista { get; set; } = new List<AfTiposIdsDto>();
    }
}