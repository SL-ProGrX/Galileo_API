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
        private readonly PortalDB _portalDB;

        public FrmSysGestionesBitacoraDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }


        private const string FromSql = " FROM vSys_Bitacora_Operaciones v LEFT JOIN SOCIOS s ON s.CEDULA = v.CEDULA ";

        // NOTE: Keep SQL as constants to avoid runtime string concatenation (Sonar S2077)
        private const string GestionesWhereBaseSql = @"
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

        private const string GestionesWhereWithGeneralSql = GestionesWhereBaseSql + @"
                      AND (
                            @GeneralLike IS NULL
                            OR v.[CEDULA] LIKE @GeneralLike
                            OR s.[CEDULAR] LIKE @GeneralLike
                            OR s.[NOMBRE] LIKE @GeneralLike
                            OR v.[REGISTRO_USUARIO] LIKE @GeneralLike
                            OR v.[DESCRIPCION] LIKE @GeneralLike
                            OR v.[NOTAS] LIKE @GeneralLike
                          )";

        private const string GestionesCountWithGeneralSql = "SELECT COUNT(*) " + FromSql + GestionesWhereWithGeneralSql;

        private const string GestionesSelectColumnsSql = @"
                    SELECT
                        v.[CEDULA]            AS Cedula,
                        s.[NOMBRE]            AS Nombre,
                        v.[REGISTRO_FECHA]    AS Registro_Fecha,
                        v.[REGISTRO_USUARIO]  AS Registro_Usuario,
                        v.[DESCRIPCION]       AS Descripcion,
                        v.[NOTAS]             AS Notas,
                        v.[COD_GESTION]       AS Cod_Gestion
                    ";

        private const string GestionesPagedOrderBySql = @"
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

        private const string GestionesPagedWithGeneralSql = GestionesSelectColumnsSql + FromSql + GestionesWhereWithGeneralSql + GestionesPagedOrderBySql;

        private const string GestionesAllBaseSql = GestionesSelectColumnsSql + FromSql + GestionesWhereBaseSql + "\n                    ORDER BY v.[REGISTRO_FECHA] DESC";

        private static DateTime? ParseDateOrNull(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, out var d)
                ? d
                : null;
        }

        private sealed class GestionesSpec
        {
            public DynamicParameters Params { get; init; } = new();
            public int Offset { get; init; }
            public int Fetch { get; init; }
            public string SortField { get; init; } = string.Empty;
            public int SortOrder { get; init; }
        }

        private static GestionesSpec BuildSpec(SysGestionesBitacoraFiltro? filtro, bool includeGeneral)
        {
            var f = filtro ?? new SysGestionesBitacoraFiltro();
            var ui = f.Filtros ?? new FiltrosLazyLoadData();

            var offset = Math.Max(0, ui.pagina);
            var fetch = Math.Max(1, ui.paginacion == 0 ? 30 : ui.paginacion);

            var sortOrder = ui.sortOrder; // 0=DESC, 1=ASC
            var sortField = (ui.sortField ?? string.Empty).Trim().ToLowerInvariant();

            DateTime? fechaIni = null;
            DateTime? fechaFin = null;

            if (!f.TodasFechas)
            {
                var di = ParseDateOrNull(f.FechaInicio);
                var df = ParseDateOrNull(f.FechaFin);

                if (di.HasValue) fechaIni = di.Value.Date;
                if (df.HasValue) fechaFin = df.Value.Date.AddDays(1).AddSeconds(-1);
            }

            string? usuarioLike = string.IsNullOrWhiteSpace(f.UsuarioBuscar) ? null : $"%{f.UsuarioBuscar.Trim()}%";
            string? clienteLike = string.IsNullOrWhiteSpace(f.ClienteBuscar) ? null : $"%{f.ClienteBuscar.Trim()}%";

            string? gestionCod = null;
            if (!string.IsNullOrWhiteSpace(f.GestionCod) && !f.GestionCod.Equals("TODOS", StringComparison.OrdinalIgnoreCase))
                gestionCod = f.GestionCod.Trim();

            string? generalLike = string.IsNullOrWhiteSpace(ui.filtro) ? null : $"%{ui.filtro.Trim()}%";

            var p = new DynamicParameters();
            p.Add("@TodasFechas", f.TodasFechas ? 1 : 0, DbType.Int32);
            p.Add("@FechaIni", fechaIni, DbType.DateTime);
            p.Add("@FechaFin", fechaFin, DbType.DateTime);
            p.Add("@UsuarioLike", usuarioLike, DbType.String);
            p.Add("@ClienteLike", clienteLike, DbType.String);
            p.Add("@GestionCod", gestionCod, DbType.String);

            if (includeGeneral)
                p.Add("@GeneralLike", generalLike, DbType.String);

            p.Add("@SortField", sortField, DbType.String);
            p.Add("@SortOrder", sortOrder, DbType.Int32);
            p.Add("@Offset", offset, DbType.Int32);
            p.Add("@Fetch", fetch, DbType.Int32);

            return new GestionesSpec
            {
                Params = p,
                Offset = offset,
                Fetch = fetch,
                SortField = sortField,
                SortOrder = sortOrder
            };
        }


        private ErrorDto<T> WithEmpresaConn<T>(int codEmpresa, Func<SqlConnection, T> action)
            => DbHelper.WithConn(_portalDB, codEmpresa, action);


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
            return WithEmpresaConn(CodEmpresa, connection =>
            {
                var spec = BuildSpec(filtro, includeGeneral: true);
                var sqlCount = GestionesCountWithGeneralSql;
                var sqlData = GestionesPagedWithGeneralSql;

                var total = connection.QueryFirstOrDefault<int>(sqlCount, spec.Params);
                var lista = connection.Query<SysGestionesBitacorasData>(sqlData, spec.Params).ToList();

                return new SysGestionesBitacorasLista { total = total, lista = lista };
            });
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
            return WithEmpresaConn(CodEmpresa, connection =>
            {
                var spec = BuildSpec(filtro, includeGeneral: false);
                var sql = GestionesAllBaseSql;

                return connection.Query<SysGestionesBitacorasData>(sql, spec.Params).ToList();
            });
        }


        /// <summary>
        /// Obtiene una lista de gestiones para gestiones de bitacora.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Sys_Gestiones_Tipos_Obtener(int CodEmpresa)
        {
            const string query = @"
                    SELECT cod_gestion AS item,
                           RTRIM(descripcion) AS descripcion
                    FROM SYS_GESTIONES_TIPOS
                    WHERE ACTIVA = 1
                    ORDER BY descripcion";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDB, CodEmpresa, query);
        }


        /// <summary>
        /// Obtiene una lista de socios de gestiones de bitacora con paginación y filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<SociosLookupLista> Sys_Socios_Buscar_Lista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            return WithEmpresaConn(CodEmpresa, connection =>
            {
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

                var total = connection.QueryFirstOrDefault<int>(sqlCount, p);
                var lista = connection.Query<SociosLookupData>(sqlData, p).ToList();

                return new SociosLookupLista { total = total, lista = lista };
            });
        }
    }
}