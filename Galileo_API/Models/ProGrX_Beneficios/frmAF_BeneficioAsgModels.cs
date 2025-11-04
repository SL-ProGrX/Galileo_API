namespace PgxAPI.Models.AF
{
    public class AfiBeneOtorgaAsgDataList
    {
        public int total { get; set; }
        public List<AfiBeneOtorgaData> beneficios { get; set; } = new List<AfiBeneOtorgaData>();
    }

    public class AfiBeneOtorgaData
    {
        public int? consec { get; set; }
        public string cod_beneficio { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public float monto { get; set; }
        public float monto_aplicado { get; set; }
        public string modifica_monto { get; set; } = string.Empty;
        public DateTime registra_fecha { get; set; }
        public string registra_user { get; set; } = string.Empty;
        public string autoriza_user { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public Nullable<DateTime> autoriza_fecha { get; set; } // Nullable in case the date is not set
        public string? notas { get; set; } = string.Empty;
        public string solicita { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string analista_revision { get; set; } = string.Empty;
        public string analista_recepcion { get; set; } = string.Empty;
        public string cod_remesa { get; set; } = string.Empty;
        public string cod_oficina { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string sNombre { get; set; } = string.Empty;
        public string? cod_banco { get; set; }
        public string? tipo_emision { get; set; }
        public string? cta_bancaria { get; set; }
        public float MontoTag { get; set; } = 0;
        public bool? pagos_multiples { get; set; }
        public bool? aplica_pago_masivo { get; set; }
    }

    public class AfiBeneDto
    {
        public string cod_beneficio { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public DateTime registra_fecha { get; set; }
        public string registra_user { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public int? consecutivo { get; set; } // Nullable in case the value is not set
        public decimal? monto { get; set; } // Nullable in case the value is not set
        public decimal? maximo_otorga { get; set; } // Nullable in case the value is not set
        public bool? modifica_monto { get; set; } // Nullable in case the value is not set
        public decimal? modifica_diferencia { get; set; } // Nullable in case the value is not set
        public string cod_cuenta { get; set; } = string.Empty;
        public bool? aplica_beneficiarios { get; set; } // Nullable in case the value is not set
        public bool? aplica_parcial { get; set; } // Nullable in case the value is not set
        public int tipo_producto { get; set; }
        public int tipo_monetario { get; set; }
    }

    public class FxMontoModel
    {
        public string cedula { get; set; } = string.Empty;
        public string cod_beneficio { get; set; } = string.Empty;
        public int iBeneficiario { get; set; }
        public string solicita { get; set; } = string.Empty;
        public float monto { get; set; }
        public bool bConsulta { get; set; }
        public bool bNuevo { get; set; }
        public bool bAsignado { get; set; }
        public int iGrupo { get; set; } = 0;
        public float cMontoRealGrupo { get; set; }
    }

    public class FxMontosResult
    {
        public int Code { get; set; }
        public string Description { get; set; } = string.Empty;
        public float monto { get; set; }
        public float disponible { get; set; }
        public float montoGira { get; set; }
    }

    public class CuentaListaData
    {
        public string cuenta_interna { get; set; } = string.Empty;
        public string cuenta_desc { get; set; } = string.Empty;
        public string idx { get; set; } = string.Empty;
        public string itmx { get; set; } = string.Empty;
        public int prioridad { get; set; }
    }

    public class AfiBeneficioPago
    {
        public string cedula { get; set; } = string.Empty;
        public int consec { get; set; }
        public string cod_beneficio { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public string cod_banco { get; set; } = string.Empty;
        public string tipo_emision { get; set; } = string.Empty;
        public string cta_bancaria { get; set; } = string.Empty;
        public int tesoreria { get; set; }
        public string estado { get; set; } = string.Empty;
        public string envio_user { get; set; } = string.Empty;
        public string envio_fecha { get; set; } = string.Empty;
        public string tes_supervision_usuario { get; set; } = string.Empty;
        public string tes_supervision_fecha { get; set; } = string.Empty;
        public string id_token { get; set; } = string.Empty;
        public string bancoDesc { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string cod_producto { get; set; } = string.Empty;
        public int cantidad { get; set; }
        public float costo_unidad { get; set; }
        public string prodDesc { get; set; } = string.Empty;
        public string prodCu { get; set; } = string.Empty;

    }

    public class AfiBeneMontoData
    {
        public int cantidad { get; set; }
        public float monto { get; set; }
    }

    public class AsientoContableData
    {
        public string fxcuentabanco { get; set; } = string.Empty;
        public string fxDescripcion { get; set; } = string.Empty;
        public string fxDescribe { get; set; } = string.Empty;
        public string fxcuenta { get; set; } = string.Empty;
        public float fxmonto { get; set; }
        public float fxmontobene { get; set; }
    }

    public class CuentasBancariasModels
    {
        public string cuenta { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class AfiBeneficioAsgInsertar
    {
        public string? cedula { get; set; }
        public string? cod_beneficio { get; set; }
        public string? tipoBeneficio { get; set; }
        public string? estado { get; set; }
        public string? solicita { get; set; }
        public string? solicita_nombre { get; set; }
        public string? notas { get; set; }
        public float? monto { get; set; } = 0;
        public float? disponible { get; set; } = 0;
        public string? emitir { get; set; }
        public float? montoGira { get; set; } = 0;
        public string? cod_banco { get; set; }
        public string? cod_cuenta { get; set; }
        public List<AfBeneAsgProductoData>? productos { get; set; }
        //parametros
        public string? strConsulta { get; set; }
        public bool? bNuevo { get; set; }
        public string? txtBeneficioId { get; set; }
        //otros parametros
        public int? consec { get; set; } = 0;



    }
  
    public class AfBeneAsgProductoData
    {
        public string cod_producto { get; set; } = string.Empty;
        public float cantidad { get; set; }
        public float costo_unidad { get; set; }
    }
}