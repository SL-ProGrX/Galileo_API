namespace Galileo_API.Models.ProGrX_Procesos
{
    public class CcPlanillaBitacoraData
    {
        public int id_seq { get; set; }
        public string gestion { get; set; } = string.Empty;
        public string transaccion { get; set; } = string.Empty;
        public string documento { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public DateTime? fecha { get; set; }
    }

    public class CcPlanillaBitacoraListaResult
    {
        public int total { get; set; }
        public List<CcPlanillaBitacoraData> lista { get; set; } = new();
    }

    public class CcPlanillaProcesoScrollDto
    {
        public decimal proceso { get; set; }
        public string proceso_format { get; set; } = string.Empty;
    }

    public class CcPlanillaBitacoraFiltroDto
    {
        public string cod_institucion { get; set; } = string.Empty;
        public string texto { get; set; } = string.Empty;
    }
}
