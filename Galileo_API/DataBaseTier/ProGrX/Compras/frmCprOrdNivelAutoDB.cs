using System.Data;
using Dapper;
using Newtonsoft.Json;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier
{
    public class FrmCprOrdNivelAutoDB
    {
        private readonly PortalDB _portalDb;

        public FrmCprOrdNivelAutoDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        // ----------------- Usuarios Autorizadores -----------------

        public ErrorDto<UsuariosAuthorizaLista> UsuariosAutorizadores_Obtener(int CodEmpresa, string jFiltros)
        {
            var filtros = SafeParse<FiltroLazy>(jFiltros) ?? new FiltroLazy();

            var r = DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                EnsureOpen(conn);

                var (whereSql, whereParams) = BuildUserFilterWhere(filtros.filtro);
                var (pagingSql, pagingParams) = BuildPaging(filtros.pagina, filtros.paginacion);
                var prms = MergeParams(whereParams, pagingParams);

                const string sqlTotalBase = @"
SELECT COUNT(U.nombre)
FROM usuarios U
LEFT JOIN cpr_orden_autorizadores A ON U.nombre = A.usuario
";

                var total = conn.QueryFirstOrDefault<int>(
                    sqlTotalBase + " " + whereSql,
                    prms // ✅ mismo prms que lista
                );

                const string sqlListaBase = @"
SELECT
    U.nombre,
    U.descripcion,
    A.fecha,
    CASE WHEN A.fecha IS NOT NULL THEN 1 ELSE 0 END AS isCheck
FROM usuarios U
LEFT JOIN cpr_orden_autorizadores A ON U.nombre = A.usuario
";

                var lista = conn.Query<UsuariosAutorizaData>(
                    sqlListaBase + " " + whereSql + @"
ORDER BY A.fecha DESC
" + pagingSql,
                    prms
                ).ToList();

                return new UsuariosAuthorizaLista { total = total, lista = lista };
            });

            return Map(r, () => new UsuariosAuthorizaLista { total = 0, lista = new List<UsuariosAutorizaData>() });
        }

        public ErrorDto OrdenAutousers_Insertar(int CodEmpresa, string usuario, string usuario_asignado)
        {
            var r = DbHelper.ExecuteNonQueryWithResult(
                _portalDb,
                CodEmpresa,
                @"INSERT INTO cpr_orden_autousers(usuario, usuario_asignado, fecha_Asignacion)
                  VALUES (@Usuario, @Asignado, GETDATE());",
                new { Usuario = usuario, Asignado = usuario_asignado }
            );

            return r.Code == 0 && r.Result > 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(r.Description ?? "Error al guardar la orden de autorización", GetErrorCode(r.Code));
        }

        public ErrorDto OrdenAutousers_Eliminar(int CodEmpresa, string usuario, string usuario_asignado)
        {
            var r = DbHelper.ExecuteNonQueryWithResult(
                _portalDb,
                CodEmpresa,
                @"DELETE cpr_orden_autousers
                  WHERE usuario = @Usuario AND usuario_asignado = @Asignado;",
                new { Usuario = usuario, Asignado = usuario_asignado }
            );

            return r.Code == 0 && r.Result > 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(r.Description ?? "Error al borrar la orden de autorización", GetErrorCode(r.Code));
        }

        private static int GetErrorCode(int? code) => (code.HasValue && code.Value != 0) ? code.Value : -1;

        public ErrorDto OrdenAutorizadores_Insertar(int CodEmpresa, string usuario)
        {
            var r = DbHelper.ExecuteNonQueryWithResult(
                _portalDb,
                CodEmpresa,
                @"INSERT INTO cpr_orden_autorizadores(usuario, fecha, estado)
                  VALUES (@Usuario, GETDATE(), 'A');",
                new { Usuario = usuario }
            );

            return r.Code == 0 && r.Result > 0
                ? DbHelper.OkResponse($"Usuario Autorizador de Ordenes de Compra: {usuario}")
                : DbHelper.ErrorResponse(r.Description ?? "Error en guardar usuario Autorizadores", GetErrorCode(r.Code));
        }

        public ErrorDto OrdenAutorizadores_Eliminar(int CodEmpresa, string usuario)
        {
            var r = DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                EnsureOpen(conn);
                using var tx = conn.BeginTransaction();

                try
                {
                    conn.Execute(
                        @"DELETE cpr_orden_autousers WHERE usuario = @Usuario;",
                        new { Usuario = usuario },
                        transaction: tx
                    );

                    var rows = conn.Execute(
                        @"DELETE cpr_orden_autorizadores WHERE usuario = @Usuario;",
                        new { Usuario = usuario },
                        transaction: tx
                    );

                    if (rows <= 0)
                    {
                        tx.Rollback();
                        return DbHelper.ErrorResponse("Error en borrar usuario Autorizador", -1);
                    }

                    tx.Commit();
                    return DbHelper.OkResponse($"Usuario Autorizador de Ordenes de Compra: {usuario}");
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    return DbHelper.ErrorResponse(ex.Message, -1);
                }
            });

            if (r.Code != 0 || r.Result == null)
                return DbHelper.ErrorResponse(r.Description ?? "Error", r.Code ?? -1);

            return r.Result;
        }

        // ----------------- Cambio de fecha autorizadores -----------------

        public ErrorDto<UsuariosAuthorizaLista> FechaCamnbioAutorizadores_Obtener(int CodEmpresa, string jFiltros)
        {
            var filtros = SafeParse<FiltroLazy>(jFiltros) ?? new FiltroLazy();

            var r = DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                EnsureOpen(conn);

                var (whereSql, whereParams) = BuildUserFilterWhere(filtros.filtro);
                var (pagingSql, pagingParams) = BuildPaging(filtros.pagina, filtros.paginacion);
                var prms = MergeParams(whereParams, pagingParams);

                const string sqlTotalBase = @"
SELECT COUNT(U.nombre)
FROM usuarios U
LEFT JOIN cpr_INVUSRFECHAS A ON U.nombre = A.usuario
";

                var total = conn.QueryFirstOrDefault<int>(
                    sqlTotalBase + " " + whereSql,
                    prms
                );

                const string sqlListaBase = @"
SELECT
    U.nombre,
    U.descripcion,
    A.usuario,
    CASE WHEN A.usuario IS NOT NULL THEN 1 ELSE 0 END AS isCheck
FROM usuarios U
LEFT JOIN cpr_INVUSRFECHAS A ON U.nombre = A.usuario
";

                var lista = conn.Query<UsuariosAutorizaData>(
                    sqlListaBase + " " + whereSql + @"
ORDER BY A.usuario DESC
" + pagingSql,
                    prms
                ).ToList();

                return new UsuariosAuthorizaLista { total = total, lista = lista };
            });

            return Map(r, () => new UsuariosAuthorizaLista { total = 0, lista = new List<UsuariosAutorizaData>() });
        }

        public ErrorDto FechaCambioAutorizadores_Insertar(int CodEmpresa, string usuario, string registro_usuario)
        {
            var r = DbHelper.ExecuteNonQueryWithResult(
                _portalDb,
                CodEmpresa,
                @"INSERT INTO CPR_INVUSRFECHAS(usuario, registro_fecha, registro_usuario)
                  VALUES (@Usuario, GETDATE(), @RegistroUsuario);",
                new { Usuario = usuario, RegistroUsuario = registro_usuario }
            );

            return r.Code == 0 && r.Result > 0
                ? DbHelper.OkResponse($"Usuario Autorizado Cambio Fecha Compras: {usuario}")
                : DbHelper.ErrorResponse(r.Description ?? "Error en guardar autorización de cambio de fecha", GetErrorCode(r.Code));
        }

        public ErrorDto FechaCambioAutorizadores_Eliminar(int CodEmpresa, string usuario)
        {
            var r = DbHelper.ExecuteNonQueryWithResult(
                _portalDb,
                CodEmpresa,
                @"DELETE CPR_INVUSRFECHAS WHERE usuario = @Usuario;",
                new { Usuario = usuario }
            );

            return r.Code == 0 && r.Result > 0
                ? DbHelper.OkResponse($"Usuario Autorizado Cambio Fecha Compras: {usuario}")
                : DbHelper.ErrorResponse(r.Description ?? "Error en borrar autorización de cambio de fecha", GetErrorCode(r.Code));
        }

        // ----------------- Listas -----------------

        public ErrorDto<List<UsuariosAutorizaData>> ListaAutorizador_Obtener(int CodEmpresa, string filtro)
        {
            var f = (filtro == "0") ? string.Empty : (filtro ?? string.Empty);
            var like = $"%{f.Trim()}%";

            return DbHelper.ExecuteListQuery<UsuariosAutorizaData>(
                _portalDb,
                CodEmpresa,
                @"SELECT U.nombre,
                         U.nombre + ' - ' + U.descripcion AS descripcion
                  FROM usuarios U
                  INNER JOIN cpr_orden_autorizadores A ON U.nombre = A.usuario
                  WHERE U.nombre LIKE @F OR U.descripcion LIKE @F
                  ORDER BY U.nombre;",
                new { F = like }
            );
        }

        public ErrorDto<UsuariosAuthorizaLista> ListaAutousers_Obtener(int CodEmpresa, string usuario, string jFiltros)
        {
            var filtros = SafeParse<FiltroLazy>(jFiltros) ?? new FiltroLazy();

            var r = DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                EnsureOpen(conn);

                var (whereSql, whereParams) = BuildUserFilterWhere(filtros.filtro);
                var (pagingSql, pagingParams) = BuildPaging(filtros.pagina, filtros.paginacion);

                var baseParams = MergeParams(whereParams, new { Usuario = usuario });
                var prms = MergeParams(whereParams, pagingParams, new { Usuario = usuario });

                var total = conn.QueryFirstOrDefault<int>(
                    @"
SELECT COUNT(U.nombre)
FROM usuarios U
LEFT JOIN cpr_orden_autousers C
       ON U.nombre = C.usuario_asignado AND C.usuario = @Usuario
" + whereSql,
                    baseParams
                );

                var lista = conn.Query<UsuariosAutorizaData>(
                    @"
SELECT
    U.nombre,
    U.descripcion,
    C.fecha_asignacion,
    CASE WHEN C.fecha_asignacion IS NOT NULL THEN 1 ELSE 0 END AS isCheck
FROM usuarios U
LEFT JOIN cpr_orden_autousers C
       ON U.nombre = C.usuario_asignado AND C.usuario = @Usuario
" + whereSql + @"
ORDER BY C.fecha_asignacion DESC
" + pagingSql,
                    prms
                ).ToList();

                return new UsuariosAuthorizaLista { total = total, lista = lista };
            });

            return Map(r, () => new UsuariosAuthorizaLista { total = 0, lista = new List<UsuariosAutorizaData>() });
        }

        // ----------------- Rangos -----------------

        public ErrorDto<List<RangosDto>> ObtenerListaRangos(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<RangosDto>(
                _portalDb,
                CodEmpresa,
                @"SELECT * FROM cpr_orden_rangos;"
            );
        }

        public ErrorDto<List<RangosUsuariosDto>> obtenerRangoUsuarios(int CodCliente, string cod_rango, string cod_uen, string? filtro)
        {
            return DbHelper.ExecuteListQuery<RangosUsuariosDto>(
                _portalDb,
                CodCliente,
                @"EXEC spCPR_RANGOS_USUARIOS @CodRango, @CodUen, @Filtro;",
                new { CodRango = cod_rango, CodUen = cod_uen, Filtro = filtro ?? string.Empty }
            );
        }

        public ErrorDto registroRangosUsuarios(int CodCliente, string Cod_Categoria, RangosUsuariosDto request)
        {
            var activo = request.activo ? 1 : 0;

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                CodCliente,
                @"EXEC spCpr_RegistroRangosUsuarios
                      @Nombre,
                      @Activo,
                      @CodRango,
                      @RegistroUsuario,
                      @CodRangoUsuario,
                      @Uen;",
                new
                {
                    Nombre = request.nombre,
                    Activo = activo,
                    CodRango = request.cod_rango,
                    RegistroUsuario = request.registro_usuario,
                    CodRangoUsuario = request.cod_rango_usuario,
                    Uen = request.uen
                }
            );
        }

        public ErrorDto Rangos_Actualizar(int CodEmpresa, RangosDto request)
        {
            return DbHelper.ExecuteNonQuery(
                _portalDb,
                CodEmpresa,
                @"UPDATE cpr_orden_rangos
                  SET descripcion = @Descripcion,
                      monto_minimo = @MontoMinimo,
                      monto_maximo = @MontoMaximo,
                      modifica_fecha = GETDATE(),
                      modifica_usuario = @ModificaUsuario
                  WHERE cod_rango = @CodRango;",
                new
                {
                    Descripcion = request.descripcion,
                    MontoMinimo = request.monto_minimo,
                    MontoMaximo = request.monto_maximo,
                    ModificaUsuario = request.modifica_usuario,
                    CodRango = request.cod_rango
                }
            );
        }

        public ErrorDto Rangos_Agregar(int CodEmpresa, RangosDto request)
        {
            var r = DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                EnsureOpen(conn);

                var existe = conn.QueryFirstOrDefault<int>(
                    @"SELECT COUNT(*)
                      FROM cpr_orden_rangos
                      WHERE cod_rango = @CodRango;",
                    new { CodRango = request.cod_rango }
                );

                if (existe > 0)
                    return DbHelper.ErrorResponse($"Ya existe un rango con el código: {request.cod_rango}, por favor verifique", -1);

                var rows = conn.Execute(
                    @"INSERT INTO cpr_orden_rangos(cod_rango, descripcion, monto_minimo, monto_maximo, registro_fecha, registro_usuario)
                      VALUES (@CodRango, @Descripcion, @MontoMinimo, @MontoMaximo, GETDATE(), @RegistroUsuario);",
                    new
                    {
                        CodRango = request.cod_rango,
                        Descripcion = request.descripcion,
                        MontoMinimo = request.monto_minimo,
                        MontoMaximo = request.monto_maximo,
                        RegistroUsuario = request.registro_usuario
                    }
                );

                return rows > 0 ? DbHelper.CreateOkResponse() : DbHelper.ErrorResponse("No se pudo insertar el rango", -1);
            });

            if (r.Code != 0 || r.Result == null)
                return DbHelper.ErrorResponse(r.Description ?? "Error", r.Code ?? -1);

            return r.Result;
        }

        public ErrorDto Rangos_Eliminar(int CodEmpresa, string id)
        {
            return DbHelper.ExecuteNonQuery(
                _portalDb,
                CodEmpresa,
                @"DELETE cpr_orden_rangos WHERE cod_rango = @CodRango;",
                new { CodRango = id }
            );
        }

        // ----------------- Helpers comunes -----------------

        private static void EnsureOpen(IDbConnection conn)
        {
            if (conn.State != ConnectionState.Open) conn.Open();
        }

        private static (string whereSql, object parameters) BuildUserFilterWhere(string? filtro)
        {
            if (string.IsNullOrWhiteSpace(filtro))
                return (string.Empty, new { });

            var like = $"%{filtro.Trim()}%";
            return ("WHERE U.nombre LIKE @F OR U.descripcion LIKE @F", new { F = like });
        }

        private static (string pagingSql, object parameters) BuildPaging(int? pagina, int? paginacion)
        {
            if (pagina is null || paginacion is null || pagina < 0 || paginacion <= 0)
                return (string.Empty, new { });

            return ("OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY", new { Offset = pagina.Value, Fetch = paginacion.Value });
        }

        private static object MergeParams(params object[] parts)
        {
            var p = new DynamicParameters();
            foreach (var part in parts) p.AddDynamicParams(part);
            return p;
        }

        private static T? SafeParse<T>(string json)
        {
            try { return JsonConvert.DeserializeObject<T>(json); }
            catch { return default; }
        }

        private static ErrorDto<T> Map<T>(ErrorDto<T> r, Func<T> emptyFactory)
        {
            if (r.Code != 0)
                return DbHelper.CreateErrorResponse<T>(r.Description ?? "Error", r.Code ?? -1, default!);

            return DbHelper.CreateOkResponse(r.Result ?? emptyFactory());
        }
    }
}