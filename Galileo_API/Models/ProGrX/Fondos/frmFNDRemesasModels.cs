namespace Galileo.Models.ProGrX.Fondos
{
    public class FndRemesasData
    {
        public int remesa { get; set; } = 0;
        public DateTime fecha { get; set; }
        public string usuario { get; set; } = string.Empty;
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_corte { get; set; }
        public string notas { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public decimal? total { get; set; }
    }

    public class FndRemesasCargaData
    {
        public int consec { get; set; } = 0;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string cod_plan { get; set; } = string.Empty;
        public string cod_operadora { get; set; } = string.Empty;
        public string cod_contrato { get; set; } = string.Empty;
        public decimal? monto { get; set; }
        public DateTime? fecha { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string cta_ahorros { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
    }
}
