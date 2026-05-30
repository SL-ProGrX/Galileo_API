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
        public int periodoId { get; set; }
        public string tipoAplicacion { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    public class FrmAhExcedentesMensualesMensualAplicarRequest
    {
        public int periodoId { get; set; }
        public DateTime corte { get; set; }
        public decimal monto { get; set; }
        public string tipoAplicacion { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    public class FrmAhExcedentesMensualesCierreAplicarRequest
    {
        public int periodoId { get; set; }
        public string usuario { get; set; } = string.Empty;
    }

    public class FrmAhExcedentesMensualesSalidasSeparaRequest
    {
        public int periodoId { get; set; }
        public string usuario { get; set; } = string.Empty;
    }

    public class FrmAhExcedentesMensualesSalidasFondosRequest
    {
        public int periodoId { get; set; }
        public string salida { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    public class FrmAhExcedentesMensualesBitacoraRegistrarRequest
    {
        public int periodoId { get; set; }
        public string codProceso { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string tipoDocumento { get; set; } = string.Empty;
        public string codTransaccion { get; set; } = string.Empty;
    }
}
