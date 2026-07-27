using System.Text.Json.Serialization;

namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrSeguimientoTramitesOpcionItem
    {
        public string item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class CrSeguimientoTramitesInicializarData
    {
        public DateTime fecha_servidor { get; set; }
        public string oficina { get; set; } = string.Empty;
        public string oficina_titular { get; set; } = string.Empty;
        public string oficina_apoyo { get; set; } = string.Empty;
        public decimal fecha_credito { get; set; }
        public int sys_plan_pagos { get; set; }
        public int sys_doc_version { get; set; }
        public List<CrSeguimientoTramitesOpcionItem> comites { get; set; } = new();
        public List<CrSeguimientoTramitesOpcionItem> oficinas { get; set; } = new();
        public List<CrSeguimientoTramitesOpcionItem> garantias_fondo { get; set; } = new();
        public List<CrSeguimientoTramitesOpcionItem> actividades { get; set; } = new();
        public List<CrSeguimientoTramitesOpcionItem> canales { get; set; } = new();
        public List<CrSeguimientoTramitesOpcionItem> bancos { get; set; } = new();
        public List<CrSeguimientoTramitesOpcionItem> tipos_documento { get; set; } = new();
    }

    public class CrSeguimientoTramitesBusquedaItem
    {
        public int id_solicitud { get; set; }
        public string codigo { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public DateTime? fechasol { get; set; }
        public decimal montosol { get; set; }
        public string estadosol { get; set; } = string.Empty;
        public string estado_descripcion { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string activo_descripcion { get; set; } = string.Empty;
        public string proceso { get; set; } = string.Empty;
        public string proceso_descripcion { get; set; } = string.Empty;
    }

    public class CrSeguimientoTramitesOperacionData
    {
        public int id_solicitud { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public string coddesc { get; set; } = string.Empty;
        public string cod_divisa { get; set; } = string.Empty;
        public string estadosol { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public decimal montoapr { get; set; }
        public decimal montosol { get; set; }
        public decimal cuota { get; set; }
        public int plazo { get; set; }
        public decimal @int { get; set; }
        public decimal tasa_pts_bono { get; set; }
        public decimal tasafacial { get; set; }
        public decimal iva_monto { get; set; }
        public string observacion { get; set; } = string.Empty;
        public string pagare { get; set; } = string.Empty;
        public DateTime? fecha_server { get; set; }
        public DateTime? fechasol { get; set; }
        public DateTime? fechares { get; set; }
        public DateTime? fechaforp { get; set; }
        public DateTime? fecha_registro { get; set; }
        public DateTime? fecha_inicio_calculo { get; set; }
        public DateTime? fecha_vence { get; set; }
        public DateTime? tesoreria { get; set; }
        public DateTime? autoriza_fecha { get; set; }
        public DateTime? anula_fecha { get; set; }
        public int id_comite { get; set; }
        public string comdesc { get; set; } = string.Empty;
        public string cod_destino { get; set; } = string.Empty;
        public string destinodesc { get; set; } = string.Empty;
        public string ofi_presenta { get; set; } = string.Empty;
        public string oficinadesc { get; set; } = string.Empty;
        public int cod_banco { get; set; }
        public string bancodesc { get; set; } = string.Empty;
        public string cta_banco { get; set; } = string.Empty;
        public string cuentadesc { get; set; } = string.Empty;
        public string primer_cuota { get; set; } = string.Empty;
        public int deduccion { get; set; }
        public string ind_deduce_planilla { get; set; } = string.Empty;
        public string documento_referido { get; set; } = string.Empty;
        public string emitir { get; set; } = string.Empty;
        public int proveedorid { get; set; }
        public string proveedordesc { get; set; } = string.Empty;
        public int cod_institucion { get; set; }
        public int cod_deductora { get; set; }
        public string deductoradesc { get; set; } = string.Empty;
        public decimal? prideduc { get; set; }
        public string cod_grupo { get; set; } = string.Empty;
        public string recursodesc { get; set; } = string.Empty;
        public string garantia { get; set; } = string.Empty;
        public string garantiadesc { get; set; } = string.Empty;
        public string garantia_fnd { get; set; } = string.Empty;
        public int garantia_fnd_contrato { get; set; }
        public string cod_actividad { get; set; } = string.Empty;
        public string actividaddesc { get; set; } = string.Empty;
        public string canal_tipo { get; set; } = string.Empty;
        public string canaldesc { get; set; } = string.Empty;
        public int id_promotor { get; set; }
        public string ejecutivo { get; set; } = string.Empty;
        public int ind_expediente_digital { get; set; }
        public int pagare_manual { get; set; }
        public long? formulario { get; set; }
        public int ind_aplica_traslado_salario { get; set; }
        public string userrec { get; set; } = string.Empty;
        public string userres { get; set; } = string.Empty;
        public string userfor { get; set; } = string.Empty;
        public string usertesoreria { get; set; } = string.Empty;
        public string autoriza_user { get; set; } = string.Empty;
        public string autoriza_nota { get; set; } = string.Empty;
        public string anula_usuario { get; set; } = string.Empty;
        public string basecalculo { get; set; } = string.Empty;
        public string base_calculo { get; set; } = string.Empty;
        public int bulletind { get; set; }
        public byte[] ts { get; set; } = Array.Empty<byte>();
        public string estado_tooltip { get; set; } = string.Empty;
        public string tasa_tooltip { get; set; } = string.Empty;
        public string seccion_inicial { get; set; } = "0-0";
        public bool permite_formalizar { get; set; }
        public List<CrSeguimientoTramitesOpcionItem> estados { get; set; } = new();
        public List<CrSeguimientoTramitesOpcionItem> destinos { get; set; } = new();
        public List<CrSeguimientoTramitesOpcionItem> garantias { get; set; } = new();
        public List<CrSeguimientoTramitesOpcionItem> recursos { get; set; } = new();
        public List<CrSeguimientoTramitesOpcionItem> cuentas_bancarias { get; set; } = new();
        public List<CrSeguimientoTramitesOpcionItem> deductoras { get; set; } = new();
    }

    internal sealed class CrSeguimientoTramitesOpcionRaw
    {
        public object? idx { get; set; }
        public string itmx { get; set; } = string.Empty;
    }

    public class CrSeguimientoTramitesRecepcionSocioItem
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string estado_actual { get; set; } = string.Empty;
    }

    public class CrSeguimientoTramitesRecepcionLineaItem
    {
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string cod_divisa { get; set; } = string.Empty;
        public string base_calculo { get; set; } = string.Empty;
    }

    public class CrSeguimientoTramitesRecepcionPromotorItem
    {
        public int id_promotor { get; set; }
        public string nombre { get; set; } = string.Empty;
    }

    public class CrSeguimientoTramitesRecepcionProveedorItem
    {
        public int cod_proveedor { get; set; }
        public string cedjur { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class CrSeguimientoTramitesRecepcionLineaContextoRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
    }

    public class CrSeguimientoTramitesRecepcionLineaContextoData
    {
        public string codigo { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string estado_actual { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string cod_divisa { get; set; } = string.Empty;
        public string base_calculo { get; set; } = string.Empty;
        public int comite_id { get; set; }
        public bool muestra_vencimiento { get; set; }
        public List<CrSeguimientoTramitesOpcionItem> destinos { get; set; } = new();
        public List<CrSeguimientoTramitesOpcionItem> garantias { get; set; } = new();
        public List<CrSeguimientoTramitesOpcionItem> recursos { get; set; } = new();
        public List<CrSeguimientoTramitesOpcionItem> bancos { get; set; } = new();
        public List<CrSeguimientoTramitesOpcionItem> estados { get; set; } = new();
    }

    public class CrSeguimientoTramitesRecepcionGarantiaContextoRequest
    {
        public string cedula { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public string destino { get; set; } = string.Empty;
        public string garantia { get; set; } = string.Empty;
        [JsonRequired]
        public decimal monto_actual { get; set; }
        [JsonRequired]
        public int plazo { get; set; }
    }

    public class CrSeguimientoTramitesRecepcionGarantiaContextoData
    {
        public string formulario { get; set; } = string.Empty;
        public decimal? monto_sugerido { get; set; }
        public int? plazo_sugerido { get; set; }
        public decimal tasa_pts_bono { get; set; }
        public bool muestra_fondo { get; set; }
        public bool muestra_vencimiento { get; set; }
        public bool permite_traslado_salario { get; set; }
    }

    public class CrSeguimientoTramitesRecepcionBancoCuentasRequest
    {
        public string cedula { get; set; } = string.Empty;
        [JsonRequired]
        public int banco_id { get; set; }
    }

    public class CrSeguimientoTramitesRecepcionFondoContextoRequest
    {
        public string cedula { get; set; } = string.Empty;
        public string fnd_garantia { get; set; } = string.Empty;
        [JsonRequired]
        public int fnd_contrato { get; set; }
    }

    public class CrSeguimientoTramitesRecepcionFondoContextoData
    {
        public List<CrSeguimientoTramitesOpcionItem> contratos { get; set; } = new();
        public int contrato_seleccionado { get; set; }
        public decimal disponible { get; set; }
        public decimal tasa { get; set; }
        public int plazo { get; set; }
        public bool aplica_tasa { get; set; }
        public bool aplica_plazo { get; set; }
    }

    internal sealed class CrSeguimientoTramitesRecepcionLineaContextoRaw
    {
        public string codigo { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string estado_actual { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string cod_divisa { get; set; } = string.Empty;
        public string base_calculo { get; set; } = string.Empty;
        public int comite_id { get; set; }
    }

    internal sealed class CrSeguimientoTramitesRecepcionGarantiaCalculoRaw
    {
        public decimal? monto_sugerido { get; set; }
        public decimal tasa_pts_bono { get; set; }
        public int plazo_bono { get; set; }
    }

    internal sealed class CrSeguimientoTramitesRecepcionGarantiaReglaRaw
    {
        public string formulario { get; set; } = string.Empty;
        public string base_calculo { get; set; } = string.Empty;
    }

    internal sealed class CrSeguimientoTramitesRecepcionFondoCalculoRaw
    {
        public decimal disponible { get; set; }
        public decimal tasa { get; set; }
        public int plazo { get; set; }
        public bool aplica_tasa { get; set; }
        public bool aplica_plazo { get; set; }
    }

    public class CrSeguimientoTramitesRecepcionGuardarRequest
    {
        [JsonRequired]
        public int operacion { get; set; }
        public string codigo { get; set; } = string.Empty;
        public string codigo_anterior { get; set; } = string.Empty;
        public string destino { get; set; } = string.Empty;
        public string garantia { get; set; } = string.Empty;
        public string estado_solicitud { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        [JsonRequired]
        public decimal monto { get; set; }
        [JsonRequired]
        public int plazo { get; set; }
        [JsonRequired]
        public decimal tasa { get; set; }
        [JsonRequired]
        public decimal cuota { get; set; }
        [JsonRequired]
        public decimal tasa_pts_bono { get; set; }
        [JsonRequired]
        public DateTime fecha_solicitud { get; set; }
        public string divisa { get; set; } = string.Empty;
        [JsonRequired]
        public int comite_id { get; set; }
        public string observacion { get; set; } = string.Empty;
        public string oficina_presenta { get; set; } = string.Empty;
        public int? promotor_id { get; set; }
        [JsonRequired]
        public int banco_id { get; set; }
        public string cuenta_bancaria { get; set; } = string.Empty;
        public string emite_tipo { get; set; } = string.Empty;
        public int? proveedor_id { get; set; }
        public string fnd_garantia { get; set; } = string.Empty;
        [JsonRequired]
        public int fnd_contrato { get; set; }
        public DateTime? fecha_vence { get; set; }
        [JsonRequired]
        public bool ind_expediente_digital { get; set; }
        [JsonRequired]
        public bool pagare_manual { get; set; }
        public long? formulario { get; set; }
        [JsonRequired]
        public bool ind_traslado_salario { get; set; }
        [JsonRequired]
        public bool ind_deduce_planilla { get; set; }
        public string? actividad_id { get; set; }
        public string? canal_id { get; set; }
        public string usuario { get; set; } = string.Empty;
    }

    public class CrSeguimientoTramitesRecepcionGuardarResult
    {
        public int operacion { get; set; }
        public bool inicial { get; set; }
        public string mensaje { get; set; } = string.Empty;
    }

    internal sealed class CrSeguimientoTramitesRecepcionGuardarRaw
    {
        public int operacion { get; set; }
        public int inicial { get; set; }
    }

    internal sealed class CrSeguimientoTramitesRecepcionCatalogoRaw
    {
        public string? cta_nint_c { get; set; }
        public string retencion { get; set; } = string.Empty;
        public string poliza { get; set; } = string.Empty;
        public int activo { get; set; }
        public int permite_cbr { get; set; }
        public string base_calculo { get; set; } = string.Empty;
    }

    internal sealed class CrSeguimientoTramitesRecepcionReferenciasValidacion
    {
        public int banco_asignado { get; init; }
        public int estado_persona { get; init; }
        public int destino { get; init; }
        public int banco { get; init; }
        public int comite { get; init; }
        public int garantia { get; init; }
    }

    internal sealed class CrSeguimientoTramitesRecepcionValidacion
    {
        public List<string> mensajes { get; set; } = new();
        public string base_calculo { get; set; } = string.Empty;
    }
}
