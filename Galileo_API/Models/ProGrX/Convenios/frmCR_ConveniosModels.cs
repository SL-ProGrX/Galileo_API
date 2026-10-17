namespace Galileo.Models.ProGrX.Convenios
{
    public sealed class CrConveniosInicialData
    {
        public DateTime fecha_servidor { get; set; }
    }

    public sealed class CrConvenioDto
    {
        public string cod_convenio { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string tipo_convenio { get; set; } = string.Empty;
        public string tipo_descripcion { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string cliente_nombre { get; set; } = string.Empty;
        public int? cod_proveedor { get; set; }
        public string proveedor_nombre { get; set; } = string.Empty;
        public string contrato_numero { get; set; } = string.Empty;
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_vencimiento { get; set; }
        public bool activo { get; set; }
        public decimal porc_comision_creditos { get; set; }
        public decimal porc_comision_recaudacion { get; set; }
        public decimal reservas_porc { get; set; }
        public decimal reservas_retenidas { get; set; }
        public decimal reservas_ejecutado { get; set; }
        public decimal reservas_saldo { get; set; }
        public DateTime? reservas_corte { get; set; }
        public decimal reserva_tope { get; set; }
        public DateTime? ultimo_corte { get; set; }
        public bool comision_informativa { get; set; }
        public bool cobra_creditos_anulados { get; set; }
    }

    public sealed class CrConveniosGuardarRequest
    {
        public bool v_edita { get; set; }
        public CrConvenioDto convenio { get; set; } = new();
    }

    public class CrConveniosScrollRequest
    {
        public string? cod_convenio { get; set; }
        public int direccion { get; set; }
    }

    public sealed class CrConveniosF4Data
    {
        public string cod_convenio { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public sealed class CrConveniosConvenioRequest
    {
        public string cod_convenio { get; set; } = string.Empty;
    }

    public sealed class CrConveniosCodigoBusquedaRequest
    {
        public string? texto { get; set; }
        public FiltrosLazyLoadData filtros { get; set; } = new();
    }

    public sealed class CrConveniosRecaudacionData
    {
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
    }

    public sealed class CrConveniosRecaudacionActualizarRequest
    {
        public string cod_convenio { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public bool asigna { get; set; }
    }

    public sealed class CrConveniosRecaudacionScrollRequest
    {
        public string? codigo { get; set; } = string.Empty;
        public int direccion { get; set; }
    }

    public sealed class CrConveniosDevolucionesData
    {
        public string retencion_codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
    }

    public sealed class CrConveniosDevolucionesActualizarRequest
    {
        public string cod_convenio { get; set; } = string.Empty;
        public string retencion_codigo { get; set; } = string.Empty;
        public bool asigna { get; set; }
    }

    public sealed class CrConveniosDevolucionesScrollRequest
    {
        public string? retencion_codigo { get; set; } = string.Empty;
        public int direccion { get; set; }
    }

    public sealed class CrConveniosCreditoData
    {
        public string tipo { get; set; } = "C";
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool? combina_destino { get; set; }
        public string asociar_descripcion { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
    }

    public sealed class CrConveniosCreditosListaRequest
    {
        public string cod_convenio { get; set; } = string.Empty;
        public string tipo { get; set; } = "C";
    }

    public sealed class CrConveniosCreditosBusquedaRequest
    {
        public string tipo { get; set; } = "C";
        public string? texto { get; set; }
        public FiltrosLazyLoadData filtros { get; set; } = new();
    }

    public sealed class CrConveniosCreditosScrollRequest
    {
        public string tipo { get; set; } = "C";

        public string? codigo { get; set; }

        public int direccion { get; set; }
    }

    public sealed class CrConveniosCreditosActualizarRequest
    {
        public string cod_convenio { get; set; } = string.Empty;
        public string tipo { get; set; } = "C";
        public string codigo { get; set; } = string.Empty;
        public bool combina_destino { get; set; }
        public bool asigna { get; set; }
    }

    public sealed class CrConveniosCargoData
    {
        public string cod_cargo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal valor { get; set; }
    }

    public sealed class CrConveniosCargosActualizarRequest
    {
        public string cod_convenio { get; set; } = string.Empty;
        public string cod_cargo { get; set; } = string.Empty;
        public bool asigna { get; set; }
    }

    public sealed class CrConveniosCargosScrollRequest
    {
        public string? cod_cargo { get; set; } = string.Empty;
        public int direccion { get; set; }
    }

    public sealed class CrConveniosRebajasCxpData
    {
        public string cod_cargo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string cod_cuenta { get; set; } = string.Empty;
        public string cod_cuenta_mask { get; set; } = string.Empty;
        public string cuenta_descripcion { get; set; } = string.Empty;
        public bool asignado { get; set; }
    }

    public sealed class CrConveniosRebajasCxpActualizarRequest
    {
        public string cod_convenio { get; set; } = string.Empty;
        public string cod_cargo { get; set; } = string.Empty;
        public bool asigna { get; set; }
    }

    public sealed class CrConveniosAhorrosData
    {
        public string cod_plan { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public int cod_contrato { get; set; }
        public DateTime? fecha_inicio { get; set; }
        public decimal mensualidad { get; set; }
        public decimal acumulado { get; set; }
    }
    public sealed class CrConveniosAhorroPlanData
    {
        public string cod_plan { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public int? cod_contrato { get; set; }
        public decimal? mensualidad { get; set; }
    }

    public sealed class CrConveniosAhorrosPlanesBusquedaRequest
    {
        public string? cod_convenio { get; set; }
        public string? texto { get; set; }
        public FiltrosLazyLoadData filtros { get; set; } = new();
    }

    public sealed class CrConveniosAhorrosScrollRequest
    {
        public string? cod_convenio { get; set; }
        public string? cod_plan { get; set; }
        public int direccion { get; set; }
    }

    public sealed class CrConveniosAhorrosContratosBusquedaRequest
    {
        public string cod_convenio { get; set; } = string.Empty;
        public bool terceros { get; set; }
        public string? texto { get; set; }
        public FiltrosLazyLoadData filtros { get; set; } = new();
    }

    public sealed class CrConveniosAhorrosContratoF4Data
    {
        public string cod_plan { get; set; } = string.Empty;
        public int cod_contrato { get; set; }
        public decimal monto { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string cliente_nombre { get; set; } = string.Empty;
    }

    public sealed class CrConveniosAhorrosActualizarRequest
    {
        public string cod_convenio { get; set; } = string.Empty;
        public string cod_plan { get; set; } = string.Empty;
        public int cod_contrato { get; set; }
        public decimal mensualidad { get; set; }
        public bool asigna { get; set; }
    }

    public sealed class CrConveniosOrdenesListaRequest
    {
        public string cod_convenio { get; set; } = string.Empty;
        public int lineas { get; set; } = 50;
    }

    public sealed class CrConveniosOrdenData
    {
        public int cod_orden { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_corte { get; set; }
        public decimal recaudacion_cuotas { get; set; }
        public decimal recaudacion_cargos { get; set; }
        public decimal nuevos_creditos { get; set; }
        public decimal total_rebajos_cxp { get; set; }
        public decimal retencion_ahorros { get; set; }
        public decimal total_pagar { get; set; }
        public string documento { get; set; } = string.Empty;
    }

    public sealed class CrConveniosReservaData
    {
        public string cod_convenio { get; set; } = string.Empty;
        public DateTime corte_inicial { get; set; }
        public decimal saldo_inicial { get; set; }
        public decimal total_recaudado { get; set; }
        public decimal total_ejecutado { get; set; }
        public decimal saldo_actual { get; set; }
        public DateTime? actualizado { get; set; }
    }
}