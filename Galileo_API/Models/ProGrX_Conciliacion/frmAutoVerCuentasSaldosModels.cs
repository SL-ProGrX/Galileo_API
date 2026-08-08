namespace Galileo_API.Models.ProGrX_Conciliacion
{
    public abstract class AutoVerPeriodoBase
    {
        public int anio { get; set; } = 0;
        public int mes { get; set; } = 0;
    }

    public abstract class AutoVerCuentaBase :
        AutoVerPeriodoBase
    {
        public string cuenta { get; set; } = string.Empty;
    }

    public sealed class AutoVerCuentasSaldosPeriodoData :
        AutoVerPeriodoBase
    {
        public int id_per_historico { get; set; } = 0;
        public string descripcion { get; set; } = string.Empty;
    }

    public sealed class AutoVerCuentasSaldosAuxiliarData
    {
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public sealed class AutoVerCuentasSaldosPantallaData
    {
        public List<AutoVerCuentasSaldosPeriodoData> periodos
        {
            get;
            set;
        } = [];

        public List<AutoVerCuentasSaldosAuxiliarData> auxiliares
        {
            get;
            set;
        } = [];
    }

    public sealed class AutoVerCuentasSaldosResumenQuery :
     AutoVerPeriodoBase
    {
        public string auxiliar { get; set; } =
            string.Empty;
    }

    public class AutoVerCuentasSaldosPeriodoQuery :
        AutoVerPeriodoBase
    {
    }

    public class AutoVerCuentasSaldosCuentaQuery :
        AutoVerCuentaBase
    {
    }

    public sealed class AutoVerCuentasSaldosAnaliticoQuery :
        AutoVerCuentaBase
    {
        public string origen { get; set; } = "C";
    }

    public sealed class AutoVerCuentasSaldosConciliaQuery :
        AutoVerCuentaBase
    {
        public string tipo_movimiento { get; set; } = "D";
    }

    public sealed class AutoVerCuentasSaldosResumenData :
        AutoVerPeriodoBase
    {
        public string cod_cuenta_mask { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal saldo { get; set; } = 0;
        public decimal saldo_contable { get; set; } = 0;
        public decimal diferencia { get; set; } = 0;
        public int operaciones { get; set; } = 0;
        public string currency_sim { get; set; } = string.Empty;
        public string divisa_desc { get; set; } = string.Empty;
    }

    public sealed class AutoVerCuentasSaldosTotalesData
    {
        public decimal saldo { get; set; } = 0;
        public decimal saldo_contable { get; set; } = 0;
        public decimal diferencia { get; set; } = 0;
        public int operaciones { get; set; } = 0;
    }

    public sealed class AutoVerCuentasSaldosResumenResult :
        AutoVerPeriodoBase
    {
        public List<AutoVerCuentasSaldosResumenData> datos
        {
            get;
            set;
        } = [];

        public AutoVerCuentasSaldosTotalesData totales
        {
            get;
            set;
        } = new();
    }

    public sealed class AutoVerCuentasSaldosTendenciaData :
        AutoVerPeriodoBase
    {
        public string cod_cuenta_mask { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal saldo { get; set; } = 0;
        public decimal saldo_contable { get; set; } = 0;
        public decimal diferencia { get; set; } = 0;
        public int operaciones { get; set; } = 0;
    }

    public sealed class AutoVerCuentasSaldosAsignacionData :
        AutoVerPeriodoBase
    {
        public string modulo { get; set; } = string.Empty;
        public string localizacion { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public sealed class AutoVerCuentasSaldosFormaPagoData
    {
        public string tipo_documento { get; set; } = string.Empty;
        public long cod_transaccion { get; set; } = 0;
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public decimal monto { get; set; } = 0;
        public string descripcion { get; set; } = string.Empty;
        public decimal tipo_cambio { get; set; } = 0;
        public string num_referencia { get; set; } = string.Empty;
        public string afecta_auxiliar { get; set; } = string.Empty;
        public string cta_cod { get; set; } = string.Empty;
        public string cta_desc { get; set; } = string.Empty;
        public string cliente_identificacion { get; set; } = string.Empty;
        public string cliente_nombre { get; set; } = string.Empty;
    }

    public sealed class AutoVerCuentasSaldosRevisionContableData
    {
        public decimal aux_debito { get; set; } = 0;
        public decimal aux_credito { get; set; } = 0;
        public decimal cnt_debito { get; set; } = 0;
        public decimal cnt_credito { get; set; } = 0;
    }

    public sealed class AutoVerCuentasSaldosNoContabilizadoData
    {
        public string tipo_documento { get; set; } = string.Empty;
        public long cod_transaccion { get; set; } = 0;
        public string detalle { get; set; } = string.Empty;
        public string modulo { get; set; } = string.Empty;
        public string cod_cuenta { get; set; } = string.Empty;
        public string ref_01 { get; set; } = string.Empty;
        public string ref_02 { get; set; } = string.Empty;
        public string ref_03 { get; set; } = string.Empty;
        public DateTime? fecha { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string cod_concepto { get; set; } = string.Empty;
        public decimal monto { get; set; } = 0;
    }

    public sealed class AutoVerCuentasSaldosCambioData
    {
        public int id_solicitud { get; set; } = 0;
        public string codigo { get; set; } = string.Empty;
        public string proceso_desc { get; set; } = string.Empty;
        public string opex_desc { get; set; } = string.Empty;
        public decimal saldo_final { get; set; } = 0;
        public string cuenta_corte_mask { get; set; } = string.Empty;
        public string cuenta_corte_desc { get; set; } = string.Empty;
        public string inicial_codigo { get; set; } = string.Empty;
        public string inicial_proceso_desc { get; set; } = string.Empty;
        public string inicial_opex_desc { get; set; } = string.Empty;
        public string cuenta_inicio_mask { get; set; } = string.Empty;
        public string cuenta_inicio_desc { get; set; } = string.Empty;
        public decimal saldo_inicial { get; set; } = 0;
        public DateTime? cambio_fecha { get; set; }
        public decimal cambio_monto { get; set; } = 0;
    }

    public sealed class AutoVerCuentasSaldosAnaliticoData
    {
        public string tipo_asiento { get; set; } = string.Empty;
        public string num_asiento { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string referencia { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public DateTime? fecha_asiento { get; set; }
        public string cod_cuenta_mask { get; set; } = string.Empty;
        public string cod_unidad { get; set; } = string.Empty;
        public string cod_centro_costo { get; set; } = string.Empty;
        public string cod_divisa { get; set; } = string.Empty;
        public decimal tipo_cambio { get; set; } = 0;
        public decimal monto_debito { get; set; } = 0;
        public decimal monto_credito { get; set; } = 0;
    }

    public sealed class AutoVerCuentasSaldosConciliaData
    {
        public string cod_cuenta_mask { get; set; } =
            string.Empty;

        public string tipo_movimiento { get; set; } =
            string.Empty;

        public string tipo_asiento_contable
        {
            get;
            set;
        } = string.Empty;

        public string num_asiento_contable
        {
            get;
            set;
        } = string.Empty;

        public DateTime? fecha_asiento_contable
        {
            get;
            set;
        }

        public decimal monto_contable { get; set; } = 0;
        public decimal monto_auxiliar { get; set; } = 0;
        public decimal diferencia { get; set; } = 0;

        public string tipo_asiento_auxiliar
        {
            get;
            set;
        } = string.Empty;

        public string num_asiento_auxiliar
        {
            get;
            set;
        } = string.Empty;

        public DateTime? fecha_asiento_auxiliar
        {
            get;
            set;
        }
    }
}