namespace PgxAPI.Models.ProGrX.Clientes
{
    public class AfPersonaActivacionDto
    {
        public string cedula { get; set; } = string.Empty;
        public int pri_deduc { get; set; }
        public string usuario { get; set; } = string.Empty;
        public int id_promotor { get; set; }
        public string cod_oficina { get; set; } = string.Empty;
        public string boleta { get; set; } = string.Empty;
    }
}