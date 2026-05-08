namespace Galileo.Models.ProGrX_Cobros
{
    public class CoControlAsgManualExpedienteItem
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string operacion { get; set; } = string.Empty;
        public string linea { get; set; } = string.Empty;
        public string linea_desc { get; set; } = string.Empty;
    }
    public class CoControlAsgManualExpedienteDetalle
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string usuario_actual { get; set; } = string.Empty;
        public string asignacion_texto { get; set; } = string.Empty;
        public string oficina_agencia { get; set; } = string.Empty;

        public int mantener { get; set; } = 1;
        public int tiene_morosidad { get; set; } = 0;
        public string estado_morosidad { get; set; } = "N/A";
        public string info_expediente { get; set; } = string.Empty;
    }
    public class CoControlAsgManualUsuarioItem
    {
        public string usuario { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
    }
    public class CoControlAsgManualAplicarRequest
    {
        public string cedula { get; set; } = string.Empty;
        public string usuario_nuevo { get; set; } = string.Empty;
        public int mantener { get; set; } = 1;
    }
}