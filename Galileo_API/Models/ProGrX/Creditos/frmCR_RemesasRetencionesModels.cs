namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrRemesasRetencionesPantallaData
    {
        public int anio { get; set; }
        public int mes { get; set; }
        public long fecha_proceso { get; set; }
    }

    public class CrRemesasRetencionesLineaRequest
    {
        public string codigo { get; set; } = string.Empty;
        public int plazo { get; set; }
        public decimal cuota { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
    }

    public class CrRemesasRetencionesValidarRequest
    {
        public List<CrRemesasRetencionesLineaRequest> lineas { get; set; } = new();
    }

    public class CrRemesasRetencionesAplicarRequest : CrRemesasRetencionesValidarRequest
    {
        public required int anio { get; set; }
        public required int mes { get; set; }
    }

    public class CrRemesasRetencionesLineaData : CrRemesasRetencionesLineaRequest
    {
        public int inconsistencia { get; set; }
        public string detalle_inc { get; set; } = string.Empty;
    }

    public class CrRemesasRetencionesTotalesData
    {
        public int casos_apl { get; set; }
        public decimal saldo_apl { get; set; }
        public decimal cuota_apl { get; set; }
        public int casos_inc { get; set; }
        public decimal saldo_inc { get; set; }
        public decimal cuota_inc { get; set; }
    }

    public class CrRemesasRetencionesValidarData
    {
        public List<CrRemesasRetencionesLineaData> detalle { get; set; } = new();
        public CrRemesasRetencionesTotalesData totales { get; set; } = new();
    }
}
