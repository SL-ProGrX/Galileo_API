namespace Galileo.Models.GEN
{
    public class EnlaceCreditoDto
    {
        public required int CodEmpresa { get; set; }
        public required int CodInstitucion { get; set; }
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