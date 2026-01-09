using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
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

        private static UsuariosAuthorizaLista QueryUsuariosChecklist(
            IDbConnection conn,
            FiltroLazy filtros,
            string totalSql,
            string listSql,
            object? extraParams = null)
        {
            EnsureOpen(conn);

            var like = NormalizeLike(filtros.filtro);
            var (offset, fetch) = NormalizePaging(filtros.pagina, filtros.paginacion);

            var dp = extraParams == null ? new DynamicParameters() : new DynamicParameters(extraParams);
            dp.Add("F", like);
            dp.Add("Offset", offset);
            dp.Add("Fetch", fetch);

            var total = conn.QueryFirstOrDefault<int>(totalSql, dp);
            var lista = conn.Query<UsuariosAutorizaData>(listSql, dp).ToList();

            return new UsuariosAuthorizaLista { total = total, lista = lista };
        }

        private static UsuariosAuthorizaLista EmptyUsuariosChecklist()
            => new UsuariosAuthorizaLista { total = 0, lista = new List<UsuariosAutorizaData>() };

        // ----------------- Usuarios Autorizadores -----------------

        public ErrorDto<UsuariosAuthorizaLista> UsuariosAutorizadores_Obtener(int CodEmpresa, string jFiltros)
        {
            var filtros = SafeParse<FiltroLazy>(jFiltros) ?? new FiltroLazy();

            var r = DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string sqlTotal = @"
SELECT COUNT(U.nombre)
FROM usuarios U
LEFT JOIN cpr_orden_autorizadores A ON U.nombre = A.usuario
WHERE (@F IS NULL OR U.nombre LIKE @F OR U.descripcion LIKE @F);
";

                const string sqlLista = @"
SELECT
    U.nombre,
    U.descripcion,
    A.fecha,
    CASE WHEN A.fecha IS NOT NULL THEN 1 ELSE 0 END AS isCheck
FROM usuarios U
LEFT JOIN cpr_orden_autorizadores A ON U.nombre = A.usuario
WHERE (@F IS NULL OR U.nombre LIKE @F OR U.descripcion LIKE @F)
ORDER BY A.fecha DESC
OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY;
";

                return QueryUsuariosChecklist(conn, filtros, sqlTotal, sqlLista);
            });

            return Map(r, EmptyUsuariosChecklist);
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
                const string sqlTotal = @"
SELECT COUNT(U.nombre)
FROM usuarios U
LEFT JOIN cpr_INVUSRFECHAS A ON U.nombre = A.usuario
WHERE (@F IS NULL OR U.nombre LIKE @F OR U.descripcion LIKE @F);
";

                const string sqlLista = @"
SELECT
    U.nombre,
    U.descripcion,
    A.usuario,
    CASE WHEN A.usuario IS NOT NULL THEN 1 ELSE 0 END AS isCheck
FROM usuarios U
LEFT JOIN cpr_INVUSRFECHAS A ON U.nombre = A.usuario
WHERE (@F IS NULL OR U.nombre LIKE @F OR U.descripcion LIKE @F)
ORDER BY A.usuario DESC
OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY;
";

                return QueryUsuariosChecklist(conn, filtros, sqlTotal, sqlLista);
            });

            return Map(r, EmptyUsuariosChecklist);
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
                const string sqlTotal = @"
SELECT COUNT(U.nombre)
FROM usuarios U
LEFT JOIN cpr_orden_autousers C
       ON U.nombre = C.usuario_asignado AND C.usuario = @Usuario
WHERE (@F IS NULL OR U.nombre LIKE @F OR U.descripcion LIKE @F);
";

                const string sqlLista = @"
SELECT
    U.nombre,
    U.descripcion,
    C.fecha_asignacion,
    CASE WHEN C.fecha_asignacion IS NOT NULL THEN 1 ELSE 0 END AS isCheck
FROM usuarios U
LEFT JOIN cpr_orden_autousers C
       ON U.nombre = C.usuario_asignado AND C.usuario = @Usuario
WHERE (@F IS NULL OR U.nombre LIKE @F OR U.descripcion LIKE @F)
ORDER BY C.fecha_asignacion DESC
OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY;
";

                return QueryUsuariosChecklist(conn, filtros, sqlTotal, sqlLista, new { Usuario = usuario });
            });

            return Map(r, EmptyUsuariosChecklist);
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

        private static string? NormalizeLike(string? filtro)
        {
            if (string.IsNullOrWhiteSpace(filtro))
                return null;

            var f = filtro.Trim();
            return f.Length == 0 ? null : $"%{f}%";
        }

        private static (int Offset, int Fetch) NormalizePaging(int? pagina, int? paginacion)
        {
            // We keep the existing meaning: `pagina` is used as OFFSET.
            if (pagina is null || paginacion is null || pagina < 0 || paginacion <= 0)
                return (0, int.MaxValue);

            return (pagina.Value, paginacion.Value);
        }

        private static void EnsureOpen(IDbConnection conn)
        {
            if (conn.State != ConnectionState.Open) conn.Open();
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