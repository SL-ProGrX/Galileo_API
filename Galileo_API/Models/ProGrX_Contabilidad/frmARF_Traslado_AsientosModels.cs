namespace Galileo_API.Models.ProGrX_Activos
{
    public class ArfTrasladoFiltroDto
    {
        public DateTime? fechaInicio { get; set; }
        public DateTime? fechaCorte { get; set; }
        public bool? todos { get; set; }
    }

    public class ArfTrasladoTablaDto
    {
        public int? cod_contabilidad { get; set; }
        public string? tipo_asiento { get; set; } = string.Empty;
        public string? num_asiento { get; set; } = string.Empty;
        public DateTime fecha { get; set; }
        public string? referencia { get; set; } = string.Empty;
        public string? notas { get; set; } = string.Empty;
    }

    public class ArfTrasladoRequestDto
    {
        public int? cod_contabilidad { get; set; }
        public string? tipo_asiento { get; set; } = string.Empty;
        public string? num_asiento { get; set; } = string.Empty;
    }


}
