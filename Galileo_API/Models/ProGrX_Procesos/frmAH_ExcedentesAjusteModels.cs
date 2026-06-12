namespace Galileo.Models.AH
{
    public class FrmAhExcedentesAjusteCargarResponse
    {
        public List<ExcPeriodosDto> periodos { get; set; } = [];
        public List<FrmAhExcedentesAjustePendienteDto> pendientes { get; set; } = [];
        public FrmAhExcedentesAjusteResumenDto resumen { get; set; } = new();
        public int lineas { get; set; } = 100;
    }

    public class FrmAhExcedentesAjustePendienteListaRequest
    {
        public string filtro { get; set; } = string.Empty;
        public int lineas { get; set; } = 100;
    }

    public class FrmAhExcedentesAjustePendienteDto
    {
        public int ajuste_id { get; set; } = 0;
        public string estado { get; set; } = string.Empty;
        public string estado_desc { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal ajuste { get; set; } = 0;
        public string detalle { get; set; } = string.Empty;
        public int id_periodo { get; set; } = 0;
        public string periodo_desc { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
    }

    public class FrmAhExcedentesAjusteResumenDto
    {
        public int casos { get; set; } = 0;
        public decimal ajuste_positivo { get; set; } = 0;
        public decimal ajuste_negativo { get; set; } = 0;
    }

    public class FrmAhExcedentesAjusteCedulaDto
    {
        public bool socio_valido { get; set; } = false;
        public bool existe_ajuste { get; set; } = false;
        public int ajuste_id { get; set; } = 0;
        public string estado { get; set; } = string.Empty;
        public string estado_desc { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal ajuste { get; set; } = 0;
        public string detalle { get; set; } = string.Empty;
        public int id_periodo { get; set; } = 0;
        public string periodo_desc { get; set; } = string.Empty;
    }

    public class FrmAhExcedentesAjusteGuardarRequest
    {
        public int id_periodo { get; set; } = 0;
        public string cedula { get; set; } = string.Empty;
        public decimal ajuste { get; set; } = 0;
        public string detalle { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    public class FrmAhExcedentesAjusteProcesoResponse
    {
        public int aplicado { get; set; } = 0;
        public int ajuste_id { get; set; } = 0;
        public string mensaje { get; set; } = string.Empty;
    }

    public class FrmAhExcedentesAjusteSpResult
    {
        public int Aplicado { get; set; } = 0;
        public int Ajuste_Id { get; set; } = 0;
        public string Mensaje { get; set; } = string.Empty;
    }

    public class FrmAhExcedentesAjusteSocioInternoDto
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
    }

    public class FrmAhExcedentesAjusteDeleteInternoDto
    {
        public string cedula { get; set; } = string.Empty;
        public decimal ajuste { get; set; } = 0;
    }
}
