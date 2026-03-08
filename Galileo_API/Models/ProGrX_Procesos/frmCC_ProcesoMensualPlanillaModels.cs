namespace Galileo_API.Models.ProGrX_Procesos
{
    public class CcProcesoMensualPlanillaListaDto
    {
        public int cod_institucion { get; set; }
        public string desc_corta { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string proceso { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;

        public short aplicada { get; set; }
        public string frecuencia_id { get; set; } = "M";
        public DateTime? pr_fecha_corte { get; set; }
        public short activa { get; set; }
    }

    public class CcProcesoMensualPlanillaFiltrosDto
    {
        public string usuario { get; set; } = string.Empty;
        public short activa { get; set; } = 1;
        public int? codigo { get; set; }
        public string descripcion { get; set; } = string.Empty;
    }
}
