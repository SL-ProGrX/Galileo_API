namespace Galileo_API.Models.ProGrX.Creditos
{
    #region Seguridad - Grupos

    public class CrReportesSeguridadGruposLista
    {
        public int total { get; set; }
        public List<CrReportesSeguridadGrupoData> lista { get; set; } = new();
    }

    public class CrReportesSeguridadGrupoData
    {
        public int? cod_grupo { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public bool? activo { get; set; }
        public bool? isNew { get; set; }
    }

    #endregion

    #region Seguridad - Miembros

    public class CrReportesSeguridadMiembrosLista
    {
        public int total { get; set; }
        public List<CrReportesSeguridadMiembroData> lista { get; set; } = new();
    }

    public class CrReportesSeguridadMiembroData
    {
        public string usuario { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool asignado { get; set; }
    }

    public class CrReportesSeguridadMiembroActualizarRequest
    {
        public int? cod_grupo { get; set; }
        public string usuario { get; set; } = string.Empty;
        public bool? asignado { get; set; }
        public string usuario_sesion { get; set; } = string.Empty;
    }

    #endregion

    #region Seguridad - Informes Autorizados

    public class CrReportesSeguridadReportesLista
    {
        public int total { get; set; }
        public List<CrReportesSeguridadReporteData> lista { get; set; } = new();
    }

    public class CrReportesSeguridadReporteData
    {
        public int id { get; set; }
        public string tipo { get; set; } = string.Empty;
        public string reporte { get; set; } = string.Empty;
        public bool autorizado { get; set; }
    }

    public class CrReportesSeguridadReporteActualizarRequest
    {
        public int? cod_grupo { get; set; }
        public int? id { get; set; }
        public bool? autorizado { get; set; }
        public string usuario_sesion { get; set; } = string.Empty;
    }

    #endregion
    #region Configuración - Grupos

    public class CrReportesConfigGruposLista
    {
        public int total { get; set; }
        public List<CrReportesConfigGrupoData> lista { get; set; } = new();
    }

    public class CrReportesConfigGrupoData
    {
        public string cod_grupo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool? isNew { get; set; }
    }

    #endregion

    #region Configuración - Miembros

    public class CrReportesConfigMiembrosLista
    {
        public int total { get; set; }
        public List<CrReportesConfigMiembroData> lista { get; set; } = new();
    }

    public class CrReportesConfigMiembroData
    {
        public string usuario { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool asignado { get; set; }
    }

    public class CrReportesConfigMiembroActualizarRequest
    {
        public string cod_grupo { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public bool? asignado { get; set; }
        public string usuario_sesion { get; set; } = string.Empty;
    }

    #endregion

    #region Configuración - Informes

    public class CrReportesConfigReportesLista
    {
        public int total { get; set; }
        public List<CrReportesConfigReporteData> lista { get; set; } = new();
    }
    public class CrReportesConfigReporteGuardarRequest
    {
        public string usuario_sesion { get; set; } = string.Empty;
        public string clave_edicion { get; set; } = string.Empty;
        public CrReportesConfigReporteData reporte { get; set; } = new();
    }
    public class CrReportesConfigReporteData
    {
        public int? id { get; set; }
        public string tipo { get; set; } = string.Empty;
        public string reporte { get; set; } = string.Empty;
        public string prefijo { get; set; } = string.Empty;
        public short? adicional { get; set; }
        public short? seguridad { get; set; }
        public bool? isNew { get; set; }
    }

    public class CrReportesConfigReportesActualizarListaRequest
    {
        public string usuario_sesion { get; set; } = string.Empty;
        public string clave_edicion { get; set; } = string.Empty;
    }

    #endregion
    #region Informes - Panel Base

    public class CrReportesInformesFiltroBaseDto
    {
        public string tipo_reporte { get; set; } = "D";
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_corte { get; set; }
        public string base_reporte { get; set; } = "S";
        public string estado_solicitud { get; set; } = string.Empty;
        public string estado_operacion { get; set; } = string.Empty;
        public string estado_persona { get; set; } = string.Empty;
        public string oficina { get; set; } = string.Empty;
        public bool? todas_fechas { get; set; }
    }

    public class CrReportesInformesDropdownRequest
    {
        public string tipo { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    #endregion

    #region Informes - General

    public class CrReportesInformesFiltroGeneralDto
    {
        public string divisa { get; set; } = string.Empty;
        public string garantia { get; set; } = string.Empty;
        public string comite { get; set; } = string.Empty;
        public string linea { get; set; } = string.Empty;
        public string linea_desc { get; set; } = string.Empty;
        public bool todas_lineas { get; set; } = true;
        public string destino { get; set; } = string.Empty;
        public string recurso { get; set; } = string.Empty;
        public string institucion { get; set; } = string.Empty;
        public string deductora { get; set; } = string.Empty;
        public string especial { get; set; } = string.Empty;
    }

    #endregion

    #region Informes - Adicionales

    public class CrReportesInformesFiltroAdicionalesDto
    {
        public string cobro_en { get; set; } = string.Empty;
        public string proceso { get; set; } = string.Empty;
        public string gestor_cobros_ext { get; set; } = string.Empty;
        public string tipos_de { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string autorizaciones { get; set; } = string.Empty;

        public int plazo_desde { get; set; } = 1;
        public int plazo_hasta { get; set; } = 999;
        public bool todos_plazos { get; set; } = true;

        public decimal? tasa_desde { get; set; }
        public decimal tasa_hasta { get; set; } = 100;
        public bool todas_tasas { get; set; } = true;

        public string primer_deduccion_operador { get; set; } = "=";
        public string primer_deduccion { get; set; } = string.Empty;
        public bool todas_primer_deduccion { get; set; } = true;

        public string ult_mov_operador { get; set; } = "=";
        public string ult_mov { get; set; } = string.Empty;
        public bool todas_ult_mov { get; set; } = true;
    }

    #endregion

    #region Informes - F1

    public class CrReportesInformesFiltroF1Dto
    {
        public string usuario { get; set; } = string.Empty;
        public string zona { get; set; } = string.Empty;

        public string provincia { get; set; } = string.Empty;
        public bool todas_provincias { get; set; } = true;

        public string canton { get; set; } = string.Empty;
        public bool todos_cantones { get; set; } = true;

        public string distrito { get; set; } = string.Empty;
        public bool todos_distritos { get; set; } = true;

        public string unidad_programatica { get; set; } = string.Empty;
        public string unidad_programatica_desc { get; set; } = string.Empty;
        public bool todas_unidades_programaticas { get; set; } = true;

        public string unidad_trabajo { get; set; } = string.Empty;
        public string unidad_trabajo_desc { get; set; } = string.Empty;
        public bool todas_unidades_trabajo { get; set; } = true;
    }

    #endregion

    #region Informes - F2

    public class CrReportesInformesFiltroF2Dto
    {
        public string profesion { get; set; } = string.Empty;
        public string sector { get; set; } = string.Empty;
        public string sexo { get; set; } = string.Empty;
        public string estado_civil { get; set; } = string.Empty;
        public string condicion_laboral { get; set; } = string.Empty;
        public string ejecutivo_colocador { get; set; } = string.Empty;
        public string ejecutivo_colocador_desc { get; set; } = string.Empty;
    }

    #endregion

    #region Informes - Árbol y Reporte

    public class CrReportesInformesArbolDto
    {
        public int id { get; set; }
        public string tipo { get; set; } = string.Empty;
        public string reporte { get; set; } = string.Empty;
        public string prefijo { get; set; } = string.Empty;
        public int adicional { get; set; }
        public int seguridad { get; set; }
        public string categoria { get; set; } = string.Empty;
        public bool tiene_acceso { get; set; }
    }

    public class CrReportesInformesGenerarRequest
    {
        public int? id_reporte { get; set; }
        public string usuario_sesion { get; set; } = string.Empty;

        public CrReportesInformesFiltroBaseDto base_filtros { get; set; } = new();
        public CrReportesInformesFiltroGeneralDto general { get; set; } = new();
        public CrReportesInformesFiltroAdicionalesDto adicionales { get; set; } = new();
        public CrReportesInformesFiltroF1Dto f1 { get; set; } = new();
        public CrReportesInformesFiltroF2Dto f2 { get; set; } = new();
        public CrReportesInformesFiltroF3Dto f3 { get; set; } = new();
    }

    #endregion
    #region Informes - F3

    public class CrReportesInformesFiltroF3Dto
    {
        public int? tipo { get; set; }
        public Dictionary<string, string> valores { get; set; } = new();
    }
    public class CrReportesInformesGenerarResult
    {
        public string tipo { get; set; } = string.Empty;
        public string reporte { get; set; } = string.Empty;
        public string prefijo { get; set; } = string.Empty;
        public string reporte_file_name { get; set; } = string.Empty;
        public string selection_formula { get; set; } = string.Empty;
        public string fx_fecha { get; set; } = string.Empty;
        public string fx_empresa { get; set; } = string.Empty;
        public string fx_usuario { get; set; } = string.Empty;
        public string fx_titulo { get; set; } = string.Empty;
        public string fx_subtitulo { get; set; } = string.Empty;
        public string fx_filtro { get; set; } = string.Empty;
    }
    #endregion
}