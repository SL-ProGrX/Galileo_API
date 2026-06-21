namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrAnulaAbonosConsultaResponse
    {
        public CrAnulaAbonosOperacionData? operacion { get; set; }
        public List<CrAnulaAbonosMovimientoData> movimientos { get; set; } = [];
        public List<CrAnulaAbonosUltimaCuotaData> ultimas_cuotas { get; set; } = [];
    }

    public class CrAnulaAbonosOperacionData
    {
        public int id_solicitud { get; set; }
        public decimal saldo { get; set; }
        public string proceso { get; set; } = string.Empty;
        public string proceso_desc { get; set; } = string.Empty;
        public decimal interes { get; set; }
        public int plazo { get; set; }
        public int prideduc { get; set; }
        public int fecult { get; set; }
        public bool opex { get; set; }
        public string opex_desc { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool retencion { get; set; }
        public string base_calculo { get; set; } = string.Empty;
    }

    public class CrAnulaAbonosMovimientoData
    {
        public int id_seq { get; set; }
        public int num_cuota { get; set; }
        public int fecha_proceso { get; set; }
        public decimal cuota { get; set; }
        public string estado { get; set; } = string.Empty;
        public decimal mov_intcor { get; set; }
        public decimal mov_intmor { get; set; }
        public decimal mov_principal { get; set; }
        public decimal mov_cargos { get; set; }
        public decimal mov_poliza { get; set; }
        public int dias_calculo { get; set; }
        public int mora_dias { get; set; }
        public string tipo_documento { get; set; } = string.Empty;
        public string num_comprobante { get; set; } = string.Empty;
        public DateTime? mov_fecha { get; set; }
        public string mov_usuario { get; set; } = string.Empty;
    }

    public class CrAnulaAbonosUltimaCuotaData
    {
        public int fecha_proceso { get; set; }
        public string descripcion { get; set; } = string.Empty;
    }

    public class CrAnulaAbonosCuentaRecomendadaRequest
    {
        [System.Text.Json.Serialization.JsonRequired]
        public int id_solicitud { get; set; }

        [System.Text.Json.Serialization.JsonRequired]
        public decimal monto_amortizacion { get; set; }
    }

    public class CrAnulaAbonosProcesarRequest
    {
        [System.Text.Json.Serialization.JsonRequired]
        public int id_solicitud { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string accion { get; set; } = "S";

        [System.Text.Json.Serialization.JsonRequired]
        public bool recalcular_cuota { get; set; }

        [System.Text.Json.Serialization.JsonRequired]
        public int ultima_cuota_cancelada { get; set; }
        public string notas { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonRequired]
        public decimal int_corriente { get; set; }

        [System.Text.Json.Serialization.JsonRequired]
        public decimal int_morosidad { get; set; }

        [System.Text.Json.Serialization.JsonRequired]
        public decimal amortizacion { get; set; }

        [System.Text.Json.Serialization.JsonRequired]
        public decimal cargos { get; set; }

        [System.Text.Json.Serialization.JsonRequired]
        public decimal poliza { get; set; }
        public List<int> id_seq_movimientos { get; set; } = [];
    }

    public class CrAnulaAbonosProcesarResponse
    {
        public string tipo_documento { get; set; } = "ND";
        public string num_documento { get; set; } = string.Empty;
        public decimal monto_total { get; set; }
        public string mensaje { get; set; } = string.Empty;
        public string? reporte_resultado { get; set; }
    }

    public class CrAnulaAbonosOperacionCtasData
    {
        public int ID_SOLICITUD { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public decimal Saldo { get; set; }
        public string cod_Divisa { get; set; } = string.Empty;
        public string Cod_Unidad { get; set; } = string.Empty;
        public string Cod_Centro_Costo { get; set; } = string.Empty;
        public string ctaintc { get; set; } = string.Empty;
        public string ctaintm { get; set; } = string.Empty;
        public string CtaCargos { get; set; } = string.Empty;
        public string ctaamortiza { get; set; } = string.Empty;
    }
}
