
using Galileo.Models;

namespace Galileo_API.Models.ProGrX_RRHH
{
    public class RhCatConceptoData
    {
        public string cod_concepto { get; set; } = string.Empty;

        public string descripcion { get; set; } = string.Empty;

        public string prioridad { get; set; } = string.Empty;

        public bool? activo { get; set; }

        public string tipo_concepto { get; set; } = string.Empty;

        public bool? patrono_control { get; set; }

        public bool? colilla_muestra { get; set; }

        public bool? revisa_salario_imbargable { get; set; }

        public bool? registro_contable { get; set; }

        public bool? aplicacion_general { get; set; }

        public bool? aplicacion_permite_cambios { get; set; }

        public bool? aplicacion_credito_fiscal { get; set; }

        public string aplicacion_tipo { get; set; } = string.Empty;

        public string aplicacion_base { get; set; } = string.Empty;

        public decimal? aplicacion_valor { get; set; }

        public string? aplicacion_formula { get; set; }

        public DateTime? aplicacion_inicio { get; set; }

        public DateTime? aplicacion_corte { get; set; }

        public string? sumar_en { get; set; }

        public string? cod_cuenta { get; set; }

        public string? cod_cuenta_cierre { get; set; }

        public string? cod_er { get; set; }

        public string? cod_concepto_base { get; set; }

        public string? cod_grupo { get; set; }

        public string tipo_concepto_desc { get; set; } = string.Empty;

        public string aplicacion_tipo_desc { get; set; } = string.Empty;

        public string aplicacion_base_desc { get; set; } = string.Empty;

        public string grupo_desc { get; set; } = string.Empty;

        public string cta_mask { get; set; } = string.Empty;

        public string cta_desc { get; set; } = string.Empty;

        public string cta_cc_mask { get; set; } = string.Empty;

        public string cta_cc_desc { get; set; } = string.Empty;

        public string entidad_id { get; set; } = string.Empty;

        public string entidad_desc { get; set; } = string.Empty;

        public string concepto_base_id { get; set; } = string.Empty;

        public string concepto_base_desc { get; set; } = string.Empty;

        public DateTime? fecha_finaliza_default { get; set; }

        public DateTime? registro_fecha { get; set; }

        public string registro_usuario { get; set; } = string.Empty;
    }
    public class RhCatConceptoOpcionData
    {
        public string codigo { get; set; } = string.Empty;

        public string descripcion { get; set; } = string.Empty;
    }

    public class RhCatConceptosInicialData
    {
        public DateTime? fecha_servidor { get; set; }

        public List<DropDownListaGenericaModel> grupos { get; set; } = new();

        public List<DropDownListaGenericaModel> tipos_concepto { get; set; } = new();

        public List<DropDownListaGenericaModel> tipos_aplicacion { get; set; } = new();

        public List<DropDownListaGenericaModel> bases_aplicacion { get; set; } = new();

        public List<DropDownListaGenericaModel> resumenes { get; set; } = new();
    }

    public class RhCatConceptoObtenerRequest
    {
        public string? cod_concepto { get; set; }
    }
    public class RhCatConceptoBusquedaData
    {
        public string cod_concepto { get; set; } = string.Empty;

        public string descripcion { get; set; } = string.Empty;
    }
    public class RhCatConceptoBusquedaRequest
    {
        public string? texto { get; set; } = string.Empty;

        public string? cod_concepto_excluir { get; set; }
    }
    public class RhCatConceptoScrollRequest
    {
        public string? cod_concepto { get; set; } = string.Empty;

        public int? direccion { get; set; }
    }
    public class RhCatConceptoCodigoNuevoRequest
    {
    }
    public class RhCatConceptoCodigoNuevoData
    {
        public string cod_concepto { get; set; } = string.Empty;
    }
    public class RhCatConceptoPrioridadNuevaRequest
    {
        public string? tipo_concepto { get; set; }
    }
    public class RhCatConceptoPrioridadNuevaData
    {
        public string prioridad { get; set; } = string.Empty;
    }
    public class RhCatConceptoGuardarRequest
    {
        public bool? v_edita { get; set; }

        public RhCatConceptoData concepto { get; set; } = new();
    }
    public class RhCatConceptoDetalleData
    {
        public string cod_concepto { get; set; } = string.Empty;

        public string cod_detalle { get; set; } = string.Empty;

        public string descripcion { get; set; } = string.Empty;

        public decimal? base_minima { get; set; }

        public decimal? valor { get; set; }

        public DateTime? registro_fecha { get; set; }

        public string registro_usuario { get; set; } = string.Empty;
    }

    /// <summary>
    /// Lista de detalles de un concepto.
    /// </summary>
    public class RhCatConceptoDetallesLista
    {
        public int total { get; set; }

        public List<RhCatConceptoDetalleData> lista { get; set; } = new();
    }
    public class RhCatConceptoDetalleListaRequest
    {
        public string? cod_concepto { get; set; }
    }
    public class RhCatConceptoDetalleGuardarRequest
    {
        public RhCatConceptoDetalleData detalle { get; set; } = new();
    }
    public class RhCatConceptoDetalleEliminarRequest
    {
        public string? cod_concepto { get; set; }

        public string? cod_detalle { get; set; }
    }
    public class RhCatConceptoExclusionData
    {
        public string cod_concepto_exc { get; set; } = string.Empty;

        public string descripcion { get; set; } = string.Empty;
    }
    public class RhCatConceptoExclusionesLista
    {
        public int total { get; set; }

        public List<RhCatConceptoExclusionData> lista { get; set; } = new();
    }
    public class RhCatConceptoExclusionListaRequest
    {
        public string? cod_concepto { get; set; }
    }
    public class RhCatConceptoExclusionGuardarRequest
    {
        public string? cod_concepto { get; set; }

        public string? cod_concepto_exc { get; set; }
    }
    public class RhCatConceptoExclusionEliminarRequest
    {
        public string? cod_concepto { get; set; }

        public string? cod_concepto_exc { get; set; }
    }
    public class RhCatConceptoTablaData
    {
        public long? tabla_id { get; set; }

        public string cod_concepto { get; set; } = string.Empty;

        public decimal? mnt_inicio { get; set; }

        public decimal? mnt_corte { get; set; }

        public decimal? porcentaje { get; set; }

        public bool? activo { get; set; }

        public DateTime? registro_fecha { get; set; }

        public string registro_usuario { get; set; } = string.Empty;
    }
    public class RhCatConceptoTablasLista
    {
        public int total { get; set; }

        public List<RhCatConceptoTablaData> lista { get; set; } = new();
    }
    public class RhCatConceptoTablaListaRequest
    {
        public string? cod_concepto { get; set; }
    }
    public class RhCatConceptoTablaGuardarRequest
    {
        public RhCatConceptoTablaData tabla { get; set; } = new();
    }
    public class RhCatConceptoTablaEliminarRequest
    {
        public string? cod_concepto { get; set; }

        public long? tabla_id { get; set; }
    }
    public class RhCatConceptoPersonasActualizarRequest
    {
        public string? cod_concepto { get; set; }
    }

    public class RhCatConceptoCodigoValidarRequest
    {
        public string? cod_concepto { get; set; }
    }

    public class RhCatConceptoEntidadBusquedaRequest
    {
        public string? texto { get; set; }
    }

    public class RhCatConceptoEntidadBusquedaData
    {
        public string cod_er { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string nombre_corto { get; set; } = string.Empty;
    }

}
