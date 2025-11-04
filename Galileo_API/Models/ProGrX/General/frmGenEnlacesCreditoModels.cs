namespace PgxAPI.Models.GEN
{
    public class EnlaceCreditoDto
    {
        public int CodEmpresa { get; set; }
        public int CodInstitucion { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string CodCredito { get; set; } = string.Empty;
    }

    public class CodigoCreditoDto
    {
        public string Descripcion { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
    }

    public class EnlaceCreditoLista
    {
        public List<EnlaceCreditoDto> lista { get; set; } = new List<EnlaceCreditoDto>();
        public int total { get; set; }
    }
}