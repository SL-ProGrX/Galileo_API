using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCRReportesController : ControllerBase
    {
        private readonly FrmCRReportesBL BL;

        public FrmCRReportesController(IConfiguration config)
        {
            BL = new FrmCRReportesBL(config);
        }

        #region Seguridad - Grupos

        [Authorize]
        [HttpGet("CR_Reportes_Seguridad_Grupos_Lista_Obtener")]
        public ErrorDto<CrReportesSeguridadGruposLista> CR_Reportes_Seguridad_Grupos_Lista_Obtener( int CodEmpresa,string parametros)
        {
            return BL.CR_Reportes_Seguridad_Grupos_Lista_Obtener(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpGet("CR_Reportes_Seguridad_Grupos_Lista_Export")]
        public ErrorDto<List<CrReportesSeguridadGrupoData>> CR_Reportes_Seguridad_Grupos_Lista_Export(int CodEmpresa, string parametros)
        {
            return BL.CR_Reportes_Seguridad_Grupos_Lista_Export(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpPost("CR_Reportes_Seguridad_Grupos_Guardar")]
        public ErrorDto CR_Reportes_Seguridad_Grupos_Guardar(int CodEmpresa, string usuario, [FromBody] CrReportesSeguridadGrupoData grupo)
        {
            return BL.CR_Reportes_Seguridad_Grupos_Guardar(CodEmpresa, usuario, grupo);
        }

        [Authorize]
        [HttpGet("CR_Reportes_Seguridad_Grupos_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Seguridad_Grupos_Dropdown_Obtener( int CodEmpresa)
        {
            return BL.CR_Reportes_Seguridad_Grupos_Dropdown_Obtener(CodEmpresa);
        }

        #endregion

        #region Seguridad - Miembros

        [Authorize]
        [HttpGet("CR_Reportes_Seguridad_Miembros_Lista_Obtener")]
        public ErrorDto<CrReportesSeguridadMiembrosLista> CR_Reportes_Seguridad_Miembros_Lista_Obtener( int CodEmpresa,int codGrupo,string parametros)
        {
            return BL.CR_Reportes_Seguridad_Miembros_Lista_Obtener(CodEmpresa, codGrupo, parametros);
        }

        [Authorize]
        [HttpGet("CR_Reportes_Seguridad_Miembros_Lista_Export")]
        public ErrorDto<List<CrReportesSeguridadMiembroData>> CR_Reportes_Seguridad_Miembros_Lista_Export(int CodEmpresa,int codGrupo,string parametros)
        {
            return BL.CR_Reportes_Seguridad_Miembros_Lista_Export(CodEmpresa, codGrupo, parametros);
        }

        [Authorize]
        [HttpPost("CR_Reportes_Seguridad_Miembros_Actualizar")]
        public ErrorDto CR_Reportes_Seguridad_Miembros_Actualizar(int CodEmpresa,[FromBody] CrReportesSeguridadMiembroActualizarRequest request)
        {
            return BL.CR_Reportes_Seguridad_Miembros_Actualizar(CodEmpresa, request);
        }

        #endregion

        #region Seguridad - Informes Autorizados

        [Authorize]
        [HttpGet("CR_Reportes_Seguridad_Reportes_Lista_Obtener")]
        public ErrorDto<CrReportesSeguridadReportesLista> CR_Reportes_Seguridad_Reportes_Lista_Obtener(int CodEmpresa, int codGrupo,string parametros)
        {
            return BL.CR_Reportes_Seguridad_Reportes_Lista_Obtener(CodEmpresa, codGrupo, parametros);
        }

        [Authorize]
        [HttpGet("CR_Reportes_Seguridad_Reportes_Lista_Export")]
        public ErrorDto<List<CrReportesSeguridadReporteData>> CR_Reportes_Seguridad_Reportes_Lista_Export(int CodEmpresa, int codGrupo, string parametros)
        {
            return BL.CR_Reportes_Seguridad_Reportes_Lista_Export(CodEmpresa, codGrupo, parametros);
        }

        [Authorize]
        [HttpPost("CR_Reportes_Seguridad_Reportes_Actualizar")]
        public ErrorDto CR_Reportes_Seguridad_Reportes_Actualizar(int CodEmpresa,[FromBody] CrReportesSeguridadReporteActualizarRequest request)
        {
            return BL.CR_Reportes_Seguridad_Reportes_Actualizar(CodEmpresa, request);
        }

        #endregion
        #region Configuración - Grupos

        [Authorize]
        [HttpGet("CR_Reportes_Config_Grupos_Lista_Obtener")]
        public ErrorDto<CrReportesConfigGruposLista> CR_Reportes_Config_Grupos_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return BL.CR_Reportes_Config_Grupos_Lista_Obtener(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpGet("CR_Reportes_Config_Grupos_Lista_Export")]
        public ErrorDto<List<CrReportesConfigGrupoData>> CR_Reportes_Config_Grupos_Lista_Export(int CodEmpresa,string parametros)
        {
            return BL.CR_Reportes_Config_Grupos_Lista_Export(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpPost("CR_Reportes_Config_Grupos_Guardar")]
        public ErrorDto CR_Reportes_Config_Grupos_Guardar( int CodEmpresa,string usuario,[FromBody] CrReportesConfigGrupoData grupo)
        {
            return BL.CR_Reportes_Config_Grupos_Guardar(CodEmpresa, usuario, grupo);
        }

        [Authorize]
        [HttpGet("CR_Reportes_Config_Grupos_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Config_Grupos_Dropdown_Obtener( int CodEmpresa)
        {
            return BL.CR_Reportes_Config_Grupos_Dropdown_Obtener(CodEmpresa);
        }

        #endregion

        #region Configuración - Miembros

        [Authorize]
        [HttpGet("CR_Reportes_Config_Miembros_Lista_Obtener")]
        public ErrorDto<CrReportesConfigMiembrosLista> CR_Reportes_Config_Miembros_Lista_Obtener(int CodEmpresa,string codGrupo, string parametros)
        {
            return BL.CR_Reportes_Config_Miembros_Lista_Obtener(CodEmpresa, codGrupo, parametros);
        }

        [Authorize]
        [HttpGet("CR_Reportes_Config_Miembros_Lista_Export")]
        public ErrorDto<List<CrReportesConfigMiembroData>> CR_Reportes_Config_Miembros_Lista_Export(int CodEmpresa,string codGrupo,string parametros)
        {
            return BL.CR_Reportes_Config_Miembros_Lista_Export(CodEmpresa, codGrupo, parametros);
        }

        [Authorize]
        [HttpPost("CR_Reportes_Config_Miembros_Actualizar")]
        public ErrorDto CR_Reportes_Config_Miembros_Actualizar(int CodEmpresa,[FromBody] CrReportesConfigMiembroActualizarRequest request)
        {
            return BL.CR_Reportes_Config_Miembros_Actualizar(CodEmpresa, request);
        }

        #endregion

        #region Configuración - Informes

        [Authorize]
        [HttpGet("CR_Reportes_Config_Reportes_Lista_Obtener")]
        public ErrorDto<CrReportesConfigReportesLista> CR_Reportes_Config_Reportes_Lista_Obtener( int CodEmpresa,string parametros)
        {
            return BL.CR_Reportes_Config_Reportes_Lista_Obtener(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpGet("CR_Reportes_Config_Reportes_Lista_Export")]
        public ErrorDto<List<CrReportesConfigReporteData>> CR_Reportes_Config_Reportes_Lista_Export(int CodEmpresa,string parametros)
        {
            return BL.CR_Reportes_Config_Reportes_Lista_Export(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpPost("CR_Reportes_Config_Reportes_Actualizar_Lista")]
        public ErrorDto CR_Reportes_Config_Reportes_Actualizar_Lista(int CodEmpresa,[FromBody] CrReportesConfigReportesActualizarListaRequest request)
        {
            return BL.CR_Reportes_Config_Reportes_Actualizar_Lista(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("CR_Reportes_Config_Reportes_Guardar")]
        public ErrorDto CR_Reportes_Config_Reportes_Guardar(int CodEmpresa, [FromBody] CrReportesConfigReporteGuardarRequest request)
        {
            return BL.CR_Reportes_Config_Reportes_Guardar(CodEmpresa, request);
        }

        #endregion
        #region Informes

        [Authorize]
        [HttpGet("CR_Reportes_Informes_TipoReporte_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_TipoReporte_Dropdown_Obtener()
        {
            return FrmCRReportesBL.CR_Reportes_Informes_TipoReporte_Dropdown_Obtener();
        }

        [Authorize]
        [HttpGet("CR_Reportes_Informes_BaseFecha_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_BaseFecha_Dropdown_Obtener()
        {
            return FrmCRReportesBL.CR_Reportes_Informes_BaseFecha_Dropdown_Obtener();
        }

        [Authorize]
        [HttpGet("CR_Reportes_Informes_EstadoSolicitud_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_EstadoSolicitud_Dropdown_Obtener()
        {
            return FrmCRReportesBL.CR_Reportes_Informes_EstadoSolicitud_Dropdown_Obtener();
        }

        [Authorize]
        [HttpGet("CR_Reportes_Informes_EstadoOperacion_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_EstadoOperacion_Dropdown_Obtener()
        {
            return FrmCRReportesBL.CR_Reportes_Informes_EstadoOperacion_Dropdown_Obtener();
        }

        [Authorize]
        [HttpGet("CR_Reportes_Informes_Oficinas_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Oficinas_Dropdown_Obtener(int CodEmpresa, string? filtro)
        {
            return BL.CR_Reportes_Informes_Oficinas_Dropdown_Obtener(CodEmpresa, filtro);
        }

        [Authorize]
        [HttpGet("CR_Reportes_Informes_EstadoPersona_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_EstadoPersona_Dropdown_Obtener(int CodEmpresa, string? filtro)
        {
            return BL.CR_Reportes_Informes_EstadoPersona_Dropdown_Obtener(CodEmpresa, filtro);
        }

        [Authorize]
        [HttpGet("CR_Reportes_Informes_Divisas_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Divisas_Dropdown_Obtener(int CodEmpresa, string? filtro)
        {
            return BL.CR_Reportes_Informes_Divisas_Dropdown_Obtener(CodEmpresa, filtro);
        }

        [Authorize]
        [HttpGet("CR_Reportes_Informes_Garantias_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Garantias_Dropdown_Obtener(int CodEmpresa, string? filtro)
        {
            return BL.CR_Reportes_Informes_Garantias_Dropdown_Obtener(CodEmpresa, filtro);
        }

        [Authorize]
        [HttpGet("CR_Reportes_Informes_Comites_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Comites_Dropdown_Obtener(int CodEmpresa, string? filtro)
        {
            return BL.CR_Reportes_Informes_Comites_Dropdown_Obtener(CodEmpresa, filtro);
        }

        [Authorize]
        [HttpGet("CR_Reportes_Informes_GruposUsuario_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_GruposUsuario_Dropdown_Obtener(int CodEmpresa, string? filtro)
        {
            return BL.CR_Reportes_Informes_GruposUsuario_Dropdown_Obtener(CodEmpresa, filtro);
        }

        [Authorize]
        [HttpGet("CR_Reportes_Informes_Recursos_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Recursos_Dropdown_Obtener(int CodEmpresa,string? codigo,bool todasLineas,string? filtro)
        {
            return BL.CR_Reportes_Informes_Recursos_Dropdown_Obtener(CodEmpresa, codigo, todasLineas, filtro);
        }

        [Authorize]
        [HttpGet("CR_Reportes_Informes_Destinos_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Destinos_Dropdown_Obtener(int CodEmpresa, string? codigo,bool todasLineas,string? filtro)
        {
            return BL.CR_Reportes_Informes_Destinos_Dropdown_Obtener(CodEmpresa, codigo, todasLineas, filtro);
        }

        [Authorize]
        [HttpGet("CR_Reportes_Informes_Instituciones_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Instituciones_Dropdown_Obtener(int CodEmpresa, string? filtro)
        {
            return BL.CR_Reportes_Informes_Instituciones_Dropdown_Obtener(CodEmpresa, filtro);
        }

        [Authorize]
        [HttpGet("CR_Reportes_Informes_Deductoras_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Deductoras_Dropdown_Obtener(int CodEmpresa,int codInstitucion,string? filtro)
        {
            return BL.CR_Reportes_Informes_Deductoras_Dropdown_Obtener(CodEmpresa, codInstitucion, filtro);
        }

        [Authorize]
        [HttpGet("CR_Reportes_Informes_Especial_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Especial_Dropdown_Obtener()
        {
            return FrmCRReportesBL.CR_Reportes_Informes_Especial_Dropdown_Obtener();
        }

        [Authorize]
        [HttpGet("CR_Reportes_Informes_Cobro_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Cobro_Dropdown_Obtener()
        {
            return FrmCRReportesBL.CR_Reportes_Informes_Cobro_Dropdown_Obtener();
        }

        [Authorize]
        [HttpGet("CR_Reportes_Informes_Proceso_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Proceso_Dropdown_Obtener()
        {
            return FrmCRReportesBL.CR_Reportes_Informes_Proceso_Dropdown_Obtener();
        }

        [Authorize]
        [HttpGet("CR_Reportes_Informes_TipoOperacion_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_TipoOperacion_Dropdown_Obtener()
        {
            return FrmCRReportesBL.CR_Reportes_Informes_TipoOperacion_Dropdown_Obtener();
        }

        [Authorize]
        [HttpGet("CR_Reportes_Informes_TiposTasas_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_TiposTasas_Dropdown_Obtener()
        {
            return FrmCRReportesBL.CR_Reportes_Informes_TiposTasas_Dropdown_Obtener();
        }

        [Authorize]
        [HttpGet("CR_Reportes_Informes_Autorizaciones_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Autorizaciones_Dropdown_Obtener()
        {
            return FrmCRReportesBL.CR_Reportes_Informes_Autorizaciones_Dropdown_Obtener();
        }

        [Authorize]
        [HttpGet("CR_Reportes_Informes_Signos_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Signos_Dropdown_Obtener()
        {
            return FrmCRReportesBL.CR_Reportes_Informes_Signos_Dropdown_Obtener();
        }

        [Authorize]
        [HttpGet("CR_Reportes_Informes_Gestiona_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Gestiona_Dropdown_Obtener(int CodEmpresa, string? filtro)
        {
            return BL.CR_Reportes_Informes_Gestiona_Dropdown_Obtener(CodEmpresa, filtro);
        }

        [Authorize]
        [HttpGet("CR_Reportes_Informes_Provincias_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Provincias_Dropdown_Obtener(int CodEmpresa, string? filtro)
        {
            return BL.CR_Reportes_Informes_Provincias_Dropdown_Obtener(CodEmpresa, filtro);
        }

        [Authorize]
        [HttpGet("CR_Reportes_Informes_Cantones_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Cantones_Dropdown_Obtener(int CodEmpresa,string provincia,string? filtro)
        {
            return BL.CR_Reportes_Informes_Cantones_Dropdown_Obtener(CodEmpresa, provincia, filtro);
        }

        [Authorize]
        [HttpGet("CR_Reportes_Informes_Distritos_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Distritos_Dropdown_Obtener(int CodEmpresa, string provincia,string canton,string? filtro)
        {
            return BL.CR_Reportes_Informes_Distritos_Dropdown_Obtener(CodEmpresa, provincia, canton, filtro);
        }

        [Authorize]
        [HttpGet("CR_Reportes_Informes_Zonas_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Zonas_Dropdown_Obtener(int CodEmpresa, string? filtro)
        {
            return BL.CR_Reportes_Informes_Zonas_Dropdown_Obtener(CodEmpresa, filtro);
        }

        [Authorize]
        [HttpGet("CR_Reportes_Informes_Profesiones_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Profesiones_Dropdown_Obtener(int CodEmpresa, string? filtro)
        {
            return BL.CR_Reportes_Informes_Profesiones_Dropdown_Obtener(CodEmpresa, filtro);
        }

        [Authorize]
        [HttpGet("CR_Reportes_Informes_Sectores_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Sectores_Dropdown_Obtener(int CodEmpresa, string? filtro)
        {
            return BL.CR_Reportes_Informes_Sectores_Dropdown_Obtener(CodEmpresa, filtro);
        }

        [Authorize]
        [HttpGet("CR_Reportes_Informes_Sexo_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Sexo_Dropdown_Obtener()
        {
            return FrmCRReportesBL.CR_Reportes_Informes_Sexo_Dropdown_Obtener();
        }

        [Authorize]
        [HttpGet("CR_Reportes_Informes_EstadoCivil_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_EstadoCivil_Dropdown_Obtener(int CodEmpresa, string? filtro)
        {
            return BL.CR_Reportes_Informes_EstadoCivil_Dropdown_Obtener(CodEmpresa, filtro);
        }

        [Authorize]
        [HttpGet("CR_Reportes_Informes_CondicionLaboral_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_CondicionLaboral_Dropdown_Obtener(int CodEmpresa, string? filtro)
        {
            return BL.CR_Reportes_Informes_CondicionLaboral_Dropdown_Obtener(CodEmpresa, filtro);
        }

        [Authorize]
        [HttpGet("CR_Reportes_Informes_Ejecutivos_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Ejecutivos_Dropdown_Obtener(int CodEmpresa, string? filtro)
        {
            return BL.CR_Reportes_Informes_Ejecutivos_Dropdown_Obtener(CodEmpresa, filtro);
        }

        [Authorize]
        [HttpGet("CR_Reportes_Informes_Arbol_Obtener")]
        public ErrorDto<List<CrReportesInformesArbolDto>> CR_Reportes_Informes_Arbol_Obtener(int CodEmpresa)
        {
            return BL.CR_Reportes_Informes_Arbol_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CR_Reportes_Informes_Reporte_Obtener")]
        public ErrorDto<CrReportesInformesArbolDto> CR_Reportes_Informes_Reporte_Obtener(int CodEmpresa, int idReporte, string usuario)
        {
            return BL.CR_Reportes_Informes_Reporte_Obtener(CodEmpresa, idReporte, usuario);
        }
        [Authorize]
        [HttpGet("CR_Reportes_Informes_Lineas_Buscar")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Lineas_Buscar( int CodEmpresa,string? filtro)
        {
            return BL.CR_Reportes_Informes_Lineas_Buscar(CodEmpresa, filtro);
        }

        [Authorize]
        [HttpGet("CR_Reportes_Informes_UnidadProgramatica_Buscar")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_UnidadProgramatica_Buscar(int CodEmpresa,int codInstitucion,string? filtro)
        {
            return BL.CR_Reportes_Informes_UnidadProgramatica_Buscar(CodEmpresa, codInstitucion, filtro);
        }

        [Authorize]
        [HttpGet("CR_Reportes_Informes_UnidadTrabajo_Buscar")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_UnidadTrabajo_Buscar(int CodEmpresa,int codInstitucion,string? codDepartamento, string? filtro)
        {
            return BL.CR_Reportes_Informes_UnidadTrabajo_Buscar(CodEmpresa, codInstitucion, codDepartamento, filtro);
        }
        [Authorize]
        [HttpGet("CR_Reportes_Informes_CausasTipos_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_CausasTipos_Dropdown_Obtener()
        {
            return FrmCRReportesBL.CR_Reportes_Informes_CausasTipos_Dropdown_Obtener();
        }

        [Authorize]
        [HttpGet("CR_Reportes_Informes_RequisitoMarca_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_RequisitoMarca_Dropdown_Obtener()
        {
            return FrmCRReportesBL.CR_Reportes_Informes_RequisitoMarca_Dropdown_Obtener();
        }

        [Authorize]
        [HttpGet("CR_Reportes_Informes_Cortes_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Cortes_Dropdown_Obtener()
        {
            return FrmCRReportesBL.CR_Reportes_Informes_Cortes_Dropdown_Obtener();
        }
        [Authorize]
        [HttpGet("CR_Reportes_Informes_Usuarios_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Usuarios_Dropdown_Obtener( int CodEmpresa,string? filtro)
        {
            return BL.CR_Reportes_Informes_Usuarios_Dropdown_Obtener(CodEmpresa, filtro);
        }
        [Authorize]
        [HttpGet("CR_Reportes_Informes_Requisitos_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Requisitos_Dropdown_Obtener(int CodEmpresa,string? codigo, bool todasLineas,string? filtro)
        {
            return BL.CR_Reportes_Informes_Requisitos_Dropdown_Obtener(
                CodEmpresa,
                codigo,
                todasLineas,
                filtro
            );
        }

        [Authorize]
        [HttpGet("CR_Reportes_Informes_Causas_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Causas_Dropdown_Obtener(int CodEmpresa,string tipo,string? filtro)
        {
            return BL.CR_Reportes_Informes_Causas_Dropdown_Obtener(
                CodEmpresa,
                tipo,
                filtro
            );
        }
        #endregion
        [Authorize]
        [HttpPost("CR_Reportes_Informes_Generar")]
        public ErrorDto<CrReportesInformesGenerarResult> CR_Reportes_Informes_Generar(int CodEmpresa,[FromBody] CrReportesInformesGenerarRequest request)
        {
            return BL.CR_Reportes_Informes_Generar(CodEmpresa, request);
        }
    }
}