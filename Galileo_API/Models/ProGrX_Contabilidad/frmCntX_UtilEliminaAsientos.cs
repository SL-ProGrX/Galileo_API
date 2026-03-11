namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class CntxEliminarAsientosRequestDto
    {
        public int? cod_empresa { get; set; }
        public int? cod_contabilidad { get; set; }
        public string? tipo_asiento { get; set; } = string.Empty;

        public DateTime? desde { get; set; }
        public DateTime? hasta { get; set; }

        public int? anio { get; set; }
        public int? mes { get; set; }

        public string? usuario { get; set; } = string.Empty;
    }

    public class CntxPeriodoActualDto
    {
        public int? anio { get; set; }
        public int? mes { get; set; }
    }
}
