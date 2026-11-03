namespace Galileo.Models.ProGrX_Inversiones;

public class IvrRecBancosRegistroContextoRequest
{
    public int titulo_id { get; set; } = 0;
    public int ref_codigo { get; set; } = 0;
    public string ref_tipo { get; set; } = string.Empty;
    public string ref_concepto { get; set; } = string.Empty;
    public string tipo_mov { get; set; } = string.Empty;
    public string divisa { get; set; } = string.Empty;
    public decimal tipo_cambio { get; set; } = 1;
    public decimal monto_requerido { get; set; } = 0;
}

public sealed class IvrRecBancosRegistroGuardarRequest : IvrRecBancosRegistroContextoRequest
{
    public string cod_cuenta { get; set; } = string.Empty;
    public decimal monto { get; set; } = 0;
    public string notas { get; set; } = string.Empty;
    public string usuario { get; set; } = string.Empty;
}

public sealed class IvrRecBancosRegistroEliminarRequest : IvrRecBancosRegistroContextoRequest
{
    public List<int> transacciones { get; set; } = [];
}

public sealed class IvrRecBancosRegistroCuentaRequest
{
    public string cod_cuenta { get; set; } = string.Empty;
}

public sealed class IvrRecBancosRegistroEncabezadoData
{
    public int titulo_id { get; set; } = 0;
    public string administrador_desc { get; set; } = string.Empty;
    public string instrumento_desc { get; set; } = string.Empty;
    public string portafolio_desc { get; set; } = string.Empty;
    public string cta_transito { get; set; } = string.Empty;
    public string cta_transito_desc { get; set; } = string.Empty;
    public string cta_transito_divisa { get; set; } = string.Empty;
}

public sealed class IvrRecBancosRegistroInicialData
{
    public IvrRecBancosRegistroEncabezadoData inversion { get; set; } = new();
    public string divisa_local { get; set; } = string.Empty;
    public int cod_contabilidad { get; set; } = 0;
    public IvrRecBancosRegistroListaData movimientos { get; set; } = new();
}

public sealed class IvrRecBancosRegistroListaData
{
    public List<IvrProcCuponesTransaccionData> transacciones { get; set; } = [];
    public decimal requerido { get; set; } = 0;
    public decimal registrado { get; set; } = 0;
    public decimal pendiente { get; set; } = 0;
}

public sealed class IvrRecBancosRegistroGuardarData
{
    public int transac_id { get; set; } = 0;
}

