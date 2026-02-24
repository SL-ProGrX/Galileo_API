namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class CntxContabilidadListaDto
    {
        public int? cod_contabilidad { get; set; }
        public string? nombre { get; set; } = string.Empty;
    }

    public class CntxUtilEliminaContabilidadesRequestDto
    {
        public int? cod_empresa { get; set; }
        public List<int> contabilidades { get; set; } = new();
        public string? usuario { get; set; } = string.Empty;
    }
}
