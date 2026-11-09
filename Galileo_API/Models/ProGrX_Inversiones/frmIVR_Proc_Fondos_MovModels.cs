namespace Galileo.Models.ProGrX_Inversiones;

public sealed class IvrProcFondosMovInicialData
{
    public IvrProcFondosMovInversionData? inversion { get; set; }
    public DateTime? fecha_ultimo_corte { get; set; }
    public DateTime? fecha_movimiento { get; set; }
    public string divisa_local { get; set; } = string.Empty;
    public string divisa { get; set; } = string.Empty;
    public decimal tipo_cambio { get; set; } = 1;
    public string concepto { get; set; } = string.Empty;
    public string tipo_mov { get; set; } = string.Empty;
    public string estado { get; set; } = "S";
}

public sealed class IvrProcFondosMovInversionData
{
    public int titulo_id { get; set; } = 0;
    public string administrador_desc { get; set; } = string.Empty;
    public string cod_administrador { get; set; } = string.Empty;
    public string instrumento_desc { get; set; } = string.Empty;
    public string cod_instrumento { get; set; } = string.Empty;
    public string portafolio_desc { get; set; } = string.Empty;
    public string cod_portafolio { get; set; } = string.Empty;
    public string cod_divisa { get; set; } = string.Empty;
    public decimal monto_principal { get; set; } = 0;
    public decimal participacion_numero { get; set; } = 0;
    public decimal participacion_valor { get; set; } = 0;
}

public sealed class IvrProcFondosMovMovimientoData
{
    public int fi_transac { get; set; } = 0;
    public int seq_id { get; set; } = 0;
    public string estado { get; set; } = string.Empty;
    public string tipo_mov { get; set; } = string.Empty;
    public string documento { get; set; } = string.Empty;
    public DateTime? fecha { get; set; }
    public decimal mnt_principal { get; set; } = 0;
    public decimal n_participaciones { get; set; } = 0;
    public decimal v_participaciones { get; set; } = 0;
    public string notas { get; set; } = string.Empty;
}

public sealed class IvrProcFondosMovGuardarRequest
{
    public int titulo_id { get; set; } = 0;
    public int transac_id { get; set; } = 0;
    public int seq_id { get; set; } = 0;
    public string concepto { get; set; } = string.Empty;
    public DateTime? fecha { get; set; }
    public string documento { get; set; } = string.Empty;
    public decimal monto { get; set; } = 0;
    public decimal n_participaciones { get; set; } = 0;
    public decimal v_participaciones { get; set; } = 0;
    public string divisa { get; set; } = string.Empty;
    public decimal tipo_cambio { get; set; } = 0;
    public string notas { get; set; } = string.Empty;
    public string usuario { get; set; } = string.Empty;
}

public sealed class IvrProcFondosMovGuardarData
{
    public int transac_id { get; set; } = 0;
    public int seq_id { get; set; } = 0;
    public string estado { get; set; } = string.Empty;
}

public sealed class IvrProcFondosMovTransaccionesData
{
    public List<IvrProcCuponesTransaccionData> transacciones { get; set; } = [];
    public decimal registrado { get; set; } = 0;
}