namespace Galileo.Models.ProGrX.Cobros
{
    public class FrmCONotificaEmailDropdownItem
    {
        public string item { get; set; } = "";
        public string descripcion { get; set; } = "";
    }
    public class FrmCONotificaEmailCaseData
    {
        public string cedula { get; set; } = "";
        public string nombre { get; set; } = "";
        public string estadoDesc { get; set; } = "";
        public string empresa { get; set; } = "";
        public string email { get; set; } = "";
        public decimal? moraTotal { get; set; }
        public decimal? moraCuotas { get; set; }
        public decimal? ctaObreroPend { get; set; }
        public decimal? ctaPatronalPend { get; set; }
    }
    public class FrmCONotificaEmailListaResult
    {
        public List<FrmCONotificaEmailCaseData> lista { get; set; } = new();
        public int total { get; set; } = 0;
    }
    public class FrmCONotificaEmailNotificarDto
    {
        public string cedula { get; set; } = "";
        public string tipo { get; set; } = "R";
    }

    public class FrmCONotificaEmailNotificarBulkDto
    {
        public List<string> cedulas { get; set; } = new();
        public string tipo { get; set; } = "R";
    }
    public class FrmCONotificaEmailConsultaDto
    {
        public int institucionId { get; set; } = 0;
        public string estado { get; set; } = "T";
        public string tipoCobro { get; set; } = "T";
        public string tipoNotifica { get; set; } = "D";
    }
}