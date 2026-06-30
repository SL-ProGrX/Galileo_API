namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrSeguimientoDesembolsosInicializarDto
    {
        public long operacion { get; set; }
        public decimal monto_aprobado { get; set; }
        public decimal monto_registrado { get; set; }
        public decimal monto_disponible { get; set; }
        public decimal primer_cuota { get; set; }
        public decimal poliza { get; set; }
        public decimal interes { get; set; }

        public List<CrSeguimientoDesembolsosBancoDto> bancos { get; set; } = new();
        public List<CrSeguimientoDesembolsosTipoIdDto> tipos_id { get; set; } = new();
        public List<CrSeguimientoDesembolsosDivisaDto> divisas { get; set; } = new();
        public List<CrSeguimientoDesembolsosData> desembolsos { get; set; } = new();
    }

    public class CrSeguimientoDesembolsosData
    {
        public long id_desembolso { get; set; }
        public long id_solicitud { get; set; }
        public string codigo { get; set; } = string.Empty;
        public string concepto { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public string cuenta_conta { get; set; } = string.Empty;
        public string cuenta_conta_mask { get; set; } = string.Empty;
        public string cuenta_desc { get; set; } = string.Empty;

        public string tdocumento { get; set; } = string.Empty;
        public string tipo_documento_desc { get; set; } = string.Empty;

        public int depositar { get; set; }
        public int banco { get; set; }
        public string banco_desc { get; set; } = string.Empty;

        public int retener { get; set; }
        public int modifica { get; set; }
        public int diferido_aplica { get; set; }
        public DateTime? diferido_corte { get; set; }

        public string referencia { get; set; } = string.Empty;
        public string identificacion { get; set; } = string.Empty;
        public string cta_banco { get; set; } = string.Empty;

        public int tipo_ced_destino { get; set; }
        public string tipo_id_desc { get; set; } = string.Empty;

        public string cedula_destino { get; set; } = string.Empty;
        public string id_banco_destino { get; set; } = string.Empty;
        public string cta_iban_destino { get; set; } = string.Empty;

        public string cod_divisa { get; set; } = string.Empty;
        public string divisa_desc { get; set; } = string.Empty;

        public string correo_notifica { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
    }

    public class CrSeguimientoDesembolsosGuardarRequest
    {
        public long? id_desembolso { get; set; }
        public long? id_solicitud { get; set; }
        public string codigo { get; set; } = string.Empty;
        public string concepto { get; set; } = string.Empty;
        public decimal? monto { get; set; }
        public string cuenta_conta { get; set; } = string.Empty;

        public string tdocumento { get; set; } = string.Empty;
        public int? depositar { get; set; }
        public int? cod_banco { get; set; }

        public int? retener { get; set; }
        public int? modifica { get; set; }
        public int? diferido_aplica { get; set; }
        public DateTime? diferido_corte { get; set; }

        public string referencia { get; set; } = string.Empty;
        public string identificacion { get; set; } = string.Empty;
        public string cta_banco { get; set; } = string.Empty;

        public int? tipo_ced_destino { get; set; }
        public string cedula_destino { get; set; } = string.Empty;
        public string id_banco_destino { get; set; } = string.Empty;
        public string cta_iban_destino { get; set; } = string.Empty;

        public string cod_divisa { get; set; } = string.Empty;
        public string correo_notifica { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;

        public string usuario { get; set; } = string.Empty;
    }

    public class CrSeguimientoDesembolsosEliminarRequest
    {
        public long? id_desembolso { get; set; }
        public long? id_solicitud { get; set; }
        public string usuario { get; set; } = string.Empty;
    }

    public class CrSeguimientoDesembolsosConceptoDto
    {
        public int cod_condeb { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public int retiene { get; set; }
        public int modifica { get; set; }
        public string cod_cuenta { get; set; } = string.Empty;
        public string cod_cuenta_mask { get; set; } = string.Empty;
        public string cuenta_desc { get; set; } = string.Empty;
        public int difiere { get; set; }
        public DateTime difiere_fecha { get; set; }
    }

    public class CrSeguimientoDesembolsosBancoDto
    {
        public int id_banco { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public string desc_corta { get; set; } = string.Empty;
        public string cta { get; set; } = string.Empty;
        public string cod_divisa { get; set; } = string.Empty;
        public string idx { get; set; } = string.Empty;
        public string itmx { get; set; } = string.Empty;
    }

    public class CrSeguimientoDesembolsosCuentaBancariaDto
    {
        public string cuenta_interna { get; set; } = string.Empty;
        public string cuenta_desc { get; set; } = string.Empty;
        public string idx { get; set; } = string.Empty;
        public string itmx { get; set; } = string.Empty;
        public int prioridad { get; set; }
    }

    public class CrSeguimientoDesembolsosTipoIdDto
    {
        public int idx { get; set; }
        public string itmx { get; set; } = string.Empty;
    }

    public class CrSeguimientoDesembolsosDivisaDto
    {
        public string idx { get; set; } = string.Empty;
        public string itmx { get; set; } = string.Empty;
    }

    public class CrSeguimientoDesembolsosOperacionDto
    {
        public long id_solicitud { get; set; }
        public string codigo { get; set; } = string.Empty;
        public string cod_destino { get; set; } = string.Empty;
        public decimal montoapr { get; set; }
        public decimal prideduc { get; set; }
        public int dia_pago { get; set; }
        public decimal interesv { get; set; }
        public decimal tasa_int { get; set; }
        public DateTime? fecha_inicio_calculo { get; set; }
        public DateTime? fechaforp { get; set; }
    }

    public class CrSeguimientoDesembolsosResumenDto
    {
        public decimal monto_aprobado { get; set; }
        public decimal monto_registrado { get; set; }
        public decimal monto_disponible { get; set; }
        public decimal primer_cuota { get; set; }
        public decimal poliza { get; set; }
        public decimal interes { get; set; }
    }
}