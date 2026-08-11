using Galileo.Models;

namespace Galileo_API.Models.ProGrX.Conciliacion
{
    public class CcReportesEstudioPeriodoDto
    {
        public int id_per_historico { get; set; }
        public int anio { get; set; }
        public int mes { get; set; }
        public string descripcion { get; set; } = string.Empty;
    }
    public class CcReportesEstudioAuxiliaresInicialDto
    {
        public string empresa_nombre_corto { get; set; } = string.Empty;

        public List<CcReportesEstudioPeriodoDto> periodos { get; set; } = new();

        public List<DropDownListaGenericaModel> garantias { get; set; } = new();

        public List<DropDownListaGenericaModel> divisas { get; set; } = new();

        public List<DropDownListaGenericaModel> operadoras { get; set; } = new();

        public List<DropDownListaGenericaModel> grupos_fondos { get; set; } = new();
    }
    public class CcReportesEstudioBusquedaDto
    {
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }
    public class CcReportesEstudioCuentaDto
    {
        public string cod_cuenta { get; set; } = string.Empty;
        public string cod_cuenta_mask { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string cod_divisa { get; set; } = string.Empty;
        public int cod_contabilidad { get; set; }
    }
    public class CcReportesEstudioPlanesRequest
    {
        public string cod_operadora { get; set; } = string.Empty;
        public string texto { get; set; } = string.Empty;
    }
    public class CcReportesEstudioFiltrosComunesDto
    {
        public string cod_institucion { get; set; } = string.Empty;
        public string institucion_descripcion { get; set; } = string.Empty;

        public string cod_cuenta { get; set; } = string.Empty;
        public string cuenta_descripcion { get; set; } = string.Empty;

        public string cod_divisa { get; set; } = string.Empty;

        public bool solo_lineas_con_contenido { get; set; } = true;
    }
    public class CcReportesEstudioFondosFiltrosDto
    {
        public string cod_operadora { get; set; } = string.Empty;
        public string operadora_descripcion { get; set; } = string.Empty;

        public string cod_grupo { get; set; } = string.Empty;
        public string grupo_descripcion { get; set; } = string.Empty;

        public string cod_plan { get; set; } = string.Empty;
        public string plan_descripcion { get; set; } = string.Empty;
    }
    public class CcReportesEstudioCreditoFiltrosDto
    {
        public string codigo { get; set; } = string.Empty;
        public string codigo_descripcion { get; set; } = string.Empty;

        public string garantia { get; set; } = string.Empty;
        public string garantia_descripcion { get; set; } = string.Empty;

        public string cod_destino { get; set; } = string.Empty;
        public string destino_descripcion { get; set; } = string.Empty;

        public string cod_recurso { get; set; } = string.Empty;
        public string recurso_descripcion { get; set; } = string.Empty;
        public bool? usar_reporte_general { get; set; }
    }
    public class CcReportesEstudioAuxiliarGenerarRequest
    {
        public int? id_per_historico { get; set; }
        public string tipo_auxiliar { get; set; } = string.Empty;
        public string codigo_informe { get; set; } = string.Empty;
        public string codigo_filtro { get; set; } = string.Empty;

        public CcReportesEstudioFiltrosComunesDto comunes { get; set; } = new();
        public CcReportesEstudioFondosFiltrosDto fondos { get; set; } = new();
        public CcReportesEstudioCreditoFiltrosDto credito { get; set; } = new();

        public string usuario_sesion { get; set; } = string.Empty;
        public string gstr_niveles { get; set; } = string.Empty;
    }
    public class CcReportesEstudioAuxiliarGenerarResult
    {
        public string nombre_reporte { get; set; } = string.Empty;
        public string folder { get; set; } = string.Empty;
        public string cod_reporte { get; set; } = "P";
        public string titulo_ventana { get; set; } = string.Empty;
        public string nombre_archivo { get; set; } = string.Empty;
        public List<CcReportesEstudioParametroDto> parametros { get; set; } = new();
    }
    public class CcReportesEstudioParametroDto
    {
        public string nombre { get; set; } = string.Empty;
        public object? valor { get; set; }
    }
    public class CcReportesEstudioPeriodoData
    {
        public int id_per_historico { get; set; }
        public int anio { get; set; }
        public int mes { get; set; }
        public string descripcion { get; set; } = string.Empty;
    }
    public class CcReportesEstudioEspecialRequest
    {
        public short? tipo { get; set; }
        public short? anio { get; set; }
        public short? mes { get; set; }
        public bool? detallado { get; set; }
        public List<string> carteras { get; set; } = [];
        public string usuario_sesion { get; set; } = string.Empty;
    }

    public class CcReportesEstudioEspecialReporteDto
    {
        public string tipo_salida { get; set; } = string.Empty;

        public string nombre_archivo { get; set; } = string.Empty;
        public List<Dictionary<string, object?>> filas { get; set; } = new();

        public string nombre_reporte { get; set; } = string.Empty;
        public string folder { get; set; } = string.Empty;
        public string cod_reporte { get; set; } = string.Empty;
        public string titulo_ventana { get; set; } = string.Empty;
        public List<CcReportesEstudioParametroDto> parametros { get; set; } = new();
    }
}