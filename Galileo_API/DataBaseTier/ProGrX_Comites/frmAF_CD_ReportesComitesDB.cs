using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Comites;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX_Comites
{
    public class FrmAfCdReportesComitesDb
    {
        private readonly PortalDB _portalDB;
        private readonly MProGrxMain _proGrxMain;

        public FrmAfCdReportesComitesDb(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _proGrxMain = new MProGrxMain(config);
        }

        /// <summary>
        /// Obtiene el catálogo jerárquico de reportes de la pantalla.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public static ErrorDto<List<AfCdReporteCatalogoDto>> AF_CD_ReportesComites_Catalogo_Obtener(int CodEmpresa)
        {
            return DbHelper.CreateOkResponse(BuildCatalogo());
        }

        /// <summary>
        /// Obtiene la definición funcional del reporte seleccionado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static ErrorDto<AfCdReporteDefinicionDto> AF_CD_ReportesComites_Definicion_Obtener(int CodEmpresa, string codigo)
        {
            string codigoNormalizado = NormalizarCodigo(codigo);
            AfCdReporteDefinicionDto definicion = CrearDefinicion(codigoNormalizado);

            if (string.IsNullOrWhiteSpace(definicion.codigo_opcion))
            {
                return DbHelper.CreateErrorResponse(
                    "No se encontró la definición funcional del reporte indicado.",
                    -2,
                    new AfCdReporteDefinicionDto());
            }

            return DbHelper.CreateOkResponse(definicion);
        }

        /// <summary>
        /// Obtiene los tipos de reporte disponibles para la opción seleccionada.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static ErrorDto<List<AfCdReporteTipoDto>> AF_CD_ReportesComites_TiposReporte_Obtener(int CodEmpresa, string codigo)
        {
            string codigoNormalizado = NormalizarCodigo(codigo);

            List<AfCdReporteTipoDto> tipos = RequiereResumenDetalle(codigoNormalizado)
                ? CrearTiposResumenDetalle(codigoNormalizado)
                : CrearTipoDetalle(codigoNormalizado);

            return DbHelper.CreateOkResponse(tipos);
        }

        /// <summary>
        /// Obtiene los parámetros iniciales de la pantalla.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<AfCdReportesComitesParametrosInicialesDto> AF_CD_ReportesComites_ParametrosIniciales_Obtener(int CodEmpresa)
        {
            try
            {
                DateTime fechaServidor = _proGrxMain.fxFechaServidor(CodEmpresa, 0);

                var dto = new AfCdReportesComitesParametrosInicialesDto
                {
                    fecha_inicio = fechaServidor,
                    fecha_corte = fechaServidor,
                    estados = CrearEstados(),
                    tipos_reporte = CrearTiposIniciales()
                };

                return DbHelper.CreateOkResponse(dto);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new AfCdReportesComitesParametrosInicialesDto());
            }
        }

        /// <summary>
        /// Obtiene la lista de comités para el dropdown.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_CD_Comites_Dropdown_Obtener(int CodEmpresa, string? filtro)
        {
            const string sql = @"
        select
            rtrim(COD_COMITE) as item,
            rtrim(DESCRIPCION) as descripcion
        from AFI_CD_COMITES
        where @filtro = ''
           or COD_COMITE like @like
           or DESCRIPCION like @like
        order by DESCRIPCION;";

            return EjecutarDropdown(CodEmpresa, filtro, sql);
        }

        /// <summary>
        /// Obtiene la lista de actividades para el dropdown.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_CD_Actividades_Dropdown_Obtener(int CodEmpresa, string? filtro)
        {
            const string sql = @"
        select
            rtrim(COD_ACTIVIDAD) as item,
            rtrim(DESCRIPCION) as descripcion
        from AFI_CD_ACTIVIDADES
        where ACTIVA = 1
          and (
                @filtro = ''
                or COD_ACTIVIDAD like @like
                or DESCRIPCION like @like
              )
        order by DESCRIPCION;";

            return EjecutarDropdown(CodEmpresa, filtro, sql);
        }

        /// <summary>
        /// Obtiene la lista de promotores para el dropdown.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_CD_Promotores_Dropdown_Obtener(int CodEmpresa, string? filtro)
        {
            const string sql = @"
        select
            rtrim(cast(ID_PROMOTOR as varchar(20))) as item,
            rtrim(NOMBRE) as descripcion
        from PROMOTORES
        where TIPO = 'P'
          and (
                @filtro = ''
                or cast(ID_PROMOTOR as varchar(20)) like @like
                or NOMBRE like @like
              )
        order by NOMBRE;";

            return EjecutarDropdown(CodEmpresa, filtro, sql);
        }
        private ErrorDto<List<DropDownListaGenericaModel>> EjecutarDropdown(int CodEmpresa,string? filtro,string sql)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                string filtroNormalizado = (filtro ?? string.Empty).Trim();
                string like = $"%{filtroNormalizado}%";

                List<DropDownListaGenericaModel> lista = conn
                    .Query<DropDownListaGenericaModel>(sql, new { filtro = filtroNormalizado, like })
                    .ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new List<DropDownListaGenericaModel>());
            }
        }
        private static string NormalizarCodigo(string? codigo)
        {
            return (codigo ?? string.Empty).Trim().ToUpperInvariant();
        }

        private static bool RequiereResumenDetalle(string codigo)
        {
            return codigo is "LG_CXC" or "AC_CXC" or "AUX_CT";
        }

        private static List<AfCdReporteCatalogoDto> BuildCatalogo()
        {
            return new List<AfCdReporteCatalogoDto>
            {
                new() { codigo = "ROOT", descripcion = "Reportes", codigo_padre = null, nivel = 0, orden = 0, es_hoja = false },

                new() { codigo = "CMT", descripcion = "Comites", codigo_padre = "ROOT", nivel = 1, orden = 1, es_hoja = false },
                new() { codigo = "CM_CMT", descripcion = "Comites", codigo_padre = "CMT", nivel = 2, orden = 1, es_hoja = true },
                new() { codigo = "MB_CMT", descripcion = "Miembros por Comites", codigo_padre = "CMT", nivel = 2, orden = 2, es_hoja = true },
                new() { codigo = "ACT_ASG", descripcion = "Actividades Asignadas", codigo_padre = "CMT", nivel = 2, orden = 3, es_hoja = true },
                new() { codigo = "CMT_PRM", descripcion = "Comites x Promotor", codigo_padre = "CMT", nivel = 2, orden = 4, es_hoja = true },
                new() { codigo = "HIST", descripcion = "Historial Miembros", codigo_padre = "CMT", nivel = 2, orden = 5, es_hoja = true },

                new() { codigo = "ACT", descripcion = "Actividades", codigo_padre = "ROOT", nivel = 1, orden = 2, es_hoja = false },
                new() { codigo = "ACT_LST", descripcion = "Listado de Actividades", codigo_padre = "ACT", nivel = 2, orden = 1, es_hoja = true },

                new() { codigo = "LIQ", descripcion = "Liquidaciones", codigo_padre = "ROOT", nivel = 1, orden = 3, es_hoja = false },
                new() { codigo = "LIQ_CMT", descripcion = "Liquidaciones x Comite", codigo_padre = "LIQ", nivel = 2, orden = 1, es_hoja = true },
                new() { codigo = "LIQ_EST", descripcion = "Liquidacion x Estado", codigo_padre = "LIQ", nivel = 2, orden = 2, es_hoja = true },

                new() { codigo = "CXC", descripcion = "Cuentas x Cobrar", codigo_padre = "ROOT", nivel = 1, orden = 4, es_hoja = false },
                new() { codigo = "LG_CXC", descripcion = "Listado General", codigo_padre = "CXC", nivel = 2, orden = 1, es_hoja = true },
                new() { codigo = "AC_CXC", descripcion = "Listado Actividades", codigo_padre = "CXC", nivel = 2, orden = 2, es_hoja = true },

                new() { codigo = "AUX", descripcion = "Auxiliar", codigo_padre = "ROOT", nivel = 1, orden = 5, es_hoja = false },
                new() { codigo = "AUX_CT", descripcion = "Auxiliar Contabilidad", codigo_padre = "AUX", nivel = 2, orden = 1, es_hoja = true }
            };
        }

        private static AfCdReporteDefinicionDto CrearDefinicion(string codigo)
        {
            return codigo switch
            {
                "CM_CMT" => new AfCdReporteDefinicionDto
                {
                    codigo_opcion = codigo
                },
                "MB_CMT" => new AfCdReporteDefinicionDto
                {
                    codigo_opcion = codigo,
                    requiere_estado = true,
                    requiere_fecha_inicio = true,
                    requiere_fecha_corte = true
                },
                "ACT_ASG" => new AfCdReporteDefinicionDto
                {
                    codigo_opcion = codigo
                },
                "CMT_PRM" => new AfCdReporteDefinicionDto
                {
                    codigo_opcion = codigo,
                    requiere_promotor = true,
                    usa_stored_proc = true
                },
                "HIST" => new AfCdReporteDefinicionDto
                {
                    codigo_opcion = codigo,
                    requiere_estado = true
                },
                "ACT_LST" => new AfCdReporteDefinicionDto
                {
                    codigo_opcion = codigo,
                    requiere_estado = true
                },
                "LIQ_CMT" => new AfCdReporteDefinicionDto
                {
                    codigo_opcion = codigo,
                    requiere_comite = true,
                    requiere_fecha_inicio = true,
                    requiere_fecha_corte = true,
                    requiere_tipo_reporte = true
                },
                "LIQ_EST" => new AfCdReporteDefinicionDto
                {
                    codigo_opcion = codigo,
                    requiere_estado = true,
                    requiere_fecha_inicio = true,
                    requiere_fecha_corte = true,
                    requiere_tipo_reporte = true
                },
                "LG_CXC" => new AfCdReporteDefinicionDto
                {
                    codigo_opcion = codigo,
                    requiere_fecha_corte = true,
                    requiere_tipo_reporte = true,
                    usa_stored_proc = true
                },
                "AC_CXC" => new AfCdReporteDefinicionDto
                {
                    codigo_opcion = codigo,
                    requiere_fecha_corte = true,
                    requiere_tipo_reporte = true,
                    usa_stored_proc = true
                },
                "AUX_CT" => new AfCdReporteDefinicionDto
                {
                    codigo_opcion = codigo,
                    requiere_fecha_corte = true,
                    requiere_tipo_reporte = true,
                    usa_stored_proc = true
                },
                _ => new AfCdReporteDefinicionDto()
            };
        }

        private static List<AfCdReporteTipoDto> CrearTiposResumenDetalle(string codigo)
        {
            return new List<AfCdReporteTipoDto>
            {
                new() { codigo_opcion = codigo, codigo_reporte = "R", descripcion = "Resumen" },
                new() { codigo_opcion = codigo, codigo_reporte = "D", descripcion = "Detalle" }
            };
        }

        private static List<AfCdReporteTipoDto> CrearTipoDetalle(string codigo)
        {
            return new List<AfCdReporteTipoDto>
            {
                new() { codigo_opcion = codigo, codigo_reporte = "D", descripcion = "Detalle" }
            };
        }

        private static List<DropDownListaGenericaModel> CrearEstados()
        {
            return new List<DropDownListaGenericaModel>
            {
                new() { item = "1", descripcion = "Activa" },
                new() { item = "0", descripcion = "Inactiva" }
            };
        }

        private static List<AfCdReporteTipoDto> CrearTiposIniciales()
        {
            return new List<AfCdReporteTipoDto>
            {
                new() { codigo_opcion = string.Empty, codigo_reporte = "R", descripcion = "Resumen" },
                new() { codigo_opcion = string.Empty, codigo_reporte = "D", descripcion = "Detalle" }
            };
        }
    }
}