using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCRReportesConfiguracionDB
    {
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly PortalDB _portalDB;
        private readonly int vModulo = 3;
        private const string CLAVE_ADMINISTRADOR = "trick";
        private const string UNOMBRE = "U.nombre";
        public FrmCRReportesConfiguracionDB(IConfiguration config)
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

        private static bool UsarPaginacion(FiltrosLazyLoadData filtros)
        {
            return (filtros?.paginacion ?? 0) > 0;
        }

        private static string AddPagination(string sql, FiltrosLazyLoadData filtros)
        {
            if (!UsarPaginacion(filtros))
                return sql + ";";

            return sql + @"
            OFFSET @offset ROWS
            FETCH NEXT @fetch ROWS ONLY;";
        }

        private static string ResolveSortOrder(FiltrosLazyLoadData filtros)
        {
            return filtros?.sortOrder == 0 ? "DESC" : "ASC";
        }

        private static string ResolveSortConfigGrupos(FiltrosLazyLoadData filtros)
        {
            return (filtros?.sortField ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "cod_grupo" => "cod_grupo",
                "descripcion" => "descripcion",
                _ => "cod_grupo"
            };
        }

        private static string ResolveSortConfigMiembros(FiltrosLazyLoadData filtros)
        {
            return (filtros?.sortField ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "usuario" => UNOMBRE,
                "descripcion" => "U.descripcion",
                "asignado" => UNOMBRE,
                _ => UNOMBRE
            };
        }

        private static string ResolveSortConfigReportes(FiltrosLazyLoadData filtros)
        {
            return (filtros?.sortField ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "id" => "R.id",
                "tipo" => "R.tipo",
                "reporte" => "R.reporte",
                "prefijo" => "R.prefijo",
                "adicional" => "R.adicional",
                "seguridad" => "R.seguridad",
                _ => "R.reporte"
            };
        }

        private static string ResolveMiembrosOrderFallback(string sortField)
        {
            return sortField.Equals("U.nombre", StringComparison.OrdinalIgnoreCase)
                ? string.Empty
                : ", U.nombre ASC";
        }

        private static string ResolveReportesOrderFallback(string sortField)
        {
            var normalized = sortField.Trim().ToUpperInvariant();

            return normalized switch
            {
                "R.ID" => ", R.reporte ASC",
                "R.TIPO" => ", R.reporte ASC, R.id ASC",
                "R.REPORTE" => ", R.id ASC",
                _ => ", R.reporte ASC, R.id ASC"
            };
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

        #endregion

        #region Configuración - Grupos

        /// <summary>
        /// Obtiene la lista de grupos de trabajo para configuración de reportes.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<CrReportesConfigGruposLista> CR_Reportes_Config_Grupos_Lista_Obtener(
            int CodEmpresa,
            FiltrosLazyLoadData filtros)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                int total;
                var lista = QueryConfigGrupos(conn, filtros, out total);

                return DbHelper.CreateOkResponse(new CrReportesConfigGruposLista
                {
                    total = total,
                    lista = lista
                });
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrReportesConfigGruposLista>(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse<CrReportesConfigGruposLista>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene la lista de grupos de trabajo sin paginación para exportar.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<CrReportesConfigGrupoData>> CR_Reportes_Config_Grupos_Lista_Export(
            int CodEmpresa,
            FiltrosLazyLoadData filtros)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                int total;
                var lista = QueryConfigGrupos(conn, filtros, out total);
                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<CrReportesConfigGrupoData>>(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse<List<CrReportesConfigGrupoData>>(ex.Message);
            }
        }

        /// <summary>
        /// Inserta o actualiza un grupo de trabajo.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="grupo"></param>
        /// <returns></returns>
        public ErrorDto CR_Reportes_Config_Grupos_Guardar(
            int CodEmpresa,
            string usuario,
            CrReportesConfigGrupoData grupo)
        {
            if (grupo == null)
                return DbHelper.ErrorResponse("El grupo es requerido.", -2);

            if (string.IsNullOrWhiteSpace(grupo.cod_grupo))
                return DbHelper.ErrorResponse("El código del grupo es requerido.", -2);

            if (string.IsNullOrWhiteSpace(grupo.descripcion))
                return DbHelper.ErrorResponse("La descripción del grupo es requerida.", -2);

            return grupo.isNew == true
                ? InsertarConfigGrupo(CodEmpresa, usuario, grupo)
                : ActualizarConfigGrupo(CodEmpresa, usuario, grupo);
        }

        /// <summary>
        /// Obtiene los grupos de trabajo para dropdown.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Config_Grupos_Dropdown_Obtener(int CodEmpresa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                    SELECT 
                        RTRIM(cod_grupo) AS item,
                        RTRIM(descripcion) AS descripcion
                    FROM CRD_GRUPOS
                    ORDER BY cod_grupo;";

                var result = conn.Query<DropDownListaGenericaModel>(sql).ToList();
                return DbHelper.CreateOkResponse(result);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(ex.Message);
            }
        }

        private static List<CrReportesConfigGrupoData> QueryConfigGrupos(
            SqlConnection conn,
            FiltrosLazyLoadData filtros,
            out int total)
        {
            var (filtro, like) = BuildFiltroLike(filtros);
            var sortField = ResolveSortConfigGrupos(filtros);
            var sortOrder = ResolveSortOrder(filtros);

            const string sqlCount = @"
                SELECT COUNT(1)
                FROM CRD_GRUPOS
                WHERE @filtro IS NULL
                   OR cod_grupo LIKE @like
                   OR descripcion LIKE @like;";

            total = conn.QuerySingle<int>(sqlCount, new { filtro, like });

            var sqlList = $@"
                SELECT
                    RTRIM(cod_grupo) AS cod_grupo,
                    RTRIM(descripcion) AS descripcion,
                    CAST(0 AS bit) AS isNew
                FROM CRD_GRUPOS
                WHERE @filtro IS NULL
                   OR cod_grupo LIKE @like
                   OR descripcion LIKE @like
                ORDER BY {sortField} {sortOrder}";

            sqlList = AddPagination(sqlList, filtros);

            return conn.Query<CrReportesConfigGrupoData>(sqlList, new
            {
                filtro,
                like,
                offset = filtros?.pagina ?? 0,
                fetch = filtros?.paginacion ?? 0
            }).ToList();
        }

        private ErrorDto InsertarConfigGrupo(int CodEmpresa, string usuario, CrReportesConfigGrupoData grupo)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                if (ExisteConfigGrupo(conn, grupo.cod_grupo))
                    return DbHelper.ErrorResponse("Ya existe un grupo con ese código.", -2);

                const string sql = @"
                    INSERT INTO CRD_GRUPOS(cod_grupo, descripcion)
                    VALUES(@cod_grupo, @descripcion);";

                conn.Execute(sql, new
                {
                    cod_grupo = grupo.cod_grupo.Trim(),
                    descripcion = grupo.descripcion.Trim()
                });

                LogBitacora(
                    CodEmpresa,
                    usuario,
                    $"Reportes > Configuración Grupo: {grupo.cod_grupo}",
                    "Registra - WEB");

                return DbHelper.OkResponse("Grupo registrado correctamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private ErrorDto ActualizarConfigGrupo(int CodEmpresa, string usuario, CrReportesConfigGrupoData grupo)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                    UPDATE CRD_GRUPOS
                    SET descripcion = @descripcion
                    WHERE cod_grupo = @cod_grupo;";

                conn.Execute(sql, new
                {
                    cod_grupo = grupo.cod_grupo.Trim(),
                    descripcion = grupo.descripcion.Trim()
                });

                LogBitacora(
                    CodEmpresa,
                    usuario,
                    $"Reportes > Configuración Grupo: {grupo.cod_grupo}",
                    "Modifica - WEB");

                return DbHelper.OkResponse("Grupo actualizado correctamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private static bool ExisteConfigGrupo(SqlConnection conn, string codGrupo)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM CRD_GRUPOS
                WHERE cod_grupo = @codGrupo;";

            return conn.QuerySingle<int>(sql, new { codGrupo = codGrupo.Trim() }) > 0;
        }

        #endregion

        #region Configuración - Miembros

        /// <summary>
        /// Obtiene los usuarios y marca si pertenecen al grupo de trabajo seleccionado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codGrupo"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<CrReportesConfigMiembrosLista> CR_Reportes_Config_Miembros_Lista_Obtener(
            int CodEmpresa,
            string codGrupo,
            FiltrosLazyLoadData filtros)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                int total;
                var lista = QueryConfigMiembros(conn, codGrupo, filtros, out total);

                return DbHelper.CreateOkResponse(new CrReportesConfigMiembrosLista
                {
                    total = total,
                    lista = lista
                });
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrReportesConfigMiembrosLista>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene los usuarios y marca si pertenecen al grupo de trabajo seleccionado para exportar.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codGrupo"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<CrReportesConfigMiembroData>> CR_Reportes_Config_Miembros_Lista_Export(
            int CodEmpresa,
            string codGrupo,
            FiltrosLazyLoadData filtros)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                int total;
                var lista = QueryConfigMiembros(conn, codGrupo, filtros, out total);
                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<CrReportesConfigMiembroData>>(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse<List<CrReportesConfigMiembroData>>(ex.Message);
            }
        }

        /// <summary>
        /// Asigna o desasigna un usuario a un grupo de trabajo.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CR_Reportes_Config_Miembros_Actualizar(
            int CodEmpresa,
            CrReportesConfigMiembroActualizarRequest request)
        {
            if (request == null)
                return DbHelper.ErrorResponse("La solicitud es requerida.", -2);

            if (string.IsNullOrWhiteSpace(request.cod_grupo))
                return DbHelper.ErrorResponse("El grupo es requerido.", -2);

            if (string.IsNullOrWhiteSpace(request.usuario))
                return DbHelper.ErrorResponse("El usuario es requerido.", -2);

            return request.asignado ==true
                ? AsignarConfigMiembro(CodEmpresa, request)
                : EliminarConfigMiembro(CodEmpresa, request);
        }

        private static List<CrReportesConfigMiembroData> QueryConfigMiembros(SqlConnection conn, string codGrupo,FiltrosLazyLoadData filtros,out int total)
        {
            filtros ??= new FiltrosLazyLoadData();

            var (filtro, like) = BuildFiltroLike(filtros);
            var sortField = ResolveSortConfigMiembros(filtros);
            var sortOrder = ResolveSortOrder(filtros);
            var orderFallback = ResolveMiembrosOrderFallback(sortField);

            var tieneSortSolicitado =
                !string.IsNullOrWhiteSpace(filtros.sortField);

            var orderBy = tieneSortSolicitado
                ? $"{sortField} {sortOrder}{orderFallback}"
                : $"""
          CASE
              WHEN G.usuario IS NULL THEN 0
              ELSE 1
          END DESC,
          {UNOMBRE} ASC
          """;

            const string sqlCount = @"
        SELECT COUNT(1)
        FROM usuarios U
        LEFT JOIN CRD_GRPUSERS G
               ON U.nombre = G.usuario
              AND G.cod_grupo = @codGrupo
        WHERE @filtro IS NULL
           OR U.nombre LIKE @like
           OR U.descripcion LIKE @like;";

            total = conn.QuerySingle<int>(
                sqlCount,
                new
                {
                    codGrupo = codGrupo.Trim(),
                    filtro,
                    like
                });

            var sqlList = $@"
        SELECT
            RTRIM(U.nombre) AS usuario,
            RTRIM(U.descripcion) AS descripcion,
            CAST(
                CASE
                    WHEN G.usuario IS NULL THEN 0
                    ELSE 1
                END
                AS bit
            ) AS asignado
        FROM usuarios U
        LEFT JOIN CRD_GRPUSERS G
               ON U.nombre = G.usuario
              AND G.cod_grupo = @codGrupo
        WHERE @filtro IS NULL
           OR U.nombre LIKE @like
           OR U.descripcion LIKE @like
        ORDER BY
            {orderBy}";

            sqlList = AddPagination(sqlList, filtros);

            return conn.Query<CrReportesConfigMiembroData>(
                sqlList,
                new
                {
                    codGrupo = codGrupo.Trim(),
                    filtro,
                    like,
                    offset = filtros.pagina,
                    fetch = filtros.paginacion
                }).ToList();
        }

        private ErrorDto AsignarConfigMiembro(int CodEmpresa, CrReportesConfigMiembroActualizarRequest request)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                    DELETE CRD_GRPUSERS
                    WHERE usuario = @usuario;

                    INSERT INTO CRD_GRPUSERS(cod_grupo, usuario)
                    VALUES(@cod_grupo, @usuario);";

                conn.Execute(sql, new
                {
                    cod_grupo = request.cod_grupo.Trim(),
                    usuario = request.usuario.Trim()
                });

                LogBitacora(
                    CodEmpresa,
                    request.usuario_sesion,
                    $"Reportes > Configuración Miembro: Grupo {request.cod_grupo}, Usuario {request.usuario}",
                    "Registra - WEB");

                return DbHelper.OkResponse("Miembro asignado correctamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private ErrorDto EliminarConfigMiembro(int CodEmpresa, CrReportesConfigMiembroActualizarRequest request)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                    DELETE CRD_GRPUSERS
                    WHERE cod_grupo = @cod_grupo
                      AND usuario = @usuario;";

                conn.Execute(sql, new
                {
                    cod_grupo = request.cod_grupo.Trim(),
                    usuario = request.usuario.Trim()
                });

                LogBitacora(
                    CodEmpresa,
                    request.usuario_sesion,
                    $"Reportes > Configuración Miembro: Grupo {request.cod_grupo}, Usuario {request.usuario}",
                    "Elimina - WEB");

                return DbHelper.OkResponse("Miembro desasignado correctamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        #endregion

        #region Configuración - Informes

        /// <summary>
        /// Obtiene la lista de informes configurados.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<CrReportesConfigReportesLista> CR_Reportes_Config_Reportes_Lista_Obtener(
            int CodEmpresa,
            FiltrosLazyLoadData filtros)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                int total;
                var lista = QueryConfigReportes(conn, filtros, out total);

                return DbHelper.CreateOkResponse(new CrReportesConfigReportesLista
                {
                    total = total,
                    lista = lista
                });
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrReportesConfigReportesLista>(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse<CrReportesConfigReportesLista>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene la lista de informes configurados sin paginación para exportar.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<CrReportesConfigReporteData>> CR_Reportes_Config_Reportes_Lista_Export(
            int CodEmpresa,
            FiltrosLazyLoadData filtros)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                int total;
                var lista = QueryConfigReportes(conn, filtros, out total);
                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<CrReportesConfigReporteData>>(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse<List<CrReportesConfigReporteData>>(ex.Message);
            }
        }

        /// <summary>
        /// Ejecuta el generador de reportes base.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CR_Reportes_Config_Reportes_Actualizar_Lista(int CodEmpresa, CrReportesConfigReportesActualizarListaRequest request)
        {
            if (request == null)
                return DbHelper.ErrorResponse("La solicitud es requerida.", -2);

            if (string.IsNullOrWhiteSpace(request.usuario_sesion))
                return DbHelper.ErrorResponse("El usuario de sesión es requerido.", -2);

            if (string.IsNullOrWhiteSpace(request.clave_edicion))
                return DbHelper.ErrorResponse("Proporcione la contraseña de Administrador.", -2);

            if (!string.Equals(request.clave_edicion.Trim(),CLAVE_ADMINISTRADOR,StringComparison.Ordinal))
                {
                    return DbHelper.ErrorResponse(
                        "La contraseña de Administrador es incorrecta.",
                        -2);
                }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                conn.Execute(
                    "spCRDReportesGen",
                    commandType: System.Data.CommandType.StoredProcedure);

                LogBitacora(
                    CodEmpresa,
                    request.usuario_sesion,
                    "Reportes > Configuración Informes: Actualizar lista",
                    "Modifica - WEB");

                return DbHelper.OkResponse(
                    "Lista de informes actualizada correctamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Inserta o actualiza la configuración de un informe.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="reporte"></param>
        /// <returns></returns>
        public ErrorDto CR_Reportes_Config_Reportes_Guardar(int CodEmpresa, CrReportesConfigReporteGuardarRequest request)
        {
            if (request == null)
                return DbHelper.ErrorResponse("La solicitud es requerida.", -2);

            if (string.IsNullOrWhiteSpace(request.usuario_sesion))
                return DbHelper.ErrorResponse("El usuario de sesión es requerido.", -2);

            if (string.IsNullOrWhiteSpace(request.clave_edicion))
                return DbHelper.ErrorResponse("Proporcione la contraseña de Administrador.", -2);

            if (!string.Equals(request.clave_edicion.Trim(),CLAVE_ADMINISTRADOR, StringComparison.Ordinal))
                {
                    return DbHelper.ErrorResponse(
                        "La contraseña de Administrador es incorrecta.",
                        -2);
                }

            var reporte = request.reporte;

            if (reporte == null)
                return DbHelper.ErrorResponse("El reporte es requerido.", -2);

            if (string.IsNullOrWhiteSpace(reporte.tipo))
                return DbHelper.ErrorResponse("El tipo es requerido.", -2);

            if (string.IsNullOrWhiteSpace(reporte.reporte))
                return DbHelper.ErrorResponse("La descripción del reporte es requerida.", -2);

            if (string.IsNullOrWhiteSpace(reporte.prefijo))
                return DbHelper.ErrorResponse("El prefijo es requerido.", -2);

            return reporte.isNew == true || reporte.id <= 0
                ? InsertarConfigReporte(CodEmpresa, request.usuario_sesion, reporte)
                : ActualizarConfigReporte(CodEmpresa, request.usuario_sesion, reporte);
        }

        private static List<CrReportesConfigReporteData> QueryConfigReportes(
            SqlConnection conn,
            FiltrosLazyLoadData filtros,
            out int total)
        {
            var (filtro, like) = BuildFiltroLike(filtros);
            var sortField = ResolveSortConfigReportes(filtros);
            var sortOrder = ResolveSortOrder(filtros);
            var orderFallback = ResolveReportesOrderFallback(sortField);

            const string sqlCount = @"
                SELECT COUNT(1)
                FROM CRD_REPORTES R
                WHERE @filtro IS NULL
                   OR CAST(R.id AS varchar(20)) LIKE @like
                   OR R.tipo LIKE @like
                   OR R.reporte LIKE @like
                   OR R.prefijo LIKE @like;";

            total = conn.QuerySingle<int>(sqlCount, new { filtro, like });

            var sqlList = $@"
                SELECT
                    R.id,
                    RTRIM(R.tipo) AS tipo,
                    RTRIM(R.reporte) AS reporte,
                    RTRIM(R.prefijo) AS prefijo,
                    ISNULL(R.adicional, 0) AS adicional,
                    ISNULL(R.seguridad, 0) AS seguridad,
                    CAST(0 AS bit) AS isNew
                FROM CRD_REPORTES R
                WHERE @filtro IS NULL
                   OR CAST(R.id AS varchar(20)) LIKE @like
                   OR R.tipo LIKE @like
                   OR R.reporte LIKE @like
                   OR R.prefijo LIKE @like
                ORDER BY {sortField} {sortOrder}{orderFallback}";

            sqlList = AddPagination(sqlList, filtros);

            return conn.Query<CrReportesConfigReporteData>(sqlList, new
            {
                filtro,
                like,
                offset = filtros?.pagina ?? 0,
                fetch = filtros?.paginacion ?? 0
            }).ToList();
        }

        private ErrorDto InsertarConfigReporte(int CodEmpresa, string usuario, CrReportesConfigReporteData reporte)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                    INSERT INTO CRD_REPORTES(tipo, reporte, prefijo, adicional, seguridad)
                    VALUES(@tipo, @reporte, @prefijo, @adicional, @seguridad);";

                conn.Execute(sql, BuildReporteParams(reporte));

                LogBitacora(
                    CodEmpresa,
                    usuario,
                    $"Reportes > Configuración Informe: {reporte.reporte}",
                    "Registra - WEB");

                return DbHelper.OkResponse("Informe registrado correctamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private ErrorDto ActualizarConfigReporte(int CodEmpresa, string usuario, CrReportesConfigReporteData reporte)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                    UPDATE CRD_REPORTES
                    SET tipo = @tipo,
                        reporte = @reporte,
                        prefijo = @prefijo,
                        adicional = @adicional,
                        seguridad = @seguridad
                    WHERE id = @id;";

                conn.Execute(sql, BuildReporteParams(reporte));

                LogBitacora(
                    CodEmpresa,
                    usuario,
                    $"Reportes > Configuración Informe: {reporte.id}",
                    "Modifica - WEB");

                return DbHelper.OkResponse("Informe actualizado correctamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private static object BuildReporteParams(CrReportesConfigReporteData reporte)
        {
            return new
            {
                reporte.id,
                tipo = reporte.tipo.Trim(),
                reporte = reporte.reporte.Trim(),
                prefijo = reporte.prefijo.Trim(),
                reporte.adicional,
                reporte.seguridad
            };
        }

        #endregion
    }
}