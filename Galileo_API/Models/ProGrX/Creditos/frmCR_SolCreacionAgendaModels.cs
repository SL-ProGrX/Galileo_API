namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrSolCreacionAgendaActaData
    {
        public int? acta { get; set; }
        public int? id_comite { get; set; }
        public int? validaActa { get; set; }
        public DateTime? fechaInicio { get; set; }
        public DateTime? fechaCorte { get; set; }
        public bool chkPreAnalisis { get; set; }
    }

    public class CrSolCreacionAgendaReporteData
    {
        public string reporte { get; set; } = string.Empty;
        public string reg_credito { get; set; } = string.Empty;
    }
}