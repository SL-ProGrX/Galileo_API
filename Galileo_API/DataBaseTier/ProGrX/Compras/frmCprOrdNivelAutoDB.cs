using Newtonsoft.Json;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier
{
    public class FrmCprOrdNivelAutoDB
    {
        private const string DefaultErrorMessage = "Error";
        private readonly PortalDB _portalDb;

        public FrmCprOrdNivelAutoDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        private sealed class UsuariosChecklistRow
        {
            // Populated by Dapper mapping
            public string nombre = string.Empty;
            public string descripcion = string.Empty;
            public DateTime? fecha = null;
            public DateTime? fecha_asignacion = null;
            public string? usuario = null;
            public int isCheck = 0;
            public int TotalRows = 0;
        }

        private static (string? Like, int Offset, int Fetch) BuildSearch(FiltroLazy filtros)
        {
            var like = string.IsNullOrWhiteSpace(filtros.filtro) ? null : $"%{filtros.filtro.Trim()}%";

            var offset = filtros.pagina.GetValueOrDefault(0);
            if (offset < 0) offset = 0;

            var fetch = filtros.paginacion.GetValueOrDefault(int.MaxValue);
            if (fetch <= 0) fetch = int.MaxValue;

            return (like, offset, fetch);
        }

        private static UsuariosAuthorizaLista ToUsuariosChecklist(List<UsuariosChecklistRow>? rows)
        {
            var list = rows ?? new List<UsuariosChecklistRow>();
            var total = list.Count == 0 ? 0 : list[0].TotalRows;

            var dto = list.Select(r => new UsuariosAutorizaData
            {
                nombre = r.nombre,
                descripcion = r.descripcion,
                fecha = r.fecha,
                fecha_asignacion = r.fecha_asignacion,
                usuario = r.usuario,
                isCheck = r.isCheck != 0
            }).ToList();

            return new UsuariosAuthorizaLista { total = total, lista = dto };
        }

        private static int CodeOrMinus1(int? code) => (code.HasValue && code.Value != 0) ? code.Value : -1;

        private static FiltroLazy ParseFiltroLazy(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<FiltroLazy>(json) ?? new FiltroLazy();
            }
            catch
            {
                return new FiltroLazy();
            }
        }


        // ----------------- Usuarios Autorizadores -----------------

        public ErrorDto<UsuariosAuthorizaLista> UsuariosAutorizadores_Obtener(int CodEmpresa, string jFiltros)
        {
            var filtros = ParseFiltroLazy(jFiltros);
            var (like, offset, fetch) = BuildSearch(filtros);

            const string sql = @"
SELECT
    U.nombre,
    U.descripcion,
    A.fecha,
    CAST(NULL AS DATETIME) AS fecha_asignacion,
    CAST(NULL AS VARCHAR(100)) AS usuario,
    CASE WHEN A.fecha IS NOT NULL THEN 1 ELSE 0 END AS isCheck,
    COUNT(*) OVER() AS TotalRows
FROM usuarios U
LEFT JOIN cpr_orden_autorizadores A ON U.nombre = A.usuario
WHERE (@F IS NULL OR U.nombre LIKE @F OR U.descripcion LIKE @F)
ORDER BY A.fecha DESC
OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY;";

            var r = DbHelper.ExecuteListQuery<UsuariosChecklistRow>(
                _portalDb,
                CodEmpresa,
                sql,
                new { F = like, Offset = offset, Fetch = fetch }
            );

            if (r.Code != 0)
                return DbHelper.CreateErrorResponse<UsuariosAuthorizaLista>(r.Description ?? DefaultErrorMessage, CodeOrMinus1(r.Code), new UsuariosAuthorizaLista { total = 0, lista = new List<UsuariosAutorizaData>() });

            return DbHelper.CreateOkResponse(ToUsuariosChecklist(r.Result));
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
                : DbHelper.ErrorResponse(r.Description ?? DefaultErrorMessage + " al guardar la orden de autorización", CodeOrMinus1(r.Code));
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
                : DbHelper.ErrorResponse(r.Description ?? DefaultErrorMessage + " al borrar la orden de autorización", CodeOrMinus1(r.Code));
        }

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
                : DbHelper.ErrorResponse(r.Description ?? "Error en guardar usuario Autorizadores", CodeOrMinus1(r.Code));
        }

        public ErrorDto OrdenAutorizadores_Eliminar(int CodEmpresa, string usuario)
        {
            const string sql = @"
BEGIN TRY
    BEGIN TRAN;

    DELETE cpr_orden_autousers WHERE usuario = @Usuario;
    DELETE cpr_orden_autorizadores WHERE usuario = @Usuario;

    DECLARE @rows INT = @@ROWCOUNT;

    COMMIT TRAN;
    SELECT @rows;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRAN;
    THROW;
END CATCH";

            var r = DbHelper.ExecuteNonQueryWithResult(
                _portalDb,
                CodEmpresa,
                sql,
                new { Usuario = usuario }
            );

            if (r.Code == 0 && r.Result > 0)
                return DbHelper.OkResponse($"Usuario Autorizador de Ordenes de Compra: {usuario}");

            return DbHelper.ErrorResponse(r.Description ?? "Error en borrar usuario Autorizador", CodeOrMinus1(r.Code));
        }

        // ----------------- Cambio de fecha autorizadores -----------------

        public ErrorDto<UsuariosAuthorizaLista> FechaCamnbioAutorizadores_Obtener(int CodEmpresa, string jFiltros)
        {
            var filtros = ParseFiltroLazy(jFiltros);
            var (like, offset, fetch) = BuildSearch(filtros);

            const string sql = @"
SELECT
    U.nombre,
    U.descripcion,
    CAST(NULL AS DATETIME) AS fecha,
    CAST(NULL AS DATETIME) AS fecha_asignacion,
    A.usuario,
    CASE WHEN A.usuario IS NOT NULL THEN 1 ELSE 0 END AS isCheck,
    COUNT(*) OVER() AS TotalRows
FROM usuarios U
LEFT JOIN cpr_INVUSRFECHAS A ON U.nombre = A.usuario
WHERE (@F IS NULL OR U.nombre LIKE @F OR U.descripcion LIKE @F)
ORDER BY A.usuario DESC
OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY;";

            var r = DbHelper.ExecuteListQuery<UsuariosChecklistRow>(
                _portalDb,
                CodEmpresa,
                sql,
                new { F = like, Offset = offset, Fetch = fetch }
            );

            if (r.Code != 0)
                return DbHelper.CreateErrorResponse<UsuariosAuthorizaLista>(r.Description ?? "Error", CodeOrMinus1(r.Code), new UsuariosAuthorizaLista { total = 0, lista = new List<UsuariosAutorizaData>() });

            return DbHelper.CreateOkResponse(ToUsuariosChecklist(r.Result));
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
                : DbHelper.ErrorResponse(r.Description ?? "Error en guardar autorización de cambio de fecha", CodeOrMinus1(r.Code));
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
                : DbHelper.ErrorResponse(r.Description ?? "Error en borrar autorización de cambio de fecha", CodeOrMinus1(r.Code));
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
            var filtros = ParseFiltroLazy(jFiltros);
            var (like, offset, fetch) = BuildSearch(filtros);

            const string sql = @"
SELECT
    U.nombre,
    U.descripcion,
    CAST(NULL AS DATETIME) AS fecha,
    C.fecha_asignacion,
    CAST(NULL AS VARCHAR(100)) AS usuario,
    CASE WHEN C.fecha_asignacion IS NOT NULL THEN 1 ELSE 0 END AS isCheck,
    COUNT(*) OVER() AS TotalRows
FROM usuarios U
LEFT JOIN cpr_orden_autousers C
       ON U.nombre = C.usuario_asignado AND C.usuario = @Usuario
WHERE (@F IS NULL OR U.nombre LIKE @F OR U.descripcion LIKE @F)
ORDER BY C.fecha_asignacion DESC
OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY;";

            var r = DbHelper.ExecuteListQuery<UsuariosChecklistRow>(
                _portalDb,
                CodEmpresa,
                sql,
                new { Usuario = usuario, F = like, Offset = offset, Fetch = fetch }
            );

            if (r.Code != 0)
                return DbHelper.CreateErrorResponse<UsuariosAuthorizaLista>(r.Description ?? "Error", CodeOrMinus1(r.Code), new UsuariosAuthorizaLista { total = 0, lista = new List<UsuariosAutorizaData>() });

            return DbHelper.CreateOkResponse(ToUsuariosChecklist(r.Result));
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
            const string existeSql = @"SELECT COUNT(*) FROM cpr_orden_rangos WHERE cod_rango = @CodRango;";
            var ex = DbHelper.ExecuteSingleQuery<int>(_portalDb, CodEmpresa, existeSql, 0, new { CodRango = request.cod_rango });
            if (ex.Code != 0)
                return DbHelper.ErrorResponse(ex.Description ?? DefaultErrorMessage, CodeOrMinus1(ex.Code));

            if (ex.Result > 0)
                return DbHelper.ErrorResponse($"Ya existe un rango con el código: {request.cod_rango}, por favor verifique", -1);

            const string insSql = @"INSERT INTO cpr_orden_rangos(cod_rango, descripcion, monto_minimo, monto_maximo, registro_fecha, registro_usuario)
VALUES (@CodRango, @Descripcion, @MontoMinimo, @MontoMaximo, GETDATE(), @RegistroUsuario);";

            var ins = DbHelper.ExecuteNonQueryWithResult(
                _portalDb,
                CodEmpresa,
                insSql + " SELECT @@ROWCOUNT;",
                new
                {
                    CodRango = request.cod_rango,
                    Descripcion = request.descripcion,
                    MontoMinimo = request.monto_minimo,
                    MontoMaximo = request.monto_maximo,
                    RegistroUsuario = request.registro_usuario
                }
            );

            return ins.Code == 0 && ins.Result > 0
                ? DbHelper.CreateOkResponse()
                : DbHelper.ErrorResponse(ins.Description ?? "No se pudo insertar el rango", CodeOrMinus1(ins.Code));
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

    }
}