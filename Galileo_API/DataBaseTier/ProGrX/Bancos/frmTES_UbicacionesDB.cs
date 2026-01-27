using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public class FrmTesUbicacionesDB
    {
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly int vModulo = 9; // Módulo de Tesorería
        private readonly PortalDB _portalDB;

        public FrmTesUbicacionesDB(IConfiguration config)
        {
            _Security_MainDB = new MSecurityMainDb(config);
            _portalDB = new PortalDB(config);
        }

        // ==========================
        // Helpers (reduce duplicación)
        // ==========================

        private static (string? filtro, string? like) BuildFiltroLike(FiltrosLazyLoadData filtros)
        {
            var texto = filtros?.filtro?.Trim();
            if (string.IsNullOrWhiteSpace(texto))
                return (null, null);

            return (texto, $"%{texto}%");
        }

        private const string SqlUbicacionesSelect = @"
SELECT
    cod_ubicacion,
    descripcion,
    CASE WHEN estado = 'I' THEN 0 ELSE 1 END AS activo,
    usuario
FROM tes_ubicaciones
";

        private const string SqlUbicacionesWhere = @"
WHERE
    (@filtro IS NULL)
 OR (cod_ubicacion LIKE @like)
 OR (descripcion  LIKE @like)
 OR (usuario      LIKE @like)
";

        private static (string sortField, string sortOrder) ResolveSort(FiltrosLazyLoadData filtros)
        {
            // ORDER BY seguro (whitelist)
            string sortField = (filtros?.sortField ?? "").Trim().ToLowerInvariant() switch
            {
                "cod_ubicacion" => "cod_ubicacion",
                "descripcion" => "descripcion",
                "usuario" => "usuario",
                "activo" => "estado", // "activo" es derivado; ordenamos por estado real
                _ => "cod_ubicacion"
            };

            string sortOrder = filtros?.sortOrder == 0 ? "DESC" : "ASC";
            return (sortField, sortOrder);
        }

        private List<TesUbicacionesData> QueryUbicaciones(
            SqlConnection conn,
            FiltrosLazyLoadData filtros,
            bool usarPaginacion,
            out int total)
        {
            var (filtro, like) = BuildFiltroLike(filtros);
            var (sortField, sortOrder) = ResolveSort(filtros);

            const string sqlCount = @"
SELECT COUNT(1)
FROM tes_ubicaciones
" + SqlUbicacionesWhere + @";";

            total = conn.QuerySingle<int>(sqlCount, new { filtro, like });

            var sqlList = SqlUbicacionesSelect
                        + SqlUbicacionesWhere
                        + $"\nORDER BY {sortField} {sortOrder}";

            int offset = filtros?.pagina ?? 0;
            int fetch = filtros?.paginacion ?? 0;

            if (usarPaginacion && fetch > 0)
            {
                sqlList += @"
OFFSET @offset ROWS
FETCH NEXT @fetch ROWS ONLY;";
            }
            else
            {
                sqlList += ";";
            }

            return conn.Query<TesUbicacionesData>(sqlList, new
            {
                filtro,
                like,
                offset,
                fetch
            }).ToList();
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

        private ErrorDto UpsertUbicacion(
            int CodEmpresa,
            string usuarioSesion,
            TesUbicacionesData ubicacion,
            bool isInsert)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                string estado = ubicacion.activo ? "A" : "I";

                if (isInsert)
                {
                    const string sql = @"
INSERT INTO tes_ubicaciones (cod_ubicacion, descripcion, estado, usuario)
VALUES (@cod_ubicacion, @descripcion, @estado, @usuario);";

                    conn.Execute(sql, new
                    {
                        cod_ubicacion = (ubicacion.cod_ubicacion ?? string.Empty).ToUpperInvariant(),
                        descripcion = ubicacion.descripcion?.ToUpperInvariant(),
                        estado,
                        usuario = ubicacion.usuario
                    });

                    LogBitacora(
                        empresaId: CodEmpresa,
                        usuario: usuarioSesion,
                        detalle: $"Ubicacion Doc. : {ubicacion.cod_ubicacion} - {ubicacion.descripcion}",
                        movimiento: "Registra - WEB");

                    return DbHelper.OkResponse("Ubicación insertada correctamente.");
                }
                else
                {
                    const string sql = @"
UPDATE tes_ubicaciones
SET descripcion = @descripcion,
    estado      = @estado,
    usuario     = @usuario
WHERE cod_ubicacion = @cod_ubicacion;";

                    conn.Execute(sql, new
                    {
                        cod_ubicacion = (ubicacion.cod_ubicacion ?? string.Empty).ToUpperInvariant(),
                        descripcion = ubicacion.descripcion?.ToUpperInvariant(),
                        estado,
                        usuario = ubicacion.usuario
                    });

                    LogBitacora(
                        empresaId: CodEmpresa,
                        usuario: usuarioSesion,
                        detalle: $"Ubicacion Doc. : {ubicacion.cod_ubicacion} - {ubicacion.descripcion}",
                        movimiento: "Modifica - WEB");

                    return DbHelper.OkResponse("Ubicación actualizada correctamente.");
                }
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        // ==========================================
        // Públicos
        // ==========================================

        /// <summary>
        /// Obtiene una lista de ubicaciones de tesorería con paginación y filtros.
        /// </summary>
        public ErrorDto<TesUbicacionesLista> Tes_UbicacionesLista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                int total;
                bool usarPaginacion = (filtros?.paginacion ?? 0) > 0;

                var lista = QueryUbicaciones(conn, filtros!, usarPaginacion, out total);

                var result = new TesUbicacionesLista
                {
                    total = total,
                    lista = lista
                };

                return DbHelper.CreateOkResponse(result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<TesUbicacionesLista>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene una lista de ubicaciones de tesorería sin paginación, con filtros aplicados.
        /// </summary>
        public ErrorDto<List<TesUbicacionesData>> Tes_Ubicaciones_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                int _; // total no se usa aquí
                var lista = QueryUbicaciones(conn, filtros, usarPaginacion: false, out _);

                return DbHelper.CreateOkResponse(lista);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<TesUbicacionesData>>(ex.Message);
            }
        }

        /// <summary>
        /// Inserta o actualiza una ubicación de tesorería.
        /// </summary>
        public ErrorDto Tes_Ubicaciones_Guardar(int CodEmpresa, string usuario, TesUbicacionesData ubicacion)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                if (ubicacion == null)
                    return DbHelper.ErrorResponse("Ubicación requerida.");

                var usuarioUbic = (ubicacion.usuario ?? string.Empty).Trim();
                var codUbic = (ubicacion.cod_ubicacion ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(usuarioUbic))
                    return DbHelper.ErrorResponse("El campo usuario de la ubicación es requerido.", -2);

                if (string.IsNullOrWhiteSpace(codUbic))
                    return DbHelper.ErrorResponse("El código de ubicación es requerido.", -2);

                // 1) Verifico si existe usuario (parametrizado, sin interpolación)
                const string sqlUsuario = @"
SELECT COUNT(1)
FROM usuarios
WHERE estado = 'A'
  AND UPPER(Nombre) = @nombre;";

                var nombreUpper = usuarioUbic.ToUpperInvariant();
                int existeUser = conn.QueryFirstOrDefault<int>(sqlUsuario, new { nombre = nombreUpper });

                if (existeUser == 0)
                    return DbHelper.ErrorResponse($"El usuario {nombreUpper} no existe o no está activo.", -2);

                // 2) Verifico si existe ubicación (parametrizado)
                const string sqlExisteUbic = @"
SELECT ISNULL(COUNT(*), 0)
FROM tes_ubicaciones
WHERE UPPER(cod_ubicacion) = @cod;";

                var codUpper = codUbic.ToUpperInvariant();
                int existeUbic = conn.QueryFirstOrDefault<int>(sqlExisteUbic, new { cod = codUpper });

                // 3) Flujo insert/update
                if (ubicacion.isNew)
                {
                    if (existeUbic > 0)
                        return DbHelper.ErrorResponse($"La Ubicación con el código {codUbic} ya existe.", -2);

                    return Tes_Ubicaciones_Insertar(CodEmpresa, usuario, ubicacion);
                }

                // no es new => update
                if (existeUbic == 0)
                    return DbHelper.ErrorResponse($"La Ubicación con el código {codUbic} no existe.", -2);

                return Tes_Ubicaciones_Actualizar(CodEmpresa, usuario, ubicacion);
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Elimina una ubicación de tesorería por su código.
        /// </summary>
        public ErrorDto Tes_Ubicaciones_Eliminar(int CodEmpresa, string usuario, string codUbicacion)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string query = @"DELETE FROM tes_ubicaciones WHERE cod_ubicacion = @cod_ubicacion";
                conn.Execute(query, new { cod_ubicacion = (codUbicacion ?? string.Empty).ToUpperInvariant() });

                LogBitacora(
                    empresaId: CodEmpresa,
                    usuario: usuario,
                    detalle: $"Ubicacion Doc. : {codUbicacion}",
                    movimiento: "Elimina - WEB");

                return DbHelper.OkResponse("Ubicación eliminada correctamente.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene una lista de usuarios activos para ubicaciones de tesorería.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Tes_UbicacionesUsuarios_Obtener(int CodEmpresa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string query = @"select Nombre as 'item', descripcion from usuarios where estado = 'A';";
                var result = conn.Query<DropDownListaGenericaModel>(query).ToList();

                return DbHelper.CreateOkResponse(result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(ex.Message);
            }
        }

        /// <summary>
        /// Valida si un código de ubicación ya existe en la base de datos.
        /// </summary>
        public ErrorDto Tes_Ubicaciones_Valida(int CodEmpresa, string cod_ubicacion)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string query = @"
SELECT COUNT(cod_ubicacion)
FROM tes_ubicaciones
WHERE UPPER(cod_ubicacion) = @cod_ubicacion;";

                var existe = conn.QueryFirstOrDefault<int>(query, new
                {
                    cod_ubicacion = (cod_ubicacion ?? string.Empty).ToUpperInvariant()
                });

                return existe > 0
                    ? DbHelper.ErrorResponse("El código de ubicación ya existe.")
                    : DbHelper.OkResponse("El código de ubicación es válido.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        // ==========================================
        // Privados (delegan al Upsert)
        // ==========================================

        private ErrorDto Tes_Ubicaciones_Actualizar(int CodEmpresa, string usuario, TesUbicacionesData ubicacion)
            => UpsertUbicacion(CodEmpresa, usuario, ubicacion, isInsert: false);

        private ErrorDto Tes_Ubicaciones_Insertar(int CodEmpresa, string usuario, TesUbicacionesData ubicacion)
            => UpsertUbicacion(CodEmpresa, usuario, ubicacion, isInsert: true);
    }
}
