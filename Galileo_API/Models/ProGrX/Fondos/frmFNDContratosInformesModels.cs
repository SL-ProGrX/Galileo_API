namespace Galileo.Models.ProGrX.Fondos
{
    public class FndContratosInformesContrato
    {
        public int? cod_operadora { get; set; }
        public string? cod_plan { get; set; }
        public int? cod_contrato { get; set; }
        public string? cedula { get; set; }
        public string? cliente { get; set; }
        public string? bancodesc { get; set; }
        public string? plandesc { get; set; }
        public decimal? aportes { get; set; }
        public bool? tipo_cdp { get; set; }
    }

    public class FndContratosInformesLiquidacionesLista
    {
        public int total { get; set; }
        public List<FndContratosInformesLiquidacion> lineas { get; set; } = new();
    }

    public class FndContratosInformesLiquidacion
    {
        public int consec { get; set; }
    }
}
