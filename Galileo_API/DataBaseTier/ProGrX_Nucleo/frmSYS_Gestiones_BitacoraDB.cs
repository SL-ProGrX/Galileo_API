using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;
using System.Globalization;

namespace Galileo.DataBaseTier.ProGrX_Nucleo
{
    public class FrmSysGestionesBitacoraDB
    {
        private readonly IConfiguration _config;

        public FrmSysGestionesBitacoraDB(IConfiguration config)
        {
            _config = config;
        }


        /// <summary>
        /// Obtiene una lista de gestiones de bitacora con paginación y filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cliente_Buscar"></param>
        /// <param name="gestion_Cod"></param>
        /// <param name="usuario_Buscar"></param>
        /// <param name="fecha_Inicio"></param>
        /// <param name="fecha_Fin"></param>
        /// <param name="todasFechas"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<SysGestionesBitacorasLista> Sys_Gestiones_Bitacoras_Lista_Obtener(int CodEmpresa, SysGestionesBitacoraFiltro filtro)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<SysGestionesBitacorasLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new SysGestionesBitacorasLista { total = 0, lista = new List<SysGestionesBitacorasData>() }
            };

            try
            {
                using var connection = new SqlConnection(stringConn);

                const string fromSql = " FROM vSys_Bitacora_Operaciones v LEFT JOIN SOCIOS s ON s.CEDULA = v.CEDULA ";

                // Normalización de filtros
                var f = filtro ?? new SysGestionesBitacoraFiltro();
                var ui = f.Filtros ?? new FiltrosLazyLoadData();

                int offset = Math.Max(0, ui.pagina);
                int fetch = Math.Max(1, ui.paginacion == 0 ? 30 : ui.paginacion);

                int sortOrder = ui.sortOrder; // 0=DESC, 1=ASC
                string sortField = (ui.sortField ?? string.Empty).Trim().ToLowerInvariant();

                // Fechas
                DateTime? fechaIni = null;
                DateTime? fechaFin = null;
                if (!f.TodasFechas && !string.IsNullOrWhiteSpace(f.FechaInicio)
                    && DateTime.TryParse(f.FechaInicio, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, out var di))
                    fechaIni = di.Date;
                if (!f.TodasFechas && !string.IsNullOrWhiteSpace(f.FechaFin)
                    && DateTime.TryParse(f.FechaFin, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, out var df))
                    fechaFin = df.Date.AddDays(1).AddSeconds(-1);

                string? usuarioLike = string.IsNullOrWhiteSpace(f.UsuarioBuscar) ? null : $"%{f.UsuarioBuscar.Trim()}%";
                string? clienteLike = string.IsNullOrWhiteSpace(f.ClienteBuscar) ? null : $"%{f.ClienteBuscar.Trim()}%";

                string? gestionCod = null;
                if (!string.IsNullOrWhiteSpace(f.GestionCod) && !f.GestionCod.Equals("TODOS", StringComparison.OrdinalIgnoreCase))
                    gestionCod = f.GestionCod.Trim();

                string? generalLike = string.IsNullOrWhiteSpace(ui.filtro) ? null : $"%{ui.filtro.Trim()}%";

                const string whereSql = @"
                    WHERE (
                           @TodasFechas = 1
                           OR (@FechaIni IS NULL OR @FechaFin IS NULL)
                           OR (v.[REGISTRO_FECHA] BETWEEN @FechaIni AND @FechaFin)
                          )
                      AND (@UsuarioLike IS NULL OR v.[REGISTRO_USUARIO] LIKE @UsuarioLike)
                      AND (@GestionCod IS NULL OR v.[COD_GESTION] = @GestionCod)
                      AND (
                            @ClienteLike IS NULL
                            OR v.[CEDULA] LIKE @ClienteLike
                            OR s.[CEDULAR] LIKE @ClienteLike
                            OR s.[NOMBRE] LIKE @ClienteLike
                          )
                      AND (
                            @GeneralLike IS NULL
                            OR v.[CEDULA] LIKE @GeneralLike
                            OR s.[CEDULAR] LIKE @GeneralLike
                            OR s.[NOMBRE] LIKE @GeneralLike
                            OR v.[REGISTRO_USUARIO] LIKE @GeneralLike
                            OR v.[DESCRIPCION] LIKE @GeneralLike
                            OR v.[NOTAS] LIKE @GeneralLike
                          )";

                // Total
                var sqlCount = $@"SELECT COUNT(*) {fromSql} {whereSql}";

                // Lista paginada (orden por whitelist)
                var sqlData = $@"
                    SELECT
                        v.[CEDULA]            AS Cedula,
                        s.[NOMBRE]            AS Nombre,
                        v.[REGISTRO_FECHA]    AS Registro_Fecha,
                        v.[REGISTRO_USUARIO]  AS Registro_Usuario,
                        v.[DESCRIPCION]       AS Descripcion,
                        v.[NOTAS]             AS Notas,
                        v.[COD_GESTION]       AS Cod_Gestion
                    {fromSql}
                    {whereSql}
                    ORDER BY
                        -- ASC
                        CASE WHEN @SortOrder = 1 AND @SortField = 'identificacion' THEN v.[CEDULA] END ASC,
                        CASE WHEN @SortOrder = 1 AND @SortField = 'nombre' THEN s.[NOMBRE] END ASC,
                        CASE WHEN @SortOrder = 1 AND (@SortField = 'fecha' OR @SortField = 'registro_fecha') THEN v.[REGISTRO_FECHA] END ASC,
                        CASE WHEN @SortOrder = 1 AND (@SortField = 'usuario' OR @SortField = 'registro_usuario') THEN v.[REGISTRO_USUARIO] END ASC,
                        CASE WHEN @SortOrder = 1 AND (@SortField = 'gestion' OR @SortField = 'descripcion') THEN v.[DESCRIPCION] END ASC,
                        CASE WHEN @SortOrder = 1 AND @SortField = 'notas' THEN v.[NOTAS] END ASC,

                        -- DESC
                        CASE WHEN @SortOrder = 0 AND @SortField = 'identificacion' THEN v.[CEDULA] END DESC,
                        CASE WHEN @SortOrder = 0 AND @SortField = 'nombre' THEN s.[NOMBRE] END DESC,
                        CASE WHEN @SortOrder = 0 AND (@SortField = 'fecha' OR @SortField = 'registro_fecha') THEN v.[REGISTRO_FECHA] END DESC,
                        CASE WHEN @SortOrder = 0 AND (@SortField = 'usuario' OR @SortField = 'registro_usuario') THEN v.[REGISTRO_USUARIO] END DESC,
                        CASE WHEN @SortOrder = 0 AND (@SortField = 'gestion' OR @SortField = 'descripcion') THEN v.[DESCRIPCION] END DESC,
                        CASE WHEN @SortOrder = 0 AND @SortField = 'notas' THEN v.[NOTAS] END DESC,

                        -- Fallback
                        v.[REGISTRO_FECHA] DESC
                    OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY;";

                var p = new DynamicParameters();
                p.Add("@TodasFechas", f.TodasFechas ? 1 : 0, DbType.Int32);
                p.Add("@FechaIni", fechaIni, DbType.DateTime);
                p.Add("@FechaFin", fechaFin, DbType.DateTime);
                p.Add("@UsuarioLike", usuarioLike, DbType.String);
                p.Add("@ClienteLike", clienteLike, DbType.String);
                p.Add("@GestionCod", gestionCod, DbType.String);
                p.Add("@GeneralLike", generalLike, DbType.String);
                p.Add("@SortField", sortField, DbType.String);
                p.Add("@SortOrder", sortOrder, DbType.Int32);
                p.Add("@Offset", offset, DbType.Int32);
                p.Add("@Fetch", fetch, DbType.Int32);

                result.Result.total = connection.QueryFirstOrDefault<int>(sqlCount, p);
                result.Result.lista = connection.Query<SysGestionesBitacorasData>(sqlData, p).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.total = 0;
                result.Result.lista = null;
            }

            return result;
        }


        /// <summary>
        /// Filtros para la búsqueda de gestiones de bitácora.
        /// </summary>
        public class SysGestionesBitacoraFiltro
        {
            public string? ClienteBuscar { get; set; }
            public string? GestionCod { get; set; }
            public string? UsuarioBuscar { get; set; }
            public string? FechaInicio { get; set; }
            public string? FechaFin { get; set; }
            public bool TodasFechas { get; set; }
            public FiltrosLazyLoadData? Filtros { get; set; }
        }


        /// <summary>
        /// Obtiene una lista de gestiones de bitacora sin paginación y filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<SysGestionesBitacorasData>> Sys_Gestiones_Bitacoras_Obtener(int CodEmpresa, SysGestionesBitacoraFiltro filtro)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<SysGestionesBitacorasData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<SysGestionesBitacorasData>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);

                const string fromSql = " FROM vSys_Bitacora_Operaciones v LEFT JOIN SOCIOS s ON s.CEDULA = v.CEDULA ";

                var f = filtro ?? new SysGestionesBitacoraFiltro();

                DateTime? fechaIni = null;
                DateTime? fechaFin = null;
                if (!f.TodasFechas && !string.IsNullOrWhiteSpace(f.FechaInicio)
                    && DateTime.TryParse(f.FechaInicio, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, out var di))
                    fechaIni = di.Date;
                if (!f.TodasFechas && !string.IsNullOrWhiteSpace(f.FechaFin)
                    && DateTime.TryParse(f.FechaFin, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, out var df))
                    fechaFin = df.Date.AddDays(1).AddSeconds(-1);

                string? usuarioLike = string.IsNullOrWhiteSpace(f.UsuarioBuscar) ? null : $"%{f.UsuarioBuscar.Trim()}%";
                string? clienteLike = string.IsNullOrWhiteSpace(f.ClienteBuscar) ? null : $"%{f.ClienteBuscar.Trim()}%";

                string? gestionCod = null;
                if (!string.IsNullOrWhiteSpace(f.GestionCod) && !f.GestionCod.Equals("TODOS", StringComparison.OrdinalIgnoreCase))
                    gestionCod = f.GestionCod.Trim();

                const string whereSql = @"
                    WHERE (
                           @TodasFechas = 1
                           OR (@FechaIni IS NULL OR @FechaFin IS NULL)
                           OR (v.[REGISTRO_FECHA] BETWEEN @FechaIni AND @FechaFin)
                          )
                      AND (@UsuarioLike IS NULL OR v.[REGISTRO_USUARIO] LIKE @UsuarioLike)
                      AND (@GestionCod IS NULL OR v.[COD_GESTION] = @GestionCod)
                      AND (
                            @ClienteLike IS NULL
                            OR v.[CEDULA] LIKE @ClienteLike
                            OR s.[CEDULAR] LIKE @ClienteLike
                            OR s.[NOMBRE] LIKE @ClienteLike
                          )";

                var sql = $@"
                    SELECT
                        v.[CEDULA]            AS Cedula,
                        s.[NOMBRE]            AS Nombre,
                        v.[REGISTRO_FECHA]    AS Registro_Fecha,
                        v.[REGISTRO_USUARIO]  AS Registro_Usuario,
                        v.[DESCRIPCION]       AS Descripcion,
                        v.[NOTAS]             AS Notas,
                        v.[COD_GESTION]       AS Cod_Gestion
                    {fromSql}
                    {whereSql}
                    ORDER BY v.[REGISTRO_FECHA] DESC";

                var p = new DynamicParameters();
                p.Add("@TodasFechas", f.TodasFechas ? 1 : 0, DbType.Int32);
                p.Add("@FechaIni", fechaIni, DbType.DateTime);
                p.Add("@FechaFin", fechaFin, DbType.DateTime);
                p.Add("@UsuarioLike", usuarioLike, DbType.String);
                p.Add("@ClienteLike", clienteLike, DbType.String);
                p.Add("@GestionCod", gestionCod, DbType.String);

                result.Result = connection.Query<SysGestionesBitacorasData>(sql, p).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }

            return result;
        }


        /// <summary>
        /// Obtiene una lista de gestiones para gestiones de bitacora.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Sys_Gestiones_Tipos_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = @"
                    SELECT cod_gestion AS item,
                           RTRIM(descripcion) AS descripcion
                    FROM SYS_GESTIONES_TIPOS
                    WHERE ACTIVA = 1
                    ORDER BY descripcion";

                result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }

            return result;
        }


        /// <summary>
        /// Obtiene una lista de socios de gestiones de bitacora con paginación y filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<SociosLookupLista> Sys_Socios_Buscar_Lista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<SociosLookupLista>()
            {
                Code = 0,
                Description = "Ok",
                Result = new SociosLookupLista() { total = 0, lista = new List<SociosLookupData>() }
            };

            try
            {
                using var connection = new SqlConnection(stringConn);

                string? searchLike = string.IsNullOrWhiteSpace(filtros?.filtro) ? null : $"%{filtros.filtro.Trim()}%";

                string sortField = (filtros?.sortField ?? string.Empty).Trim().ToLowerInvariant();
                int sortOrder = filtros?.sortOrder ?? 1; // 0=DESC, 1=ASC

                int offset = Math.Max(0, filtros?.pagina ?? 0);
                int fetch = Math.Max(1, filtros?.paginacion ?? 30);

                const string sqlCount = @"SELECT COUNT(*)
                                         FROM SOCIOS
                                         WHERE (@filtro IS NULL
                                                OR CEDULA LIKE @filtro
                                                OR CEDULAR LIKE @filtro
                                                OR NOMBRE LIKE @filtro);";

                const string sqlData = @"
                    SELECT CEDULA, CEDULAR, NOMBRE
                    FROM SOCIOS
                    WHERE (@filtro IS NULL
                           OR CEDULA LIKE @filtro
                           OR CEDULAR LIKE @filtro
                           OR NOMBRE LIKE @filtro)
                    ORDER BY
                        -- ASC
                        CASE WHEN @sortOrder = 1 AND @sortField = 'cedula' THEN CEDULA END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'cedular' THEN CEDULAR END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'nombre' THEN NOMBRE END ASC,

                        -- DESC
                        CASE WHEN @sortOrder = 0 AND @sortField = 'cedula' THEN CEDULA END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'cedular' THEN CEDULAR END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'nombre' THEN NOMBRE END DESC,

                        NOMBRE ASC
                    OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";

                var p = new DynamicParameters();
                p.Add("@filtro", searchLike, DbType.String);
                p.Add("@sortField", sortField, DbType.String);
                p.Add("@sortOrder", sortOrder, DbType.Int32);
                p.Add("@offset", offset, DbType.Int32);
                p.Add("@fetch", fetch, DbType.Int32);

                result.Result.total = connection.QueryFirstOrDefault<int>(sqlCount, p);
                result.Result.lista = connection.Query<SociosLookupData>(sqlData, p).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.total = 0;
                result.Result.lista = null;
            }
            return result;
        }
    }
}