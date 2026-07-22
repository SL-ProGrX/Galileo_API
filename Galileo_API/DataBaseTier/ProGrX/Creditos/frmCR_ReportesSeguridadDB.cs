using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCRReportesSeguridadDB
    {
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly PortalDB _portalDB;
        private readonly int vModulo = 3;
        private const string UNOMBRE = "U.nombre";
        private const string RTIPO = "R.tipo";

        public FrmCRReportesSeguridadDB(IConfiguration config)
        {
            _Security_MainDB = new MSecurityMainDb(config);
            _portalDB = new PortalDB(config);
        }

        #region Helpers

        private static (string? filtro, string? like) BuildFiltroLike(FiltrosLazyLoadData filtros)
        {
            var texto = filtros?.filtro?.Trim();

            if (string.IsNullOrWhiteSpace(texto))
                return (null, null);

            return (texto, $"%{texto}%");
        }

        private static string ResolveSortGrupos(FiltrosLazyLoadData filtros)
        {
            return (filtros?.sortField ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "cod_grupo" => "cod_grupo",
                "descripcion" => "descripcion",
                "activo" => "activo",
                _ => "cod_grupo"
            };
        }

        private static string ResolveSortMiembros(FiltrosLazyLoadData filtros)
        {
            return (filtros?.sortField ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "usuario" => UNOMBRE,
                "descripcion" => "U.descripcion",
                "asignado" => UNOMBRE,
                _ => UNOMBRE
            };
        }

        private static string ResolveSortReportes(FiltrosLazyLoadData filtros)
        {
            return (filtros?.sortField ?? string.Empty)
                .Trim()
                .ToLowerInvariant() switch
            {
                "id" => "R.id",
                "tipo" => RTIPO,
                "reporte" => "R.reporte",
                "autorizado" => RTIPO,
                _ => RTIPO
            };
        }

        private static string ResolveSortOrder(FiltrosLazyLoadData filtros)
        {
            return filtros?.sortOrder == 0 ? "DESC" : "ASC";
        }

        private static bool UsarPaginacion(FiltrosLazyLoadData filtros)
        {
            return (filtros?.paginacion ?? 0) > 0;
        }

        private void LogBitacora(int empresaId, string usuario, string detalle, string movimiento)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = empresaId,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }

        private static string AddPagination(string sql, FiltrosLazyLoadData filtros)
        {
            if (!UsarPaginacion(filtros))
                return sql + ";";

            return sql + @"
            OFFSET @offset ROWS
            FETCH NEXT @fetch ROWS ONLY;";
        }

        #endregion

        #region Seguridad - Grupos

        /// <summary>
        /// Obtiene la lista de grupos de seguridad de reportes con paginación y filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<CrReportesSeguridadGruposLista> CR_Reportes_Seguridad_Grupos_Lista_Obtener(int CodEmpresa,FiltrosLazyLoadData filtros)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                int total;
                var lista = QueryGrupos(conn, filtros, out total);

                return DbHelper.CreateOkResponse(new CrReportesSeguridadGruposLista
                {
                    total = total,
                    lista = lista
                });
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<CrReportesSeguridadGruposLista>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene la lista de grupos de seguridad de reportes sin paginación para exportar.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<CrReportesSeguridadGrupoData>> CR_Reportes_Seguridad_Grupos_Lista_Export(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                int total;
                var lista = QueryGrupos(conn, filtros, out total);
                return DbHelper.CreateOkResponse(lista);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<CrReportesSeguridadGrupoData>>(ex.Message);
            }
        }

        /// <summary>
        /// Inserta o actualiza un grupo de seguridad de reportes.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="grupo"></param>
        /// <returns></returns>
        public ErrorDto CR_Reportes_Seguridad_Grupos_Guardar( int CodEmpresa,string usuario,CrReportesSeguridadGrupoData grupo)
        {
            if (grupo == null)
                return DbHelper.ErrorResponse("El grupo es requerido.", -2);

            if (string.IsNullOrWhiteSpace(grupo.descripcion))
                return DbHelper.ErrorResponse("La descripción del grupo es requerida.", -2);

            return grupo.isNew == true || grupo.cod_grupo <= 0
            ? InsertarGrupo(CodEmpresa, usuario, grupo)
            : ActualizarGrupo(CodEmpresa, usuario, grupo);
        }

        /// <summary>
        /// Obtiene los grupos activos de seguridad de reportes para dropdown.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Seguridad_Grupos_Dropdown_Obtener(int CodEmpresa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                SELECT 
                    CAST(cod_grupo AS varchar(20)) AS item,
                    RTRIM(descripcion) AS descripcion
                FROM crd_reportes_grp
                WHERE activo = 1
                ORDER BY descripcion;";

                var result = conn.Query<DropDownListaGenericaModel>(sql).ToList();
                return DbHelper.CreateOkResponse(result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(ex.Message);
            }
        }

        private static List<CrReportesSeguridadGrupoData> QueryGrupos(SqlConnection conn,FiltrosLazyLoadData filtros,out int total)
        {
            var (filtro, like) = BuildFiltroLike(filtros);
            var sortField = ResolveSortGrupos(filtros);
            var sortOrder = ResolveSortOrder(filtros);

            const string sqlCount = @"
                SELECT COUNT(1)
                FROM crd_reportes_grp
                WHERE @filtro IS NULL
                   OR CAST(cod_grupo AS varchar(20)) LIKE @like
                   OR descripcion LIKE @like;";

            total = conn.QuerySingle<int>(sqlCount, new { filtro, like });

            var sqlList = $@"
                SELECT
                    cod_grupo,
                    RTRIM(descripcion) AS descripcion,
                    CASE WHEN ISNULL(activo, 0) = 1 THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END AS activo,
                    CAST(0 AS bit) AS isNew
                FROM crd_reportes_grp
                WHERE @filtro IS NULL
                   OR CAST(cod_grupo AS varchar(20)) LIKE @like
                   OR descripcion LIKE @like
                ORDER BY {sortField} {sortOrder}";

            sqlList = AddPagination(sqlList, filtros);

            return conn.Query<CrReportesSeguridadGrupoData>(sqlList, new
            {
                filtro,
                like,
                offset = filtros?.pagina ?? 0,
                fetch = filtros?.paginacion ?? 0
            }).ToList();
        }

        private ErrorDto InsertarGrupo(int CodEmpresa, string usuario, CrReportesSeguridadGrupoData grupo)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                    INSERT INTO crd_reportes_grp(descripcion, activo)
                    VALUES(@descripcion, @activo);

                    SELECT CAST(SCOPE_IDENTITY() AS int);";

                var nuevoId = conn.QuerySingle<int>(sql, new
                {
                    descripcion = grupo.descripcion.Trim(),
                    activo = grupo.activo == true ? 1 : 0
                });

                LogBitacora(
                    CodEmpresa,
                    usuario,
                    $"Reportes > Grupo de Acceso: {nuevoId}",
                    "Registra - WEB");

                return DbHelper.OkResponse("Grupo registrado correctamente.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private ErrorDto ActualizarGrupo(int CodEmpresa, string usuario, CrReportesSeguridadGrupoData grupo)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                    UPDATE crd_reportes_grp
                    SET descripcion = @descripcion,
                        activo = @activo
                    WHERE cod_grupo = @cod_grupo;";

                conn.Execute(sql, new
                {
                    grupo.cod_grupo,
                    descripcion = grupo.descripcion.Trim(),
                    activo = grupo.activo == true ? 1 : 0
                });

                LogBitacora(
                    CodEmpresa,
                    usuario,
                    $"Reportes > Grupo de Acceso: {grupo.cod_grupo}",
                    "Modifica - WEB");

                return DbHelper.OkResponse("Grupo actualizado correctamente.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        #endregion

        #region Seguridad - Miembros

        /// <summary>
        /// Obtiene los usuarios disponibles y marca si pertenecen al grupo de seguridad seleccionado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codGrupo"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<CrReportesSeguridadMiembrosLista> CR_Reportes_Seguridad_Miembros_Lista_Obtener(int CodEmpresa, int codGrupo,FiltrosLazyLoadData filtros)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                int total;
                var lista = QueryMiembros(conn, codGrupo, filtros, out total);

                return DbHelper.CreateOkResponse(new CrReportesSeguridadMiembrosLista
                {
                    total = total,
                    lista = lista
                });
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<CrReportesSeguridadMiembrosLista>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene los usuarios disponibles y marca si pertenecen al grupo de seguridad seleccionado para exportar.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codGrupo"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<CrReportesSeguridadMiembroData>> CR_Reportes_Seguridad_Miembros_Lista_Export( int CodEmpresa, int codGrupo,FiltrosLazyLoadData filtros)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                int total;
                var lista = QueryMiembros(conn, codGrupo, filtros, out total);
                return DbHelper.CreateOkResponse(lista);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<CrReportesSeguridadMiembroData>>(ex.Message);
            }
        }

        /// <summary>
        /// Asigna o desasigna un usuario a un grupo de seguridad de reportes.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CR_Reportes_Seguridad_Miembros_Actualizar(int CodEmpresa,CrReportesSeguridadMiembroActualizarRequest request)
        {
            if (request == null)
                return DbHelper.ErrorResponse("La solicitud es requerida.", -2);

            if (request.cod_grupo <= 0)
                return DbHelper.ErrorResponse("El grupo es requerido.", -2);

            if (string.IsNullOrWhiteSpace(request.usuario))
                return DbHelper.ErrorResponse("El usuario es requerido.", -2);

            return request.asignado == true
                ? InsertarMiembro(CodEmpresa, request)
                : EliminarMiembro(CodEmpresa, request);
        }

        private static List<CrReportesSeguridadMiembroData> QueryMiembros(SqlConnection conn,int codGrupo,FiltrosLazyLoadData filtros,out int total)
        {
            filtros ??= new FiltrosLazyLoadData();

            var (filtro, like) = BuildFiltroLike(filtros);
            var sortField = ResolveSortMiembros(filtros);
            var sortOrder = ResolveSortOrder(filtros);

            var orderFallback = sortField.Equals(
                UNOMBRE,
                StringComparison.OrdinalIgnoreCase)
                    ? string.Empty
                    : $", {UNOMBRE} ASC";

            var tieneSortSolicitado =
                !string.IsNullOrWhiteSpace(filtros.sortField);

            var orderBy = tieneSortSolicitado
                ? $"{sortField} {sortOrder}{orderFallback}"
                : $"""
          CASE
              WHEN A.usuario IS NULL THEN 0
              ELSE 1
          END DESC,
          {UNOMBRE} ASC
          """;

            const string sqlCount = @"
        SELECT COUNT(1)
        FROM usuarios U
        LEFT JOIN CRD_REPORTES_GRP_USR A
               ON U.nombre = A.usuario
              AND U.estado = 'A'
              AND A.cod_grupo = @codGrupo
        WHERE @filtro IS NULL
           OR U.nombre LIKE @like
           OR U.descripcion LIKE @like;";

            total = conn.QuerySingle<int>(
                sqlCount,
                new
                {
                    codGrupo,
                    filtro,
                    like
                });

            var sqlList = $@"
        SELECT
            RTRIM(U.nombre) AS usuario,
            RTRIM(U.descripcion) AS descripcion,
            CAST(
                CASE
                    WHEN A.usuario IS NULL THEN 0
                    ELSE 1
                END
                AS bit
            ) AS asignado
        FROM usuarios U
        LEFT JOIN CRD_REPORTES_GRP_USR A
               ON U.nombre = A.usuario
              AND U.estado = 'A'
              AND A.cod_grupo = @codGrupo
        WHERE @filtro IS NULL
           OR U.nombre LIKE @like
           OR U.descripcion LIKE @like
        ORDER BY
            {orderBy}";

            sqlList = AddPagination(sqlList, filtros);

            return conn.Query<CrReportesSeguridadMiembroData>(
                sqlList,
                new
                {
                    codGrupo,
                    filtro,
                    like,
                    offset = filtros.pagina,
                    fetch = filtros.paginacion
                }).ToList();
        }

        private ErrorDto InsertarMiembro(int CodEmpresa, CrReportesSeguridadMiembroActualizarRequest request)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sqlExiste = @"
                    SELECT COUNT(1)
                    FROM CRD_REPORTES_GRP_USR
                    WHERE cod_grupo = @cod_grupo
                      AND usuario = @usuario;";

                var existe = conn.QuerySingle<int>(sqlExiste, request);

                if (existe == 0)
                {
                    const string sqlInsert = @"
                        INSERT INTO CRD_REPORTES_GRP_USR(cod_grupo, usuario)
                        VALUES(@cod_grupo, @usuario);";

                    conn.Execute(sqlInsert, request);
                }

                LogBitacora(
                    CodEmpresa,
                    request.usuario_sesion,
                    $"Reportes > Seguridad Miembro: Grupo {request.cod_grupo}, Usuario {request.usuario}",
                    "Registra - WEB");

                return DbHelper.OkResponse("Miembro asignado correctamente.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private ErrorDto EliminarMiembro(int CodEmpresa, CrReportesSeguridadMiembroActualizarRequest request)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                    DELETE CRD_REPORTES_GRP_USR
                    WHERE cod_grupo = @cod_grupo
                      AND usuario = @usuario;";

                conn.Execute(sql, request);

                LogBitacora(
                    CodEmpresa,
                    request.usuario_sesion,
                    $"Reportes > Seguridad Miembro: Grupo {request.cod_grupo}, Usuario {request.usuario}",
                    "Elimina - WEB");

                return DbHelper.OkResponse("Miembro desasignado correctamente.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        #endregion

        #region Seguridad - Informes Autorizados

        /// <summary>
        /// Obtiene los reportes disponibles y marca si están autorizados para el grupo seleccionado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codGrupo"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<CrReportesSeguridadReportesLista> CR_Reportes_Seguridad_Reportes_Lista_Obtener(int CodEmpresa,int codGrupo,FiltrosLazyLoadData filtros)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                int total;
                var lista = QueryReportes(conn, codGrupo, filtros, out total);

                return DbHelper.CreateOkResponse(new CrReportesSeguridadReportesLista
                {
                    total = total,
                    lista = lista
                });
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<CrReportesSeguridadReportesLista>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene los reportes disponibles y marca si están autorizados para el grupo seleccionado para exportar.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codGrupo"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<CrReportesSeguridadReporteData>> CR_Reportes_Seguridad_Reportes_Lista_Export(int CodEmpresa, int codGrupo, FiltrosLazyLoadData filtros)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                int total;
                var lista = QueryReportes(conn, codGrupo, filtros, out total);
                return DbHelper.CreateOkResponse(lista);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<CrReportesSeguridadReporteData>>(ex.Message);
            }
        }

        /// <summary>
        /// Autoriza o desautoriza un reporte para un grupo de seguridad.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CR_Reportes_Seguridad_Reportes_Actualizar(int CodEmpresa, CrReportesSeguridadReporteActualizarRequest request)
        {
            if (request == null)
                return DbHelper.ErrorResponse("La solicitud es requerida.", -2);

            if (request.cod_grupo <= 0)
                return DbHelper.ErrorResponse("El grupo es requerido.", -2);

            if (request.id <= 0)
                return DbHelper.ErrorResponse("El reporte es requerido.", -2);

            return request.autorizado == true
                ? InsertarReporteAutorizado(CodEmpresa, request)
                : EliminarReporteAutorizado(CodEmpresa, request);
        }

        private static List<CrReportesSeguridadReporteData> QueryReportes(SqlConnection conn,int codGrupo,FiltrosLazyLoadData filtros,out int total)
        {
            filtros ??= new FiltrosLazyLoadData();

            var (filtro, like) = BuildFiltroLike(filtros);
            var sortField = ResolveSortReportes(filtros);
            var sortOrder = ResolveSortOrder(filtros);
            var orderFallback = ResolveReportesOrderFallback(sortField);

            var tieneSortSolicitado =
                !string.IsNullOrWhiteSpace(filtros.sortField);

            var orderBy = tieneSortSolicitado
                ? $"{sortField} {sortOrder}{orderFallback}"
                : $"""
          CASE
              WHEN A.cod_grupo IS NULL THEN 0
              ELSE 1
          END DESC,
          {RTIPO} ASC,
          R.id ASC
          """;

            const string sqlCount = @"
        SELECT COUNT(1)
        FROM CRD_REPORTES R
        LEFT JOIN CRD_REPORTES_GRP_AUT A
               ON R.id = A.id
              AND A.cod_grupo = @codGrupo
        WHERE @filtro IS NULL
           OR CAST(R.id AS varchar(20)) LIKE @like
           OR R.tipo LIKE @like
           OR R.reporte LIKE @like;";

            total = conn.QuerySingle<int>(
                sqlCount,
                new
                {
                    codGrupo,
                    filtro,
                    like
                });

            var sqlList = $@"
        SELECT
            R.id,
            RTRIM(R.tipo) AS tipo,
            RTRIM(R.reporte) AS reporte,
            CAST(
                CASE
                    WHEN A.cod_grupo IS NULL THEN 0
                    ELSE 1
                END
                AS bit
            ) AS autorizado
        FROM CRD_REPORTES R
        LEFT JOIN CRD_REPORTES_GRP_AUT A
               ON R.id = A.id
              AND A.cod_grupo = @codGrupo
        WHERE @filtro IS NULL
           OR CAST(R.id AS varchar(20)) LIKE @like
           OR R.tipo LIKE @like
           OR R.reporte LIKE @like
        ORDER BY
            {orderBy}";

            sqlList = AddPagination(sqlList, filtros);

            return conn.Query<CrReportesSeguridadReporteData>(
                sqlList,
                new
                {
                    codGrupo,
                    filtro,
                    like,
                    offset = filtros.pagina,
                    fetch = filtros.paginacion
                }).ToList();
        }

        private static string ResolveReportesOrderFallback(string sortField)
        {
            var normalized = sortField.Trim().ToUpperInvariant();

            return normalized switch
            {
                "R.ID" => ", R.tipo ASC",
                "R.TIPO" => ", R.id ASC",
                _ => ", R.tipo ASC, R.id ASC"
            };
        }

        private ErrorDto InsertarReporteAutorizado(int CodEmpresa, CrReportesSeguridadReporteActualizarRequest request)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sqlExiste = @"
                    SELECT COUNT(1)
                    FROM CRD_REPORTES_GRP_AUT
                    WHERE cod_grupo = @cod_grupo
                      AND id = @id;";

                var existe = conn.QuerySingle<int>(sqlExiste, request);

                if (existe == 0)
                {
                    const string sqlInsert = @"
                        INSERT INTO CRD_REPORTES_GRP_AUT(cod_grupo, id)
                        VALUES(@cod_grupo, @id);";

                    conn.Execute(sqlInsert, request);
                }

                LogBitacora(
                    CodEmpresa,
                    request.usuario_sesion,
                    $"Reportes > Informe Autorizado: Grupo {request.cod_grupo}, Reporte {request.id}",
                    "Registra - WEB");

                return DbHelper.OkResponse("Reporte autorizado correctamente.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private ErrorDto EliminarReporteAutorizado(int CodEmpresa, CrReportesSeguridadReporteActualizarRequest request)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                    DELETE CRD_REPORTES_GRP_AUT
                    WHERE cod_grupo = @cod_grupo
                      AND id = @id;";

                conn.Execute(sql, request);

                LogBitacora(
                    CodEmpresa,
                    request.usuario_sesion,
                    $"Reportes > Informe Autorizado: Grupo {request.cod_grupo}, Reporte {request.id}",
                    "Elimina - WEB");

                return DbHelper.OkResponse("Reporte desautorizado correctamente.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        #endregion
    }
}