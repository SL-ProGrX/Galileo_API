namespace Galileo_API.Models.ProGrX
{
    public class FrmAhPlanillaDirectaCargadoDto
    {
        public string llave_01 { get; set; } = string.Empty;
        public string ref_01 { get; set; } = string.Empty;
        public decimal monto_01 { get; set; } = 0;
        public string detalle { get; set; } = string.Empty;
    }

    public class FrmAhPlanillaDirectaInconsistenciaDto
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal monto { get; set; } = 0;
        public string aplica { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
    }

    public class FrmAhPlanillaDirectaCargaFilaRequest
    {
        public string llave_01 { get; set; } = string.Empty;
        public string ref_01 { get; set; } = string.Empty;
        public decimal monto_01 { get; set; } = 0;
    }

    public class FrmAhPlanillaDirectaCargadoRequest
    {
        public int cod_institucion { get; set; } = 0;
        public int proceso { get; set; } = 0;
        public string tipo_aporte { get; set; } = string.Empty;
        public string num_doc { get; set; } = string.Empty;
        public string archivo { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
        public List<FrmAhPlanillaDirectaCargaFilaRequest> registros { get; set; } = new();
    }

    public class FrmAhPlanillaDirectaProcesarRequest
    {
        public int cod_institucion { get; set; } = 0;
        public int proceso { get; set; } = 0;
        public string tipo_aporte { get; set; } = string.Empty;
        public string num_doc { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
    }

    public class FrmAhPlanillaDirectaProcesarResponse
    {
        public string accion { get; set; } = string.Empty;
        public string mensaje { get; set; } = string.Empty;
        public string num_doc { get; set; } = string.Empty;
        public int total_registros { get; set; } = 0;
    }
}
