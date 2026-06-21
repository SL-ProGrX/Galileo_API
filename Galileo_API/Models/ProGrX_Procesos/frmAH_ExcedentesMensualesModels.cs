namespace Galileo.Models.AH
{
    public class ExcParametrosDto
    {
        public string Cod_Parametro { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Valor { get; set; } = string.Empty;
    }

    public class ExcPeriodosDto
    {
        public string idx { get; set; } = string.Empty;
        public string itmx { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public bool modo_automatico { get; set; } = false;
    }

    public class ResumenExcedenteMDto
    {
        public string id_periodo { get; set; } = string.Empty;
        public string corte { get; set; } = string.Empty;
        public string corte_date_str { get; set; } = string.Empty;
        public string corte_datetime_str { get; set; } = string.Empty;
        public string casos { get; set; } = string.Empty;
        public string total { get; set; } = string.Empty;
        public string bruto { get; set; } = string.Empty;
    }

    public class FrmAhExcedentesMensualesBaseAplicacionRequest
    {
        public int periodoId { get; set; } = 0;
        public string tipoAplicacion { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    public class FrmAhExcedentesMensualesMensualAplicarRequest
    {
        public int periodoId { get; set; } = 0;
        public DateTime corte { get; set; } = DateTime.MinValue;
        public decimal monto { get; set; } = 0;
        public string tipoAplicacion { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    public class FrmAhExcedentesMensualesCierreAplicarRequest
    {
        public int periodoId { get; set; } = 0;
        public string usuario { get; set; } = string.Empty;
    }

    public class FrmAhExcedentesMensualesSalidasSeparaRequest
    {
        public int periodoId { get; set; } = 0;
        public string usuario { get; set; } = string.Empty;
    }

    public class FrmAhExcedentesMensualesSalidasFondosRequest
    {
        public int periodoId { get; set; } = 0;
        public string salida { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    public class FrmAhExcedentesMensualesBitacoraRegistrarRequest
    {
        public int periodoId { get; set; } = 0;
        public string codProceso { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string tipoDocumento { get; set; } = string.Empty;
        public string codTransaccion { get; set; } = string.Empty;
    }

    public class FrmAhExcedentesMensualesMensualPeriodoDto
    {
        public string estado { get; set; } = string.Empty;
        public string tipo_apl_mensual { get; set; } = string.Empty;
        public string tipo_apl_mensual_desc { get; set; } = string.Empty;
    }

    public class FrmAhExcedentesMensualesMensualResultadoDto
    {
        public int casos_general { get; set; } = 0;
        public decimal total_ahorros { get; set; } = 0;
        public decimal total_aportes { get; set; } = 0;
        public decimal factor { get; set; } = 0;
        public decimal total_distribuido { get; set; } = 0;
        public int casos_proceso { get; set; } = 0;
    }

    public class FrmAhExcedentesMensualesAplicacionProcesoRequest
    {
        public int periodoId { get; set; } = 0;
        public string procesoId { get; set; } = string.Empty;
        public bool limpiaAplicacionAnterior { get; set; } = false;
        public bool cargaInfoCero { get; set; } = false;
        public string salida { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    public class FrmAhExcedentesMensualesAplicacionProcesoResponse
    {
        public string mensaje { get; set; } = string.Empty;
        public List<FrmAhExcedentesMensualesAplicacionDocumentoDto> documentos { get; set; } = [];
    }

    public class FrmAhExcedentesMensualesAplicacionDocumentoDto
    {
        public string tipo_documento { get; set; } = string.Empty;
        public string cod_transaccion { get; set; } = string.Empty;
    }

  

}
