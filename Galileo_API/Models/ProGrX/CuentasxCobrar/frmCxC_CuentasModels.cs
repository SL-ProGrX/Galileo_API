using Galileo.Models;

namespace Galileo_API.Models.ProGrX.CuentasxCobrar
{
    public class CxCCuentasBusquedaOperacionLista
    {
        public int total { get; set; }
        public List<CxCCuentasBusquedaOperacionItem> lista { get; set; } = new();
    }

    public class CxCCuentasBusquedaOperacionRequest: CxCCuentasBusquedaOperacionItem
    {
        public int skip { get; set; }
        public int take { get; set; }
    }

    public class CxCCuentasBusquedaOperacionItem
    {
        public long? operacion { get; set; }
        public string? cedula { get; set; }
        public string? num_documento { get; set; }
        public string? cod_concepto { get; set; }
        public string? cod_oficina { get; set; }
    }

    public class BancoAutorizadoComboDto
    {
        public int IdX { get; set; }
        public string? ItmX { get; set; }
    }

    public class CxCCuentasConsultaData
    {
        public string? cedula { get; set; } = string.Empty;
        public long operacion { get; set; }
        public string? nombre { get; set; } = string.Empty;
        public string? cod_concepto { get; set; } = string.Empty;
        public string? conceptodesc { get; set; } = string.Empty;
        public string? cod_contrato { get; set; } = string.Empty;
        public string? contratodesc { get; set; } = string.Empty;
        public string? cedula_pagador { get; set; } = string.Empty;
        public string? pagadornom { get; set; } = string.Empty;
        public string? cedula_autorizado { get; set; } = string.Empty;
        public string? autorizadonom { get; set; } = string.Empty;
        public string? bancodesc { get; set; } = string.Empty;
        public int? emitir_banco { get; set; }
        public string? emitir_tipo { get; set; } = string.Empty;
        public string? emitir_cuenta { get; set; } = string.Empty;
        public string? cuentadesc { get; set; } = string.Empty;
        public string? notas { get; set; } = string.Empty;
        public string? cod_oficina { get; set; } = string.Empty;
        public string? oficinax { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public int plazo { get; set; }
        public decimal tasa_corriente { get; set; }
        public decimal tasa_mora { get; set; }
        public decimal cuota { get; set; }
        public string? num_documento { get; set; } = string.Empty;
        public string? estado { get; set; } = string.Empty;
        public string? autorizado { get; set; } = string.Empty;
        public string? autoriza_estado { get; set; } = string.Empty;
        public string? autoriza_usuario { get; set; } = string.Empty;
        public DateTime? autoriza_fecha { get; set; }
        public string? autoriza_notas { get; set; } = string.Empty;
        public string? registro_usuario { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string? activa_usuario { get; set; } = string.Empty;
        public DateTime? activa_fecha { get; set; }
        public string? tesoreria_usuario { get; set; } = string.Empty;
        public DateTime? tesoreria_fecha { get; set; }
        public decimal desembolso_monto { get; set; }
        public decimal desembolso_realizado { get; set; }
        public decimal desembolso_pendiente { get; set; }
        public int facturas { get; set; }
        public decimal adelanto_monto { get; set; }
        public decimal adelanto_porcentaje { get; set; }
        public decimal adelanto_comision { get; set; }
        public int adelanto_comision_dias { get; set; }
        public bool adelanto_comision_apl { get; set; }
        public bool cuotas_apl { get; set; }
        public DateTime? fecha_inicio { get; set; }
        public int pagadores_abierto { get; set; }
        public DateTime? fechaserver { get; set; }
    }

    public class CxCCuentasFacturasLista
    {
        public int casos { get; set; }
        public decimal total { get; set; }
        public decimal adelanto { get; set; }
        public List<CxCCuentasFacturasData> lista { get; set; } = new();
    }

    public class CxCCuentasFacturasData
    {
        public string? cod_factura { get; set; } = string.Empty;
        public string? estado_desc { get; set; } = string.Empty;
        public string? divisa_desc { get; set; } = string.Empty;
        public decimal importe { get; set; }
        public decimal tipo_cambio { get; set; }
        public decimal monto { get; set; }
        public DateTime? fecha_emision { get; set; }
        public DateTime? fecha_pago { get; set; }
        public bool adelanto_indica { get; set; }
        public string? adelanto_tipo { get; set; } = string.Empty;
        public string? adelanto_tipo_desc { get; set; } = string.Empty;
        public decimal adelanto_monto { get; set; }
        public decimal pendiente { get; set; }
        public decimal liberado { get; set; }
        public long operacion_origen { get; set; }
    }

    public class CxCCuentasFacturasAdelantadasLista
    {
        public int casos { get; set; }
        public decimal total { get; set; }
        public decimal adelanto { get; set; }
        public List<CxCCuentasFacturasAdelantadasData> lista { get; set; } = new();
    }

    public class CxCCuentasFacturasAdelantadasData
    {
        public string? cod_factura { get; set; } = string.Empty;
        public long operacion { get; set; }
        public string? cod_divisa { get; set; } = string.Empty;
        public decimal importe { get; set; }
        public decimal tipo_cambio { get; set; }
        public decimal monto { get; set; }
        public DateTime? fecha_emision { get; set; }
        public DateTime? fecha_pago { get; set; }
        public decimal adelanto_monto { get; set; }
        public decimal pendiente { get; set; }
    }

    public class CxCCuentasPersonasFiltroItem
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string categoria { get; set; } = string.Empty;
    }

    public class CxCCuentasPersonasFiltroLista
    {
        public int total { get; set; }
        public List<CxCCuentasPersonasFiltroItem> lista { get; set; } = new();
    }

    public class CxCCuentasConceptoData
    {
        public string cod_concepto { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public int requiere_contrato { get; set; }
        public int proceso_descuento { get; set; }
        public string pagadorid { get; set; } = string.Empty;
        public string pagadordesc { get; set; } = string.Empty;
        public int genera_desembolso { get; set; }
    }

    public class CxCCuentasConceptosFiltroItem
    {
        public string cod_concepto { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class CxCCuentasBusquedaGenericaLista<T>
    {
        public int total { get; set; }
        public List<T> lista { get; set; } = new();
    }

    public class CxCCuentasContratoData
    {
        public string cod_contrato { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public int pagadores_abierto { get; set; }
        public decimal tasa_corriente { get; set; }
        public decimal tasa_mora { get; set; }
        public int plazo { get; set; }
    }

    public class CxCCuentasContratosFiltroItem
    {
        public string cod_contrato { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class CxCCuentasPagadorData
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
    }

    public class CxCCuentasPagadoresFiltroItem
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
    }

    public class CxCCuentasAutorizadoData
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
    }

    public class CxCCuentasAutorizadosFiltroItem
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
    }

    public static class CxCCuentasConstantes
    {
        public const string scrollValido = "El tipo de scroll no es válido.";
        public const string paginacionSql = @"
                OFFSET @offset ROWS
                FETCH NEXT @fetch ROWS ONLY;";
        public const string operacionRequerida = "La operación es requerida.";
        public const string solicitudRequerida = "La solicitud es requerida.";
        public const string fechaFormat = "yyyy/MM/dd";
    }

    public class EjecutarConsultaScrollRequest
    {
        public int codEmpresa { get; set; } = 0;
        public int tipo { get; set; } = 0; // 0 = siguiente, 1 = anterior
        public string sqlAnterior { get; set; } = string.Empty;
        public string sqlSiguiente { get; set; } = string.Empty;
        public object? parametros { get; set; } 
        public string mensajeNoEncontrado { get; set; } = string.Empty;
        public string mensajeDb { get; set; } = string.Empty;
        public string mensajeGeneral { get; set; } = string.Empty;
    }

    public class EjecutarListaLazyLoadRequest
    {
        public int codEmpresa { get; set; } = 0;
        public FiltrosLazyLoadData filtros { get; set; } = new FiltrosLazyLoadData();
        public bool esExportar { get; set; } = false;
        public string sqlCount { get; set; } = string.Empty;
        public string sqlLista { get; set; } = string.Empty;
        public string mensajeDb { get; set; } = string.Empty;
        public string mensajeGeneral { get; set; } = string.Empty;
        public object? parametrosAdicionales { get; set; }
    }

    public class CxCCuentasFacturaRegistraRequest
    {
        public long operacion { get; set; } = 0;
        public string estado { get; set; } = string.Empty;
        public string autoriza_estado { get; set; } = string.Empty;
        public string factura { get; set; } = string.Empty;
        public string divisa { get; set; } = string.Empty;
        public string factura_estado { get; set; } = string.Empty;
        public decimal importe { get; set; } = 0;
        public decimal tipo_cambio { get; set; } = 0;
        public decimal monto { get; set; } = 0;
        public bool adelanta { get; set; } = false;
        public string adelanto_tipo { get; set; } = string.Empty;
        public decimal adelanto { get; set; } = 0;
        public DateTime? fecha_emision { get; set; }
        public DateTime? fecha_pago { get; set; }
        public string usuario { get; set; } = string.Empty;
    }

    public class CxCCuentasFacturaMantenimientoResult
    {
        public decimal total { get; set; } = 0;
        public int facturas { get; set; } = 0;
        public decimal adelanto { get; set; } = 0;
        public decimal pendiente { get; set; } = 0;
    }

    public class CxCCuentasFacturaEliminaRequest
    {
        public long operacion { get; set; } = 0;
        public string estado { get; set; } = string.Empty;
        public string autoriza_estado { get; set; } = string.Empty;
        public string factura { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    public class CxCCuentasFacturaVincularItem
    {
        public string factura { get; set; } = string.Empty;
        public long operacion_origen { get; set; }
        public string divisa { get; set; } = string.Empty;
        public decimal importe { get; set; }
        public decimal tipo_cambio { get; set; }
        public decimal monto { get; set; }
        public DateTime? fecha_emision { get; set; }
        public DateTime? fecha_pago { get; set; }
        public decimal adelanto { get; set; }
    }

    public class CxCCuentasFacturaVincularRequest
    {
        public long operacion { get; set; } = 0;
        public string estado { get; set; } = string.Empty;
        public string autoriza_estado { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public List<CxCCuentasFacturaVincularItem> facturas { get; set; } = new();
    }

    public class CxCCuentasFacturaCargaItem
    {
        public string factura { get; set; } = string.Empty;
        public string fecha_emite { get; set; } = string.Empty;
        public string divisa { get; set; } = string.Empty;
        public decimal tipo_cambio { get; set; }
        public decimal importe { get; set; }
        public decimal monto { get; set; }
        public string fecha_pago { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public decimal adelanto { get; set; }
    }

    public class CxCCuentasFacturaCargaRequest
    {
        public long operacion { get; set; } = 0;
        public string estado { get; set; } = string.Empty;
        public string autoriza_estado { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public List<CxCCuentasFacturaCargaItem> facturas { get; set; } = new();
    }

    public class CxCCuentasSaveParams
    {
        public long operacion { get; set; } = 0;
        public string usuario { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string? cedula_pagador { get; set; }
        public string? cedula_autorizado { get; set; }
        public string cod_concepto { get; set; } = string.Empty;
        public string cod_oficina { get; set; } = string.Empty;
        public string? notas { get; set; }
        public decimal monto { get; set; } = 0;
        public string emitir_tipo { get; set; } = string.Empty;
        public string? emitir_banco { get; set; }
        public string? emitir_cuenta { get; set; }
        public decimal tasa_corriente { get; set; } = 0;
        public decimal tasa_mora { get; set; } = 0;
        public decimal cuota { get; set; } = 0;
        public int plazo { get; set; } = 0;
        public string estado { get; set; } = "R";
        public string? num_documento { get; set; }
        public string? cod_contrato { get; set; }
        public decimal adelanto_monto { get; set; } = 0;
        public decimal adelanto_porcentaje { get; set; } = 0;
        public bool adelanto_comision_apl { get; set; } = false;
        public decimal adelanto_comision { get; set; } = 0;
        public int adelanto_comision_dias { get; set; } = 0;
        public bool chk_cta_apl { get; set; } = false;
        public DateTime? fecha_inicio { get; set; }
    }

    public class CxCCuentasActivacionVerificaResult
    {
        public bool pass { get; set; }
        public string mensaje { get; set; } = string.Empty;
    }

    public class CxCCuentasActivacionRequest
    {
        public long operacion { get; set; } = 0;
        public string usuario { get; set; } = string.Empty;
        public string emitir_tipo { get; set; } = string.Empty;
        public string? emitir_cuenta { get; set; }
        public string? num_documento { get; set; }
        public bool es_factoreo { get; set; } = false;
    }

    public class CxCCuentasAnulacionVerificaResult
    {
        public bool pass { get; set; }
        public string mensaje { get; set; } = string.Empty;
    }

    public class CxCCuentasAnulacionRequest
    {
        public long operacion { get; set; } = 0;
        public string usuario { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
    }

    public class CxCCuentasActivacionDetalleRequest
    {
        public long operacion { get; set; } = 0;
        public string opcion { get; set; } = string.Empty;
    }

    public class CxCCuentasActivacionDetalleItem
    {
        public string descripcion { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public string detalle { get; set; } = string.Empty;
    }

    public class CxCCuentasActivacionDetalleResult
    {
        public bool procesa_tesoreria { get; set; }
        public List<CxCCuentasActivacionDetalleItem> lista { get; set; } = new();
    }
    

}
