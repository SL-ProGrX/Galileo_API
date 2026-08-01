namespace Galileo_API.Models.ProGrX_Contabilidad
{
    using System;

    namespace Galileo_API.Models.ProGrX_Contabilidad
    {
        public class CntxExploradorFiltrosDto
        {

            public string? tipo { get; set; }
            public string? unidad { get; set; }
            public string? cc { get; set; }

            public string? mov_tipo { get; set; } = "NA";
            public required decimal mov_desde { get; set; } = 0;
            public required decimal mov_hasta { get; set; } = 999999999999;

            public string? num_asiento { get; set; }
            public string? num_documento { get; set; }
            public string? detalle { get; set; }
            public string? referencia { get; set; }

            public DateTime? fecha_desde { get; set; }
            public DateTime? fecha_hasta { get; set; }
            public bool? todas { get; set; }

            public string? divisa { get; set; }
            public string? cuenta_inicio { get; set; }
            public string? cuenta_corte { get; set; }

            public int? lineas { get; set; } = 1000;

            public string? estado_asiento { get; set; }


            public string? cod_cuenta { get; set; }
            public string? cod_tipo_asiento { get; set; }
            public string? cod_periodo { get; set; }

            public int? cod_contabilidad { get; set; }


        }

        public class CntxAsientoRsmDto
        {
            public string? num_asiento { get; set; }
            public string? tipo_asiento { get; set; }
            public DateTime? fecha_asiento { get; set; }
            public int anio { get; set; }
            public int mes { get; set; }
            public byte[]? ts { get; set; }
            public string? descripcion { get; set; }

            public decimal debe { get; set; }
            public decimal haber { get; set; }

            public string? aplicado { get; set; }
            public string? referencia { get; set; }
        }

        public class CntxAsientoDetDto
        {
            public string? cod_cuenta_mask { get; set; }
            public string? cod_cuenta { get; set; }
            public string? cuenta_desc { get; set; }

            public string? cuenta_descripcion { get; set; }

            public decimal monto_debito { get; set; }
            public decimal monto_credito { get; set; }

            public string? documento { get; set; }
            public string? detalle { get; set; }

            public string? cod_unidad { get; set; }
            public string? cod_centro_costo { get; set; }
            public string? cod_divisa { get; set; }

            public decimal? tipo_cambio { get; set; }
            public decimal? importe { get; set; }
        }

        public class CntxPeriodoDto
        {
            public int? anio { get; set; }
            public int? mes { get; set; }
            public DateTime? fecha_corte { get; set; }
            public string? estado { get; set; }
            public string? usuario_cierre { get; set; }
            public DateTime? fecha_cierre { get; set; }
        }

        public class CntxCuentaDto
        {
            public string? cod_cuenta { get; set; } = string.Empty;
            public string? cod_cuenta_mask { get; set; } = string.Empty;
            public string? descripcion { get; set; } = string.Empty;
            public bool? es_mayor { get; set; }

            public string? acepta_movimientos_desc { get; set; } = string.Empty;

            public string? tipo_descripcion { get; set; } = string.Empty;

        }

        public class CntxAsientoTreeDto
        {
            public string? num_asiento { get; set; } = string.Empty;
            public DateTime? fecha_asiento { get; set; }
            public int anio { get; set; }
            public int mes { get; set; }
            public byte[]? ts { get; set; }
            public string? descripcion { get; set; } = string.Empty;
            public decimal debe { get; set; }
            public decimal haber { get; set; }
            public string? aplicado { get; set; }
            public string? balanceado { get; set; }
        }

        public class CntxConsultaAnaliticaDto
        {
            public string? num_asiento { get; set; }
            public string? tipo_asiento { get; set; }
            public DateTime? fecha_asiento { get; set; }
            public string? descripcion { get; set; }
            public decimal monto_debito { get; set; }
            public decimal monto_credito { get; set; }
            public string? aplicado { get; set; }
            public string? balanceado { get; set; }
            public string? cod_cuenta_mask { get; set; }
            public string? cuenta_descripcion { get; set; }
            public string? cod_unidad { get; set; }
            public string? cod_centro_costo { get; set; }
            public string? cod_divisa { get; set; }
            public decimal? tipo_cambio { get; set; }
            public decimal? importe { get; set; }
            public string? documento { get; set; }
            public string? detalle { get; set; }
            public string? referencia { get; set; }
            public string? user_crea { get; set; }
            public string? user_modifica { get; set; }
            public string? user_autoriza { get; set; }
            public string? user_aplica { get; set; }
        }

        public class CntxMovimientoNodoRequest
        {
            public int? cod_empresa { get; set; }
            public required int cod_contabilidad { get; set; }
            public string? tipo_nodo { get; set; }
            public string? codigo { get; set; }
            public required int anio { get; set; }
            public required int mes { get; set; }
        }

        public class CntxMovimientoCuentaDto
        {
            public string? cod_cuenta { get; set; }
            public string? cuenta { get; set; }
            public string? descripcion { get; set; }
            public decimal saldo_inicial { get; set; }
            public decimal total_debitos { get; set; }
            public decimal total_creditos { get; set; }
            public decimal movimiento_mes { get; set; }
            public decimal saldo_actual { get; set; }
            public bool acepta_movimientos { get; set; }
        }

        public class CntxMovimientoDetalleDto
        {
            public string? num_asiento { get; set; }
            public string? tipo_asiento { get; set; }
            public string? detalle { get; set; }
            public decimal monto_debito { get; set; }
            public decimal monto_credito { get; set; }
            public string? cod_unidad { get; set; }
            public string? cod_centro_costo { get; set; }
            public string? cod_divisa { get; set; }
            public decimal? tipo_cambio { get; set; }
            public decimal? importe { get; set; }
        }

        public class CntxMovimientoNodoDto
        {
            public string tipo_vista { get; set; } = "RESUMEN_CUENTAS";
            public List<CntxMovimientoCuentaDto> cuentas { get; set; } = new();
            public List<CntxMovimientoDetalleDto> detalles { get; set; } = new();
        }

        public class CntxTipoCuentaDto
        {
            public string? item { get; set; } = string.Empty; // tipo_cuenta
            public string? descripcion { get; set; } = string.Empty;
        }

        public class CntxDiferidoHistoricoDto
        {
            public string? num_asiento { get; set; }

            public string? tipo_asiento { get; set; }

            public DateTime? fecha { get; set; }

            public int? anio { get; set; }

            public int? mes { get; set; }

            public string? usuario { get; set; }
        }

        public class CntxAsientoResumenDto
        {
            public string? tipo_asiento { get; set; } = string.Empty;
            public string? descripcion { get; set; } = string.Empty;
            public int? movimientos { get; set; }
            public decimal? debitos { get; set; }
            public decimal? creditos { get; set; }
            public int? asientos_total { get; set; }
            public int? asientos_aplicados { get; set; }
            public int? asientos_pendientes { get; set; }
            public int? asientos_desbalanceados { get; set; }
        }

    }

    public class CntxCatalogoResumenDto
    {
        public string? codigo { get; set; } = string.Empty;
        public string? descripcion { get; set; } = string.Empty;
        public string? clasificacion { get; set; } = string.Empty;
        public int? movimientos { get; set; }
        public decimal? total_debitos { get; set; }
        public decimal? total_creditos { get; set; }
        public decimal? diferencia { get; set; }
    }

    public class CatalogoResumenRequest
    {
        public int? codEmpresa { get; set; }
        public int? cod_contabilidad { get; set; }
        public DateTime? fechaDesde { get; set; }
        public DateTime? fechaHasta { get; set; }
    }

    public class AreaResumenDto
    {
        public string? cod_area { get; set; } = string.Empty;
        public string? descripcion { get; set; } = string.Empty;
        public int? movimientos { get; set; }
        public decimal? total_debitos { get; set; }
        public decimal? total_creditos { get; set; }
    }

    public class AreaTrabajoDto
    {
        public int cod_area { get; set; }
        public string? descripcion { get; set; } = string.Empty;
        public bool es_padre { get; set; }
    }

    public class AreaCuentaDto
    {
        public string? cuenta { get; set; } = string.Empty;
        public string? descripcion { get; set; } = string.Empty;
        public string? acepta_movimientos { get; set; } = string.Empty;
    }

    public class CntxDiferidoPlantillaDto
    {
        public int? Item { get; set; }
        public string? Descripcion { get; set; }
        public decimal? Monto { get; set; }
        public decimal? Acumulado { get; set; }
        public decimal? Pendiente { get; set; }
        public int? Plazo { get; set; }
        public DateTime? Inicio { get; set; }
        public string? Usuario { get; set; }
        public string? Documento { get; set; }
    }

    public class CntxContabilidadDto
    {
        public int? codigo { get; set; }
        public string? nombre { get; set; } = string.Empty;
        public string? tel_central { get; set; }
        public string? tel_fax { get; set; }
        public string? contacto { get; set; }
    }

    public class CntxConfiguracionArbolDto
    {
        public bool exp_asientos { get; set; }
        public bool exp_cuentas { get; set; }
        public bool exp_areas { get; set; }
        public bool exp_plan_fijo { get; set; }
        public bool exp_plan_rate { get; set; }
        public bool exp_diferidos { get; set; }
        public bool exp_mantenimiento { get; set; }
    }

    public class CntxPlantillaFijaDetalleDto
    {
        public string? cod_cuenta { get; set; }
        public string? cod_cuenta_mask { get; set; }
        public string? descripcion { get; set; }
        public decimal debitos { get; set; }
        public decimal creditos { get; set; }
        public string? detalle { get; set; }
    }

    public class CntxCierreDto
    {
        public int? in_anio { get; set; }
        public int? in_mes { get; set; }
        public int? co_anio { get; set; }
        public int? co_mes { get; set; }
        public string? descripcion { get; set; }
        public string? gan_per { get; set; }
        public string? exc_uti { get; set; }
        public string? renta_cta { get; set; }
        public decimal? renta { get; set; }
        public string? vigente { get; set; }
    }


    public class CntxTipoAsientoDto
    {
        public string? tipo_asiento { get; set; } = string.Empty;
        public string? descripcion { get; set; } = string.Empty;
        public int? consecutivo { get; set; }
    }

    public class CntxDivisaDto
    {
        public string? cod_divisa { get; set; } = string.Empty;
        public string? descripcion { get; set; } = string.Empty;
        public decimal? tc_venta { get; set; }
        public decimal? tc_compra { get; set; }
        public decimal? divisa_local { get; set; }
    }

        public class CntxMayorizarRequest
        {
            public int? cod_empresa { get; set; }
            public required int cod_contabilidad { get; set; }
            public string? tipo_asiento { get; set; } = "";
            public string? num_asiento { get; set; } = "";
            public required int anio { get; set; }
            public required int mes { get; set; }
            public string? usuario { get; set; } = "";
            public byte[]? ts { get; set; }
        }

        public class CntxBorrarAsientoRequest
        {
            public int? cod_empresa { get; set; }
            public required int cod_contabilidad { get; set; }
            public string? tipo_asiento { get; set; } = "";
            public string? num_asiento { get; set; } = "";
            public required int anio { get; set; }
            public required int mes { get; set; }
            public string? usuario { get; set; } = "";
            public byte[]? ts { get; set; }
        }


}
