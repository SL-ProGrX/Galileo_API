namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class CntXDiferidoPendienteDto
    {
        public int Cod_Diferido { get; set; }
        public int Cod_DifPlantilla { get; set; }
        public int Cod_Contabilidad { get; set; }
        public int Anio { get; set; }
        public int Mes { get; set; }
        public int Consecutivo { get; set; }
        public decimal Monto_Diferir { get; set; }
        public decimal Acumulado { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }

    public class CntXDiferidoPendienteParams
    {
        public required int CodContabilidad { get; set; }
        public required short PeriodoAnio { get; set; }
        public required short PeriodoMes { get; set; }
    }

    public class CntXDiferidoAsientoParams
    {
        public required int CodContabilidad { get; set; }
        public required int Plantilla { get; set; }
        public required int Diferido { get; set; }
        public required int Anio { get; set; }
        public required short Mes { get; set; }
        public required string Usuario { get; set; }
    }

    public class CntXDiferidoAsientoResult
    {
        public string TipoDoc { get; set; } = string.Empty;
        public string NumDoc { get; set; } = string.Empty;
    }
}
