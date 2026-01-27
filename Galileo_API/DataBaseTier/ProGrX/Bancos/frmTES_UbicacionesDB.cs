using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo.Models.Security;

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

        /// <summary>
        /// Obtiene una lista de ubicaciones de tesorería con paginación y filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<TesUbicacionesLista> Tes_UbicacionesLista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var result = new TesUbicacionesLista
                {
                    total = 0,
                    lista = new List<TesUbicacionesData>()
                };

                // Filtro opcional (LIKE seguro)
                var texto = filtros?.filtro?.Trim();
                var hasFiltro = !string.IsNullOrWhiteSpace(texto);
                var like = hasFiltro ? $"%{texto}%" : null;

                // Paginación opcional
                var offset = filtros?.pagina ?? 0;
                var fetch = filtros?.paginacion ?? 0;
                var usarPaginacion = fetch > 0;

                // ORDER BY seguro (whitelist)
                string sortField = (filtros?.sortField ?? "").Trim().ToLowerInvariant() switch
                {
                    "cod_ubicacion" => "cod_ubicacion",
                    "descripcion" => "descripcion",
                    "usuario" => "usuario",
                    "activo" => "estado",       // porque "activo" es derivado; ordenamos por estado real
                    _ => "cod_ubicacion"
                };

                string sortOrder = filtros?.sortOrder == 0 ? "DESC" : "ASC";

                // TOTAL
                const string sqlCount = @"
SELECT COUNT(1)
FROM tes_ubicaciones
WHERE
    (@filtro IS NULL)
 OR (cod_ubicacion LIKE @like)
 OR (descripcion  LIKE @like)
 OR (usuario      LIKE @like);";

                result.total = conn.QuerySingle<int>(sqlCount, new
                {
                    filtro = hasFiltro ? texto : null,
                    like
                });

                // LISTA
                var sqlList = $@"
SELECT
    cod_ubicacion,
    descripcion,
    CASE WHEN estado = 'I' THEN 0 ELSE 1 END AS activo,
    usuario
FROM tes_ubicaciones
WHERE
    (@filtro IS NULL)
 OR (cod_ubicacion LIKE @like)
 OR (descripcion  LIKE @like)
 OR (usuario      LIKE @like)
ORDER BY {sortField} {sortOrder}";

                if (usarPaginacion)
                {
                    sqlList += @"
OFFSET @offset ROWS
FETCH NEXT @fetch ROWS ONLY;";
                }

                result.lista = conn.Query<TesUbicacionesData>(sqlList, new
                {
                    filtro = hasFiltro ? texto : null,
                    like,
                    offset,
                    fetch
                }).ToList();

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
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<TesUbicacionesData>> Tes_Ubicaciones_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var texto = filtros?.filtro?.Trim();
                var hasFiltro = !string.IsNullOrWhiteSpace(texto);
                var like = hasFiltro ? $"%{texto}%" : null;

                const string sql = @"
SELECT
    cod_ubicacion,
    descripcion,
    CASE WHEN estado = 'I' THEN 0 ELSE 1 END AS activo,
    usuario
FROM tes_ubicaciones
WHERE
    (@filtro IS NULL)
 OR (cod_ubicacion LIKE @like)
 OR (descripcion  LIKE @like)
 OR (usuario      LIKE @like)
ORDER BY cod_ubicacion;";

                var result = conn.Query<TesUbicacionesData>(sql, new
                {
                    filtro = hasFiltro ? texto : null,
                    like
                }).ToList();

                return DbHelper.CreateOkResponse(result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<TesUbicacionesData>>(ex.Message);
            }
        }


        /// <summary>
        /// Inserta o actualiza una ubicación de tesorería.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="ubicacion"></param>
        /// <returns></returns>
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
        /// Actualiza una ubicación de tesorería existente.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="ubicacion"></param>
        /// <returns></returns>
        private ErrorDto Tes_Ubicaciones_Actualizar(int CodEmpresa, string usuario, TesUbicacionesData ubicacion)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                var query = $@"UPDATE tes_ubicaciones
                                    SET descripcion = @descripcion,
                                        estado = @estado,
                                        usuario = @usuario
                                    WHERE cod_ubicacion = @cod_ubicacion";
                conn.Execute(query, new
                {
                    cod_ubicacion = ubicacion.cod_ubicacion.ToUpper(),
                    descripcion = ubicacion.descripcion?.ToUpper(),
                    estado = ubicacion.activo ? "A" : "I",
                    usuario = ubicacion.usuario
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Ubicacion Doc. : {ubicacion.cod_ubicacion} - {ubicacion.descripcion}",
                    Movimiento = "Modifica - WEB",
                    Modulo = vModulo
                });

                return DbHelper.OkResponse("Ubicación actualizada correctamente.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Inserta una nueva ubicación de tesorería.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="ubicacion"></param>
        /// <returns></returns>
        private ErrorDto Tes_Ubicaciones_Insertar(int CodEmpresa, string usuario ,TesUbicacionesData ubicacion)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                var query = $@"INSERT INTO tes_ubicaciones (cod_ubicacion, descripcion, estado, usuario)
                                    VALUES (@cod_ubicacion, @descripcion, @estado, @usuario)";
                conn.Execute(query, new
                {
                    cod_ubicacion = ubicacion.cod_ubicacion.ToUpper(),
                    descripcion = ubicacion.descripcion?.ToUpper(),
                    estado = ubicacion.activo ? "A" : "I",
                    usuario = ubicacion.usuario
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Ubicacion Doc. : {ubicacion.cod_ubicacion} - {ubicacion.descripcion}",
                    Movimiento = "Registra - WEB",
                    Modulo = vModulo
                });

                return DbHelper.OkResponse("Ubicación insertada correctamente.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Elimina una ubicación de tesorería por su código.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="codUbicacion"></param>
        /// <returns></returns>
        public ErrorDto Tes_Ubicaciones_Eliminar(int CodEmpresa, string usuario, string codUbicacion)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                var query = $@"DELETE FROM tes_ubicaciones WHERE cod_ubicacion = @cod_ubicacion";
                conn.Execute(query, new { cod_ubicacion = codUbicacion.ToUpper() });
                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Ubicacion Doc. : {codUbicacion}",
                    Movimiento = "Elimina - WEB",
                    Modulo = vModulo
                });
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
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Tes_UbicacionesUsuarios_Obtener(int CodEmpresa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                var query = $@"select Nombre as 'item',descripcion from usuarios where estado = 'A' ";
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
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_ubicacion"></param>
        /// <returns></returns>
        public ErrorDto Tes_Ubicaciones_Valida(int CodEmpresa, string cod_ubicacion)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                var query = $@"SELECT count(cod_ubicacion) FROM tes_ubicaciones WHERE UPPER(cod_ubicacion) = @cod_ubicacion";
                var existe = conn.QueryFirstOrDefault<int>(query, new { cod_ubicacion = cod_ubicacion.ToUpper() });

                if (existe > 0)
                {
                    return DbHelper.ErrorResponse("El código de ubicación ya existe.");
                }
                else
                {
                    return DbHelper.OkResponse("El código de ubicación es válido.");
                }
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

    }
}
