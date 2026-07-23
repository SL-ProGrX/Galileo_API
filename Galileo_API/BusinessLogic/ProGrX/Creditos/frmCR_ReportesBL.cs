using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCRReportesBL
    {
        private readonly FrmCRReportesSeguridadDB _seguridadDb;
        private readonly FrmCRReportesConfiguracionDB _configuracionDb;
        private readonly FrmCRReportesInformesDB _informesDb;

        public FrmCRReportesBL(IConfiguration config)
        {
            _seguridadDb = new FrmCRReportesSeguridadDB(config);
            _configuracionDb = new FrmCRReportesConfiguracionDB(config);
            _informesDb = new FrmCRReportesInformesDB(config);
        }

        #region Seguridad - Grupos

        public ErrorDto<CrReportesSeguridadGruposLista> CR_Reportes_Seguridad_Grupos_Lista_Obtener(int CodEmpresa,string parametros)
        {
            var filtros = ParseFiltros(parametros);
            return _seguridadDb.CR_Reportes_Seguridad_Grupos_Lista_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<List<CrReportesSeguridadGrupoData>> CR_Reportes_Seguridad_Grupos_Lista_Export(int CodEmpresa,string parametros)
        {
            var filtros = ParseFiltros(parametros);
            return _seguridadDb.CR_Reportes_Seguridad_Grupos_Lista_Export(CodEmpresa, filtros);
        }

        public ErrorDto CR_Reportes_Seguridad_Grupos_Guardar(int CodEmpresa,string usuario,CrReportesSeguridadGrupoData grupo)
        {
            return _seguridadDb.CR_Reportes_Seguridad_Grupos_Guardar(CodEmpresa, usuario, grupo);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Seguridad_Grupos_Dropdown_Obtener(int CodEmpresa)
        {
            return _seguridadDb.CR_Reportes_Seguridad_Grupos_Dropdown_Obtener(CodEmpresa);
        }

        #endregion

        #region Seguridad - Miembros

        public ErrorDto<CrReportesSeguridadMiembrosLista> CR_Reportes_Seguridad_Miembros_Lista_Obtener(int CodEmpresa,int codGrupo,string parametros)
        {
            var filtros = ParseFiltros(parametros);
            return _seguridadDb.CR_Reportes_Seguridad_Miembros_Lista_Obtener(CodEmpresa, codGrupo, filtros);
        }

        public ErrorDto<List<CrReportesSeguridadMiembroData>> CR_Reportes_Seguridad_Miembros_Lista_Export( int CodEmpresa,int codGrupo,string parametros)
        {
            var filtros = ParseFiltros(parametros);
            return _seguridadDb.CR_Reportes_Seguridad_Miembros_Lista_Export(CodEmpresa, codGrupo, filtros);
        }

        public ErrorDto CR_Reportes_Seguridad_Miembros_Actualizar(int CodEmpresa,CrReportesSeguridadMiembroActualizarRequest request)
        {
            return _seguridadDb.CR_Reportes_Seguridad_Miembros_Actualizar(CodEmpresa, request);
        }

        #endregion

        #region Seguridad - Informes Autorizados

        public ErrorDto<CrReportesSeguridadReportesLista> CR_Reportes_Seguridad_Reportes_Lista_Obtener(int CodEmpresa,int codGrupo,string parametros)
        {
            var filtros = ParseFiltros(parametros);
            return _seguridadDb.CR_Reportes_Seguridad_Reportes_Lista_Obtener(CodEmpresa, codGrupo, filtros);
        }

        public ErrorDto<List<CrReportesSeguridadReporteData>> CR_Reportes_Seguridad_Reportes_Lista_Export( int CodEmpresa,int codGrupo,string parametros)
        {
            var filtros = ParseFiltros(parametros);
            return _seguridadDb.CR_Reportes_Seguridad_Reportes_Lista_Export(CodEmpresa, codGrupo, filtros);
        }

        public ErrorDto CR_Reportes_Seguridad_Reportes_Actualizar(int CodEmpresa,CrReportesSeguridadReporteActualizarRequest request)
        {
            return _seguridadDb.CR_Reportes_Seguridad_Reportes_Actualizar(CodEmpresa, request);
        }

        #endregion

        #region Helpers

        private static FiltrosLazyLoadData ParseFiltros(string parametros)
        {
            return JsonConvert.DeserializeObject<FiltrosLazyLoadData>(parametros)
                   ?? new FiltrosLazyLoadData();
        }

        #endregion
        #region Configuración - Grupos

        public ErrorDto<CrReportesConfigGruposLista> CR_Reportes_Config_Grupos_Lista_Obtener(int CodEmpresa,string parametros)
        {
            var filtros = ParseFiltros(parametros);
            return _configuracionDb.CR_Reportes_Config_Grupos_Lista_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<List<CrReportesConfigGrupoData>> CR_Reportes_Config_Grupos_Lista_Export(int CodEmpresa,string parametros)
        {
            var filtros = ParseFiltros(parametros);
            return _configuracionDb.CR_Reportes_Config_Grupos_Lista_Export(CodEmpresa, filtros);
        }

        public ErrorDto CR_Reportes_Config_Grupos_Guardar(int CodEmpresa,string usuario,CrReportesConfigGrupoData grupo)
        {
            return _configuracionDb.CR_Reportes_Config_Grupos_Guardar(CodEmpresa, usuario, grupo);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Config_Grupos_Dropdown_Obtener( int CodEmpresa)
        {
            return _configuracionDb.CR_Reportes_Config_Grupos_Dropdown_Obtener(CodEmpresa);
        }

        #endregion

        #region Configuración - Miembros

        public ErrorDto<CrReportesConfigMiembrosLista> CR_Reportes_Config_Miembros_Lista_Obtener(int CodEmpresa,string codGrupo,string parametros)
        {
            var filtros = ParseFiltros(parametros);
            return _configuracionDb.CR_Reportes_Config_Miembros_Lista_Obtener(CodEmpresa, codGrupo, filtros);
        }

        public ErrorDto<List<CrReportesConfigMiembroData>> CR_Reportes_Config_Miembros_Lista_Export(int CodEmpresa,string codGrupo,string parametros)
        {
            var filtros = ParseFiltros(parametros);
            return _configuracionDb.CR_Reportes_Config_Miembros_Lista_Export(CodEmpresa, codGrupo, filtros);
        }

        public ErrorDto CR_Reportes_Config_Miembros_Actualizar(int CodEmpresa,CrReportesConfigMiembroActualizarRequest request)
        {
            return _configuracionDb.CR_Reportes_Config_Miembros_Actualizar(CodEmpresa, request);
        }

        #endregion

        #region Configuración - Informes

        public ErrorDto<CrReportesConfigReportesLista> CR_Reportes_Config_Reportes_Lista_Obtener(int CodEmpresa, string parametros)
        {
            var filtros = ParseFiltros(parametros);
            return _configuracionDb.CR_Reportes_Config_Reportes_Lista_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<List<CrReportesConfigReporteData>> CR_Reportes_Config_Reportes_Lista_Export(int CodEmpresa,string parametros)
        {
            var filtros = ParseFiltros(parametros);
            return _configuracionDb.CR_Reportes_Config_Reportes_Lista_Export(CodEmpresa, filtros);
        }

        public ErrorDto CR_Reportes_Config_Reportes_Actualizar_Lista(int CodEmpresa,CrReportesConfigReportesActualizarListaRequest request)
        {
            return _configuracionDb.CR_Reportes_Config_Reportes_Actualizar_Lista(CodEmpresa, request);
        }

        public ErrorDto CR_Reportes_Config_Reportes_Guardar(int CodEmpresa, CrReportesConfigReporteGuardarRequest request)
        {
            return _configuracionDb.CR_Reportes_Config_Reportes_Guardar(CodEmpresa, request);
        }

        #endregion
        #region Informes - Panel Base

        public static ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_TipoReporte_Dropdown_Obtener()
        {
            return FrmCRReportesInformesDB.CR_Reportes_Informes_TipoReporte_Dropdown_Obtener();
        }

        public static ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_BaseFecha_Dropdown_Obtener()
        {
            return FrmCRReportesInformesDB.CR_Reportes_Informes_BaseFecha_Dropdown_Obtener();
        }

        public static ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_EstadoSolicitud_Dropdown_Obtener()
        {
            return FrmCRReportesInformesDB.CR_Reportes_Informes_EstadoSolicitud_Dropdown_Obtener();
        }

        public static ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_EstadoOperacion_Dropdown_Obtener()
        {
            return FrmCRReportesInformesDB.CR_Reportes_Informes_EstadoOperacion_Dropdown_Obtener();
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Oficinas_Dropdown_Obtener(int CodEmpresa, string? filtro)
        {
            return _informesDb.CR_Reportes_Informes_Oficinas_Dropdown_Obtener(CodEmpresa, filtro);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_EstadoPersona_Dropdown_Obtener(int CodEmpresa, string? filtro)
        {
            return _informesDb.CR_Reportes_Informes_EstadoPersona_Dropdown_Obtener(CodEmpresa, filtro);
        }

        #endregion

        #region Informes - General

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Divisas_Dropdown_Obtener(int CodEmpresa, string? filtro)
        {
            return _informesDb.CR_Reportes_Informes_Divisas_Dropdown_Obtener(CodEmpresa, filtro);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Garantias_Dropdown_Obtener(int CodEmpresa, string? filtro)
        {
            return _informesDb.CR_Reportes_Informes_Garantias_Dropdown_Obtener(CodEmpresa, filtro);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Comites_Dropdown_Obtener(int CodEmpresa, string? filtro)
        {
            return _informesDb.CR_Reportes_Informes_Comites_Dropdown_Obtener(CodEmpresa, filtro);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_GruposUsuario_Dropdown_Obtener(int CodEmpresa, string? filtro)
        {
            return _informesDb.CR_Reportes_Informes_GruposUsuario_Dropdown_Obtener(CodEmpresa, filtro);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Recursos_Dropdown_Obtener(int CodEmpresa,string? codigo, bool todasLineas,string? filtro)
        {
            return _informesDb.CR_Reportes_Informes_Recursos_Dropdown_Obtener(CodEmpresa, codigo, todasLineas, filtro);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Destinos_Dropdown_Obtener( int CodEmpresa,string? codigo,bool todasLineas,string? filtro)
        {
            return _informesDb.CR_Reportes_Informes_Destinos_Dropdown_Obtener(CodEmpresa, codigo, todasLineas, filtro);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Instituciones_Dropdown_Obtener(int CodEmpresa, string? filtro)
        {
            return _informesDb.CR_Reportes_Informes_Instituciones_Dropdown_Obtener(CodEmpresa, filtro);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Deductoras_Dropdown_Obtener(int CodEmpresa,int codInstitucion,string? filtro)
        {
            return _informesDb.CR_Reportes_Informes_Deductoras_Dropdown_Obtener(CodEmpresa, codInstitucion, filtro);
        }

        public static ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Especial_Dropdown_Obtener()
        {
            return FrmCRReportesInformesDB.CR_Reportes_Informes_Especial_Dropdown_Obtener();
        }

        #endregion

        #region Informes - Adicionales

        public static ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Cobro_Dropdown_Obtener()
        {
            return FrmCRReportesInformesDB.CR_Reportes_Informes_Cobro_Dropdown_Obtener();
        }

        public static ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Proceso_Dropdown_Obtener()
        {
            return FrmCRReportesInformesDB.CR_Reportes_Informes_Proceso_Dropdown_Obtener();
        }

        public static ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_TipoOperacion_Dropdown_Obtener()
        {
            return FrmCRReportesInformesDB.CR_Reportes_Informes_TipoOperacion_Dropdown_Obtener();
        }

        public static ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_TiposTasas_Dropdown_Obtener()
        {
            return FrmCRReportesInformesDB.CR_Reportes_Informes_TiposTasas_Dropdown_Obtener();
        }

        public static ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Autorizaciones_Dropdown_Obtener()
        {
            return FrmCRReportesInformesDB.CR_Reportes_Informes_Autorizaciones_Dropdown_Obtener();
        }

        public static ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Signos_Dropdown_Obtener()
        {
            return FrmCRReportesInformesDB.CR_Reportes_Informes_Signos_Dropdown_Obtener();
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Gestiona_Dropdown_Obtener(int CodEmpresa, string? filtro)
        {
            return _informesDb.CR_Reportes_Informes_Gestiona_Dropdown_Obtener(CodEmpresa, filtro);
        }

        #endregion

        #region Informes - F1

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Provincias_Dropdown_Obtener(int CodEmpresa, string? filtro)
        {
            return _informesDb.CR_Reportes_Informes_Provincias_Dropdown_Obtener(CodEmpresa, filtro);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Cantones_Dropdown_Obtener(int CodEmpresa, string provincia, string? filtro)
        {
            return _informesDb.CR_Reportes_Informes_Cantones_Dropdown_Obtener(CodEmpresa, provincia, filtro);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Distritos_Dropdown_Obtener(int CodEmpresa,string provincia,string canton,string? filtro)
        {
            return _informesDb.CR_Reportes_Informes_Distritos_Dropdown_Obtener(CodEmpresa, provincia, canton, filtro);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Zonas_Dropdown_Obtener(int CodEmpresa, string? filtro)
        {
            return _informesDb.CR_Reportes_Informes_Zonas_Dropdown_Obtener(CodEmpresa, filtro);
        }

        #endregion

        #region Informes - F2

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Profesiones_Dropdown_Obtener(int CodEmpresa, string? filtro)
        {
            return _informesDb.CR_Reportes_Informes_Profesiones_Dropdown_Obtener(CodEmpresa, filtro);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Sectores_Dropdown_Obtener(int CodEmpresa, string? filtro)
        {
            return _informesDb.CR_Reportes_Informes_Sectores_Dropdown_Obtener(CodEmpresa, filtro);
        }

        public static ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Sexo_Dropdown_Obtener()
        {
            return FrmCRReportesInformesDB.CR_Reportes_Informes_Sexo_Dropdown_Obtener();
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_EstadoCivil_Dropdown_Obtener(int CodEmpresa, string? filtro)
        {
            return _informesDb.CR_Reportes_Informes_EstadoCivil_Dropdown_Obtener(CodEmpresa, filtro);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_CondicionLaboral_Dropdown_Obtener(int CodEmpresa, string? filtro)
        {
            return _informesDb.CR_Reportes_Informes_CondicionLaboral_Dropdown_Obtener(CodEmpresa, filtro);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Ejecutivos_Dropdown_Obtener(int CodEmpresa, string? filtro)
        {
            return _informesDb.CR_Reportes_Informes_Ejecutivos_Dropdown_Obtener(CodEmpresa, filtro);
        }

        #endregion

        #region Informes - Adicional Dinámico

        public static ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_CausasTipos_Dropdown_Obtener()
        {
            return FrmCRReportesInformesDB.CR_Reportes_Informes_CausasTipos_Dropdown_Obtener();
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Causas_Dropdown_Obtener(int CodEmpresa, string tipo, string? filtro)
        {
            return _informesDb.CR_Reportes_Informes_Causas_Dropdown_Obtener(CodEmpresa, tipo, filtro);
        }

        public static ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_RequisitoMarca_Dropdown_Obtener()
        {
            return FrmCRReportesInformesDB.CR_Reportes_Informes_RequisitoMarca_Dropdown_Obtener();
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Requisitos_Dropdown_Obtener(int CodEmpresa, string? codigo, bool todasLineas, string? filtro)
        {
            return _informesDb.CR_Reportes_Informes_Requisitos_Dropdown_Obtener(CodEmpresa, codigo, todasLineas, filtro);
        }

        public static ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Cortes_Dropdown_Obtener()
        {
            return FrmCRReportesInformesDB.CR_Reportes_Informes_Cortes_Dropdown_Obtener();
        }

        #endregion

        #region Informes - F4

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Lineas_Buscar(int CodEmpresa, string? filtro)
        {
            return _informesDb.CR_Reportes_Informes_Lineas_Buscar(CodEmpresa, filtro);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_UnidadProgramatica_Buscar(int CodEmpresa,int codInstitucion, string? filtro)
        {
            return _informesDb.CR_Reportes_Informes_UnidadProgramatica_Buscar(CodEmpresa, codInstitucion, filtro);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_UnidadTrabajo_Buscar(int CodEmpresa,int codInstitucion,string? codDepartamento,string? filtro)
        {
            return _informesDb.CR_Reportes_Informes_UnidadTrabajo_Buscar(CodEmpresa, codInstitucion, codDepartamento, filtro);
        }

        #endregion

        #region Informes - Árbol

        public ErrorDto<List<CrReportesInformesArbolDto>> CR_Reportes_Informes_Arbol_Obtener(int CodEmpresa)
        {
            return _informesDb.CR_Reportes_Informes_Arbol_Obtener(CodEmpresa);
        }

        public ErrorDto<CrReportesInformesArbolDto> CR_Reportes_Informes_Reporte_Obtener(int CodEmpresa, int idReporte, string usuario)
        {
            return _informesDb.CR_Reportes_Informes_Reporte_Obtener(CodEmpresa, idReporte, usuario);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Usuarios_Dropdown_Obtener(int CodEmpresa,string? filtro)
        {
            return _informesDb.CR_Reportes_Informes_Usuarios_Dropdown_Obtener(CodEmpresa, filtro);
        }
        #endregion
        #region Informes - Generar Reporte

        public ErrorDto<CrReportesInformesGenerarResult> CR_Reportes_Informes_Generar(int CodEmpresa, CrReportesInformesGenerarRequest request)
        {
            return _informesDb.CR_Reportes_Informes_Generar(CodEmpresa, request);
        }

        #endregion
    }
}