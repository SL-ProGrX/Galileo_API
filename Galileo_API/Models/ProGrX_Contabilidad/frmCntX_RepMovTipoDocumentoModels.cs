namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class CntXTipoAsientoDto
    {
        public string Tipo_Asiento { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    public class CntXAsientoDto
    {
        public string Num_Asiento { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    public class CntXAsientoParams
    {
        public required int CodContabilidad { get; set; }
        public required string TipoAsiento { get; set; }
        public required int PeriodoAnio { get; set; }
        public required int PeriodoMes { get; set; }
    }
}