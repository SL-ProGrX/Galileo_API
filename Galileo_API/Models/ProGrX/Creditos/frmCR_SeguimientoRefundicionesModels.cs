namespace Galileo_API.Models.ProGrX.Creditos;

public class CrSeguimientoRefundicionesInicializarRequest
{
    public long? operacion { get; set; }
    public DateTime? fecha_desembolso { get; set; }
    public decimal? pri_deduc { get; set; }
    public int? dia_pago { get; set; }
}

public class CrSeguimientoRefundicionesInicializarDto
{
    public string cedula { get; set; } = string.Empty;
    public string codigo { get; set; } = string.Empty;
    public decimal disponible { get; set; }
    public decimal primer_cuota { get; set; }
    public decimal poliza { get; set; }
    public decimal interes { get; set; }

    public CrSeguimientoRefundicionesListaDto refundiciones { get; set; } = new();
    public CrSeguimientoRefundicionesCreditosListaDto prestamos { get; set; } = new();
}

public class CrSeguimientoRefundicionesListaDto
{
    public int total { get; set; }
    public List<CrSeguimientoRefundicionData> lista { get; set; } = new List<CrSeguimientoRefundicionData>();
}

public class CrSeguimientoRefundicionesCreditosListaDto
{
    public int total { get; set; }
    public List<CrSeguimientoRefundicionCreditoData> lista { get; set; } = new List<CrSeguimientoRefundicionCreditoData>();
}

public class CrSeguimientoRefundicionesListaRequest
{
    public long? operacion { get; set; }
    public string filtros { get; set; } = string.Empty;
}

public class CrSeguimientoRefundicionesPrestamosRequest
{
    public long? operacion { get; set; }
    public string cedula { get; set; } = string.Empty;
    public string codigo { get; set; } = string.Empty;
    public string filtros { get; set; } = string.Empty;
}

public class CrSeguimientoRefundicionesConsultaTercerosRequest
{
    public string cedula { get; set; } = string.Empty;
    public string codigo { get; set; } = string.Empty;
    public string filtros { get; set; } = string.Empty;
}

public class CrSeguimientoRefundicionData
{
    public long id_solicitud { get; set; }
    public string codigo { get; set; } = string.Empty;
    public string garantiax { get; set; } = string.Empty;
    public string descripcion { get; set; } = string.Empty;
    public string tipo { get; set; } = string.Empty;
    public string tipo_desc { get; set; } = string.Empty;

    public decimal? saldo_anterior { get; set; }
    public decimal? intcor { get; set; }
    public decimal? intmor { get; set; }
    public decimal? cargos { get; set; }
    public decimal? polizas { get; set; }
    public decimal? principal { get; set; }
    public decimal? monto { get; set; }
    public decimal? iva { get; set; }
}

public class CrSeguimientoRefundicionCreditoData
{
    public long id_solicitud { get; set; }
    public string codigo { get; set; } = string.Empty;
    public string garantiax { get; set; } = string.Empty;
    public string descripcion { get; set; } = string.Empty;
    public string tipo { get; set; } = string.Empty;
    public string tipo_desc { get; set; } = string.Empty;

    public decimal? saldo { get; set; }
    public decimal? intc { get; set; }
    public decimal? intm { get; set; }
    public decimal? amortiza { get; set; }
    public decimal? cargos { get; set; }
    public decimal? polizas { get; set; }
    public decimal? iva { get; set; }
    public decimal? total { get; set; }
}

public class CrSeguimientoRefundicionDatosDto
{
    public long id_solicitud { get; set; }
    public string codigo { get; set; } = string.Empty;
    public string linea_desc { get; set; } = string.Empty;
    public string garantia { get; set; } = string.Empty;
    public string garantia_des { get; set; } = string.Empty;
    public string tipo { get; set; } = string.Empty;

    public decimal? saldo { get; set; }
    public decimal? intcor { get; set; }
    public decimal? intmor { get; set; }
    public decimal? principal { get; set; }
    public decimal? cargos { get; set; }
    public decimal? polizas { get; set; }
    public decimal? cuota { get; set; }
    public decimal? iva { get; set; }
    public decimal? total { get; set; }
}

public class CrSeguimientoRefundicionGuardarRequest
{
    public long? operacion_refunde { get; set; }
    public string codigo_refunde { get; set; } = string.Empty;

    public long? operacion_nueva { get; set; }
    public string codigo_nuevo { get; set; } = string.Empty;

    public decimal? disponible { get; set; }

    public decimal? saldo { get; set; }
    public decimal? intcor { get; set; }
    public decimal? intmor { get; set; }
    public decimal? principal { get; set; }
    public decimal? cargos { get; set; }
    public decimal? polizas { get; set; }
    public decimal? iva { get; set; }
    public decimal? total { get; set; }

    public string tipo { get; set; } = "C";
}

public class CrSeguimientoRefundicionesEliminarRequest
{
    public long? operacion_nueva { get; set; }
    public List<long?> operaciones_refunde { get; set; } = new List<long?>();
}

public class CrSeguimientoRefundicionesActualizarRequest
{
    public long? operacion { get; set; }
}

public class CrSeguimientoRefundicionesRefundeDatosRequest
{
    public long? operacion { get; set; }
    public string tipo { get; set; } = "C";
}

public class CrSeguimientoRefundicionesOperacionBaseDto
{
    public string primer_cuota { get; set; } = string.Empty;
    public string garantia { get; set; } = string.Empty;

    public decimal? montoapr { get; set; }
    public decimal? cuota { get; set; }
    public decimal? int_credito { get; set; }

    public string convenio { get; set; } = string.Empty;
    public string cod_destino { get; set; } = string.Empty;

    public string cedula { get; set; } = string.Empty;
    public string codigo { get; set; } = string.Empty;
    public DateTime? fecha_desembolso { get; set; }
    public decimal? pri_deduc { get; set; }
    public int? dia_pago { get; set; }
}