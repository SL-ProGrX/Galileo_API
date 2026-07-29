namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrAbonosComprobanteOperacionData
    {
        public int operacion { get; set; }
        public string proceso { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public int opex { get; set; }
        public string opex_descripcion { get; set; } = string.Empty;
        public bool retencion { get; set; }
        public string oficina_descripcion { get; set; } = string.Empty;
        public string linea_descripcion { get; set; } = string.Empty;
        public string recurso_descripcion { get; set; } = string.Empty;
    }

    public class CrAbonosComprobanteOperacionListaItem
    {
        public int operacion { get; set; }
        public string codigo { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class CrAbonosComprobanteAplicarRequest
    {
        public required int operacion { get; set; }
        public required int tipo_abono { get; set; }
        public string tipo_documento { get; set; } = string.Empty;
        public string num_documento { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string cuenta_documento { get; set; } = string.Empty;
        public string referencia_documento { get; set; } = string.Empty;
        public string detalle_documento { get; set; } = string.Empty;
    }

    public class CrAbonosComprobanteAplicarResultadoData
    {
        public string tipo_documento { get; set; } = string.Empty;
        public string num_documento { get; set; } = string.Empty;
        public decimal monto_total { get; set; }
        public string mensaje { get; set; } = string.Empty;
        public string? reporte_resultado { get; set; }
    }

    public class CrAbonosComprobanteOperacionCtasData
    {
        public int id_solicitud { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string cod_Divisa { get; set; } = string.Empty;
        public string cod_unidad { get; set; } = string.Empty;
        public string cod_centro_costo { get; set; } = string.Empty;
        public string ctaintc { get; set; } = string.Empty;
        public string ctaintm { get; set; } = string.Empty;
        public string ctaamortiza { get; set; } = string.Empty;
    }

    public class CrAbonosComprobanteAfectacionData
    {
        public decimal IntCor { get; set; }
        public decimal IntMor { get; set; }
        public decimal Principal { get; set; }
        public decimal Cargos { get; set; }
        public decimal Polizas { get; set; }
    }

    public class CrAbonosComprobanteMovimientoData
    {
        public decimal saldo_anterior { get; set; }
        public decimal saldo_actual { get; set; }
        public string cod_concepto { get; set; } = string.Empty;
        public string mov_usuario { get; set; } = string.Empty;
        public DateTime mov_fecha { get; set; }
    }

    public class CrAbonosComprobanteProximoPagoData
    {
        public DateTime? fecha_pago { get; set; }
        public int num_cuota { get; set; }
        public decimal cuota { get; set; }
        public string notas { get; set; } = string.Empty;
    }

    public class CrAbonosComprobanteOficinaData
    {
        public string titular { get; set; } = string.Empty;
    }

    public class CrAbonosComprobanteCargoData
    {
        public decimal? mov_monto { get; set; }
        public string cod_unidad { get; set; } = string.Empty;
        public string cod_centro_costo { get; set; } = string.Empty;
        public string cod_cuenta { get; set; } = string.Empty;
        public int id_solicitud { get; set; }
        public string codigo { get; set; } = string.Empty;
    }

}
