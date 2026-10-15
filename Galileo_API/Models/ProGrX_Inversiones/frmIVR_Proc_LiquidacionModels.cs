namespace Galileo.Models.ProGrX_Inversiones
{
    public sealed class IvrProcLiquidacionInicializarRequest
    {
        public int? titulo_id { get; set; }
    }

    public sealed class IvrProcLiquidacionInicialData
    {
        public IvrProcLiquidacionInversionData? inversion { get; set; }

        public IvrProcLiquidacionCalculoData liquidacion { get; set; } =
            new();

        public DateTime fecha_ultimo_corte { get; set; }

        public string divisa_local { get; set; } = string.Empty;

        public string divisa { get; set; } = string.Empty;

        public decimal tipo_cambio { get; set; } = 1;

        public decimal importe_local { get; set; } = 0;

        public decimal registrado { get; set; } = 0;

        public decimal pendiente { get; set; } = 0;

        public int transac_id { get; set; } = 0;

        public string estado { get; set; } = "S";

        public string estado_descripcion { get; set; } =
            "Solicitud";

        public List<IvrProcLiquidacionTransaccionData>
            transacciones
        { get; set; } = [];
    }

    public sealed class IvrProcLiquidacionInversionData
    {
        public int titulo_id { get; set; } = 0;

        public string administrador_desc { get; set; } =
            string.Empty;

        public string cod_administrador { get; set; } =
            string.Empty;

        public string instrumento_desc { get; set; } =
            string.Empty;

        public string cod_instrumento { get; set; } =
            string.Empty;

        public string portafolio_desc { get; set; } =
            string.Empty;

        public string cod_portafolio { get; set; } =
            string.Empty;

        public string cod_divisa { get; set; } =
            string.Empty;

        public decimal tipo_cambio { get; set; } = 0;

        public string estado { get; set; } =
            string.Empty;

        public string estado_desc { get; set; } =
            string.Empty;
    }

    public sealed class IvrProcLiquidacionCalculoData
    {
        public DateTime fecha_corte { get; set; }

        public decimal mnt_total { get; set; } = 0;

        public decimal mnt_interes_apl { get; set; } = 0;

        public decimal mnt_interes { get; set; } = 0;

        public decimal mnt_principal { get; set; } = 0;
    }

    public sealed class IvrProcLiquidacionTransaccionData
    {
        public int transac_id { get; set; } = 0;

        public string tipo_mov_desc { get; set; } =
            string.Empty;

        public DateTime? registro_fecha { get; set; }

        public decimal importe { get; set; } = 0;

        public string cta_mask { get; set; } =
            string.Empty;

        public string tipo_doc_desc { get; set; } =
            string.Empty;

        public string detalle { get; set; } =
            string.Empty;

        public int? tes_nsolicitud { get; set; }

        public string tes_estado_desc { get; set; } =
            string.Empty;

        public string cod_divisa { get; set; } =
            string.Empty;

        public decimal tipo_cambio { get; set; } = 0;

        public decimal monto { get; set; } = 0;
    }

    public sealed class IvrProcLiquidacionGuardarRequest
    {
        public int? titulo_id { get; set; }

        public int? transac_id { get; set; }

        public DateTime? fecha { get; set; }

        public string? documento { get; set; }

        public decimal? total { get; set; }

        public decimal? int_acum { get; set; }

        public decimal? intereses { get; set; }

        public decimal? principal { get; set; }

        public string? divisa { get; set; }

        public decimal? tipo_cambio { get; set; }

        public string? notas { get; set; }

        public string? usuario { get; set; }
    }

    public sealed class IvrProcLiquidacionGuardarData
    {
        public int transac_id { get; set; } = 0;

        public string estado { get; set; } =
            string.Empty;

        public string estado_descripcion { get; set; } =
            string.Empty;
    }

    public sealed class IvrProcLiquidacionTransaccionesData
    {
        public List<IvrProcLiquidacionTransaccionData>
            transacciones
        { get; set; } = [];

        public decimal registrado { get; set; } = 0;
    }

    public sealed class
        IvrProcLiquidacionTransaccionEliminarRequest
    {
        public int? transaccion_id { get; set; }

        public int? liquidacion_transac_id { get; set; }
    }
}