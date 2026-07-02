using Galileo.Models;

namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrAnulaAbonosNewConsultaData
    {
        public CrAnulaAbonosNewOperacionData? operacion { get; set; }
        public List<CrAnulaAbonosNewMovimientoData> movimientos { get; set; } = [];
        public List<DropDownListaGenericaModel> ultimas_cuotas { get; set; } = [];
    }

    public class CrAnulaAbonosNewOperacionData
    {
        public int operacion { get; set; } = 0;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string proceso { get; set; } = string.Empty;
        public string proceso_descripcion { get; set; } = string.Empty;
        public int opex { get; set; } = 0;
        public string opex_descripcion { get; set; } = string.Empty;
        public decimal saldo { get; set; } = 0;
        public decimal interes { get; set; } = 0;
        public int plazo { get; set; } = 0;
        public int prideduc { get; set; } = 0;
        public int fecult { get; set; } = 0;
        public bool retencion { get; set; } = false;
        public string base_calculo { get; set; } = string.Empty;
    }

    public class CrAnulaAbonosNewMovimientoData
    {
        public int id_seq { get; set; } = 0;
        public string fecha_proceso { get; set; } = string.Empty;
        public int num_cuota { get; set; } = 0;
        public decimal cuota { get; set; } = 0;
        public string estado { get; set; } = string.Empty;
        public decimal mov_int_cor { get; set; } = 0;
        public decimal mov_int_mor { get; set; } = 0;
        public decimal mov_principal { get; set; } = 0;
        public decimal mov_cargos { get; set; } = 0;
        public decimal mov_poliza { get; set; } = 0;
        public int dias_cor { get; set; } = 0;
        public int dias_mor { get; set; } = 0;
        public string tipo_documento { get; set; } = string.Empty;
        public string num_comprobante { get; set; } = string.Empty;
        public DateTime? mov_fecha { get; set; }
        public string mov_usuario { get; set; } = string.Empty;
    }

    public class CrAnulaAbonosNewCuentaRecomendadaRequest
    {
        public int operacion { get; set; } = 0;
        public decimal amortizacion { get; set; } = 0;
    }

    public class CrAnulaAbonosNewAplicarRequest
    {
        public int operacion { get; set; } = 0;
        public string usuario { get; set; } = string.Empty;
        public string accion { get; set; } = "S";
        public bool recalcula_cuota { get; set; } = false;
        public string ult_cta_cancelada { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public decimal int_cor { get; set; } = 0;
        public decimal int_mor { get; set; } = 0;
        public decimal amortizacion { get; set; } = 0;
        public decimal cargos { get; set; } = 0;
        public decimal poliza { get; set; } = 0;
        public List<int> id_seq_movimientos { get; set; } = [];
    }

    public class CrAnulaAbonosNewAplicarResultadoData
    {
        public string tipo_documento { get; set; } = "ND";
        public string num_documento { get; set; } = string.Empty;
        public decimal monto_total { get; set; } = 0;
        public string mensaje { get; set; } = string.Empty;
        public string? reporte_resultado { get; set; }
    }

    public class CrAnulaAbonosNewOperacionCtasData
    {
        public int id_solicitud { get; set; } = 0;
        public string Codigo { get; set; } = string.Empty;
        public decimal Saldo { get; set; } = 0;
        public string cod_Divisa { get; set; } = string.Empty;
        public string Cod_Unidad { get; set; } = string.Empty;
        public string Cod_Centro_Costo { get; set; } = string.Empty;
        public string ctaintc { get; set; } = string.Empty;
        public string ctaintm { get; set; } = string.Empty;
        public string CtaCargos { get; set; } = string.Empty;
        public string ctaamortiza { get; set; } = string.Empty;
    }
}