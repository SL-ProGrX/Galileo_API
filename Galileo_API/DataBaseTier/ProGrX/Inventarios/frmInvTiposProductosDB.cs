using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Newtonsoft.Json;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmInvTiposProductosDB
    {
        private readonly IConfiguration _config;
        private const string ActualizarTipoProductoSql = @"UPDATE pv_prod_clasifica
                                                            SET descripcion = @Descripcion,
                                                                costeo = @Costeo,
                                                                valuacion = @Valuacion,
                                                                cod_cuenta = @Cod_Cuenta,
                                                                cod_alter = @Cod_Alter
                                                            WHERE Cod_Prodclas = @Cod_Prodclas";
        private const string InsertarTipoProductoSql = @"INSERT INTO pv_prod_clasifica
                                                            (descripcion, costeo, valuacion, cod_cuenta, cod_alter)
                                                         VALUES
                                                            (@Descripcion, @Costeo, @Valuacion, @Cod_Cuenta, @Cod_Alter)";
        private const string EliminarTipoProductoSql = "DELETE pv_prod_clasifica WHERE cod_prodclas = @Cod_Prodclas";
        private const string ActualizarSubcategoriaSql = @"UPDATE pv_prod_clasifica_Sub
                                                           SET descripcion = @Descripcion,
                                                               activo = @Activo,
                                                               CABYS = @CABYS,
                                                               COD_CUENTA = @COD_CUENTA,
                                                               COD_LINEA_SUB_MADRE = @COD_LINEA_SUB_MADRE,
                                                               NIVEL = @NIVEL
                                                           WHERE Cod_Prodclas = @Cod_Prodclas
                                                             AND COD_LINEA_SUB = @Cod_Linea_Sub";
        private const string TipoProductoConsultaBaseSql = @"SELECT T.cod_prodclas,
                                                                    T.descripcion,
                                                                    T.costeo,
                                                                    T.valuacion,
                                                                    C.cod_cuenta_Mask AS cod_cuenta,
                                                                    T.cod_Alter,
                                                                    C.descripcion AS Cta_Desc,
                                                                    (SELECT COUNT(Cod_Prodclas)
                                                                     FROM PV_PROD_CLASIFICA_SUB
                                                                     WHERE COD_PRODCLAS = T.cod_prodclas) AS Cantidad_Sub
                                                             FROM pv_prod_clasifica T
                                                             LEFT JOIN CntX_cuentas C
                                                               ON T.cod_cuenta = C.cod_cuenta
                                                              AND C.cod_contabilidad = @cod_contabilidad
                                                             WHERE @Filtro IS NULL
                                                                OR CONVERT(VARCHAR(20), T.cod_prodclas) LIKE @Filtro
                                                                OR T.DESCRIPCION LIKE @Filtro
                                                                OR T.costeo LIKE @Filtro
                                                                OR T.valuacion LIKE @Filtro
                                                             ORDER BY T.cod_prodclas DESC";
        private const string TipoProductoConsultaPaginadaSql = TipoProductoConsultaBaseSql
            + " OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY";
        private const string TipoProductoConteoSql = @"SELECT COUNT(*)
                                                       FROM pv_prod_clasifica T
                                                       LEFT JOIN CntX_cuentas C
                                                         ON T.cod_cuenta = C.cod_cuenta
                                                        AND C.cod_contabilidad = @cod_contabilidad
                                                       WHERE @Filtro IS NULL
                                                          OR CONVERT(VARCHAR(20), T.cod_prodclas) LIKE @Filtro
                                                          OR T.DESCRIPCION LIKE @Filtro
                                                          OR T.costeo LIKE @Filtro
                                                          OR T.valuacion LIKE @Filtro";
        private const string SubcategoriaJerarquiaSql = @"WITH RecursiveHierarchy AS (
                                                              SELECT COD_LINEA_SUB,
                                                                     Cod_Prodclas,
                                                                     DESCRIPCION,
                                                                     COD_LINEA_SUB_MADRE,
                                                                     Activo,
                                                                     Cabys,
                                                                     COD_CUENTA,
                                                                     NIVEL,
                                                                     CAST(COD_LINEA_SUB AS VARCHAR(MAX)) AS Niveles
                                                              FROM PV_PROD_CLASIFICA_SUB
                                                              WHERE COD_LINEA_SUB_MADRE IS NULL
                                                                AND COD_PRODCLAS = @ProdClas

                                                              UNION ALL

                                                              SELECT p.COD_LINEA_SUB,
                                                                     p.Cod_Prodclas,
                                                                     p.DESCRIPCION,
                                                                     p.COD_LINEA_SUB_MADRE,
                                                                     p.Activo,
                                                                     p.Cabys,
                                                                     p.COD_CUENTA,
                                                                     p.NIVEL,
                                                                     CONCAT(rh.Niveles, '.', p.NIVEL) AS Niveles
                                                              FROM PV_PROD_CLASIFICA_SUB p
                                                              INNER JOIN RecursiveHierarchy rh
                                                                ON p.COD_LINEA_SUB_MADRE = rh.COD_LINEA_SUB
                                                              WHERE p.COD_PRODCLAS = @ProdClas
                                                          ) ";
        private const string SubcategoriaConteoSql = SubcategoriaJerarquiaSql
            + @"SELECT COUNT(*)
                FROM RecursiveHierarchy
                WHERE @Filtro IS NULL
                   OR Descripcion LIKE @Filtro
                   OR Niveles LIKE @Filtro";
        private const string SubcategoriaConsultaBaseSql = SubcategoriaJerarquiaSql
            + @"SELECT COD_LINEA_SUB,
                       Cod_Prodclas,
                       DESCRIPCION,
                       COD_LINEA_SUB_MADRE,
                       Activo,
                       Cabys,
                       COD_CUENTA,
                       NIVEL,
                       Niveles
                FROM RecursiveHierarchy
                WHERE @Filtro IS NULL
                   OR Descripcion LIKE @Filtro
                   OR Niveles LIKE @Filtro
                ORDER BY Niveles ASC";
        private const string SubcategoriaConsultaPaginadaSql = SubcategoriaConsultaBaseSql
            + " OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY";

        #region Constructor y helpers

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmInvTiposProductosDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmInvTiposProductosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Crea una respuesta vacía para el listado de tipos de producto.
        /// </summary>
        /// <returns>Listado vacío inicializado.</returns>
        private static TipoProductoDataLista CrearTipoProductoListaVacia() => new()
        {
            Total = 0,
            Lista = new List<TipoProductoDto>()
        };

        /// <summary>
        /// Crea una respuesta vacía para el listado de subcategorías.
        /// </summary>
        /// <returns>Listado vacío inicializado.</returns>
        private static TipoProductoSubDataLista CrearTipoProductoSubListaVacia() => new()
        {
            Total = 0,
            Lista = new List<TipoProductoSubDto>()
        };



        /// <summary>
        /// Completa el texto de estado para una subcategoría.
        /// </summary>
        /// <param name="item">Subcategoría a completar.</param>
        private static void CompletarEstadoSubcategoria(TipoProductoSubDto item)
        {
            item.Estado = item.Activo ? "ACTIVO" : "INACTIVO";
        }

        /// <summary>
        /// Obtiene el siguiente consecutivo para subcategoría.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <returns>Consecutivo generado.</returns>
        private static int ObtenerSiguienteCodLineaSub(IDbConnection connection)
        {
            return connection.QueryFirstOrDefault<int>("SELECT ISNULL(MAX(COD_LINEA_SUB),0) + 1 FROM PV_PROD_CLASIFICA_SUB");
        }

        /// <summary>
        /// Obtiene el nivel del padre seleccionado.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="codLineaSubMadre">Código de la línea madre.</param>
        /// <returns>Nivel encontrado.</returns>
        private static int? ObtenerNivelPadre(IDbConnection connection, string codLineaSubMadre)
        {
            return connection.QueryFirstOrDefault<int?>(
                "SELECT NIVEL FROM PV_PROD_CLASIFICA_SUB WHERE Cod_Linea_Sub = @Cod_Linea_Sub_Madre",
                new { Cod_Linea_Sub_Madre = codLineaSubMadre });
        }

        /// <summary>
        /// Determina si una categoría pertenece a la descendencia de otra.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="codProdclas">Código del tipo de producto.</param>
        /// <param name="codigoRaiz">Código desde el cual se recorre la jerarquía.</param>
        /// <param name="posibleDescendiente">Código que se desea validar.</param>
        /// <returns><see langword="true"/> cuando el código pertenece a la descendencia.</returns>
        private static bool EsDescendiente(
            IDbConnection connection,
            int codProdclas,
            string codigoRaiz,
            string posibleDescendiente)
        {
            return connection.QueryFirstOrDefault<int>(
                @"WITH Descendientes AS (
                      SELECT COD_LINEA_SUB
                      FROM PV_PROD_CLASIFICA_SUB
                      WHERE COD_PRODCLAS = @codProdclas
                        AND COD_LINEA_SUB_MADRE = @codigoRaiz

                      UNION ALL

                      SELECT hijo.COD_LINEA_SUB
                      FROM PV_PROD_CLASIFICA_SUB hijo
                      INNER JOIN Descendientes padre
                        ON hijo.COD_LINEA_SUB_MADRE = padre.COD_LINEA_SUB
                      WHERE hijo.COD_PRODCLAS = @codProdclas
                  )
                  SELECT COUNT(*)
                  FROM Descendientes
                  WHERE COD_LINEA_SUB = @posibleDescendiente",
                new { codProdclas, codigoRaiz, posibleDescendiente }) > 0;
        }

        /// <summary>
        /// Obtiene la lista de subcategorías hijas de un padre.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="padre">Padre de la jerarquía.</param>
        /// <returns>Listado de hijos.</returns>
        private List<TipoProductoSubGradaData> TipoProductoSub_SeguienteNivel(IDbConnection connection, TipoProductoSubDto padre)
        {
            var response = new List<TipoProductoSubGradaData>();
            var info = connection.Query<TipoProductoSubDto>(
                @"SELECT Cod_Prodclas,
                         Cod_Linea_Sub,
                         Descripcion,
                         Activo,
                         Cabys,
                         COD_CUENTA,
                         NIVEL,
                         COD_LINEA_SUB_MADRE
                  FROM PV_PROD_CLASIFICA_SUB
                  WHERE Cod_Prodclas = @Cod_Prodclas
                    AND COD_LINEA_SUB_MADRE = @Cod_Linea_Sub_Madre",
                new
                {
                    padre.Cod_Prodclas,
                    Cod_Linea_Sub_Madre = padre.Cod_Linea_Sub
                }).ToList();

            foreach (TipoProductoSubDto dt in info)
            {
                CompletarEstadoSubcategoria(dt);

                response.Add(new TipoProductoSubGradaData
                {
                    key = dt.Cod_Linea_Sub,
                    icon = "",
                    label = dt.Descripcion,
                    data = dt,
                    children = TipoProductoSub_SeguienteNivel(connection, dt)
                });
            }

            return response;
        }

        /// <summary>
        /// Valida si puede cambiarse el nivel de un nodo raíz.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="codigoRaiz">Código a validar.</param>
        /// <returns>Resultado de la validación.</returns>
        private static ErrorDto FxValidaProfundidadRaiz(IDbConnection connection, string codigoRaiz)
        {
            var registro = connection.QueryFirstOrDefault(
                @"SELECT COD_LINEA_SUB,
                         COD_LINEA_SUB_MADRE,
                         (SELECT COUNT(*) FROM PV_PROD_CLASIFICA_SUB WHERE COD_LINEA_SUB_MADRE = p.COD_LINEA_SUB) AS HIJOS
                  FROM PV_PROD_CLASIFICA_SUB p
                  WHERE COD_LINEA_SUB = @codigoRaiz",
                new { codigoRaiz });

            if (registro == null)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "El registro no existe."
                };
            }

            if (registro.COD_LINEA_SUB_MADRE == null)
            {
                if (registro.HIJOS == 0)
                {
                    return new ErrorDto
                    {
                        Code = 0,
                        Description = "Es el primer ítem, se puede cambiar de nivel."
                    };
                }

                return new ErrorDto
                {
                    Code = -1,
                    Description = "No se puede cambiar el nivel del item raíz si tiene hijos."
                };
            }

            return new ErrorDto
            {
                Code = 0,
                Description = "El cambio de nivel es permitido."
            };
        }

        /// <summary>
        /// Valida que el cambio de categoría madre mantenga una jerarquía válida.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="request">Subcategoría que se está actualizando.</param>
        /// <param name="lineaMadreActual">Código actual de la categoría madre.</param>
        /// <param name="nuevaLineaMadre">Nuevo código de la categoría madre.</param>
        /// <returns>Error de validación o <see langword="null"/> cuando el cambio es válido.</returns>
        private static ErrorDto? ValidarCambioCategoriaMadre(
            IDbConnection connection,
            TipoProductoSubDto request,
            string? lineaMadreActual,
            string? nuevaLineaMadre)
        {
            if (string.Equals(request.Cod_Linea_Sub, nuevaLineaMadre, StringComparison.OrdinalIgnoreCase))
            {
                return new ErrorDto { Code = -1, Description = "Una subcategoría no puede ser su propia categoría madre." };
            }

            if (nuevaLineaMadre is not null &&
                EsDescendiente(connection, request.Cod_Prodclas, request.Cod_Linea_Sub, nuevaLineaMadre))
            {
                return new ErrorDto { Code = -1, Description = "No se puede seleccionar una categoría hija como categoría madre." };
            }

            if (string.Equals(lineaMadreActual, nuevaLineaMadre, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var validacion = FxValidaProfundidadRaiz(connection, request.Cod_Linea_Sub);
            return validacion.Code == -1 ? validacion : null;
        }

        /// <summary>
        /// Calcula el nivel correspondiente a la categoría madre seleccionada.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="nuevaLineaMadre">Código de la categoría madre.</param>
        /// <param name="nivel">Nivel calculado para la subcategoría.</param>
        /// <returns>Error de validación o <see langword="null"/> cuando el nivel es válido.</returns>
        private static ErrorDto? CalcularNivelSubcategoria(
            IDbConnection connection,
            string? nuevaLineaMadre,
            out int nivel)
        {
            nivel = 1;
            if (nuevaLineaMadre is null)
            {
                return null;
            }

            var nivelPadre = ObtenerNivelPadre(connection, nuevaLineaMadre);
            if (!nivelPadre.HasValue)
            {
                return new ErrorDto { Code = -1, Description = "La categoría madre seleccionada no existe." };
            }

            nivel = nivelPadre.Value + 1;
            return nivel > 5
                ? new ErrorDto { Code = -1, Description = "No se pueden agregar más de cinco niveles de subcategorías." }
                : null;
        }

        /// <summary>
        /// Crea los parámetros requeridos para actualizar una subcategoría.
        /// </summary>
        /// <param name="request">Datos de la subcategoría.</param>
        /// <param name="nuevaLineaMadre">Código normalizado de la categoría madre.</param>
        /// <param name="nivel">Nivel calculado de la subcategoría.</param>
        /// <returns>Parámetros de la consulta de actualización.</returns>
        private static DynamicParameters CrearParametrosActualizarSubcategoria(
            TipoProductoSubDto request,
            string? nuevaLineaMadre,
            int nivel)
        {
            var parameters = new DynamicParameters();
            parameters.Add("Cod_Prodclas", request.Cod_Prodclas, DbType.Int32);
            parameters.Add("Cod_Linea_Sub", request.Cod_Linea_Sub, DbType.String);
            parameters.Add("Descripcion", request.Descripcion, DbType.String);
            parameters.Add("Activo", request.Activo, DbType.Int32);
            parameters.Add("CABYS", request.Cabys, DbType.String);
            parameters.Add("COD_CUENTA", (request.Cod_Cuenta ?? string.Empty).Replace("-", string.Empty), DbType.String);
            parameters.Add("NIVEL", nivel, DbType.Int32);
            parameters.Add("COD_LINEA_SUB_MADRE", nuevaLineaMadre, DbType.String);
            return parameters;
        }

        #endregion

        #region Tipo Productos

        /// <summary>
        /// Obtiene la lista paginada de tipos de producto.
        /// </summary>
        /// <param name="CodCliente">Código de la empresa cliente.</param>
        /// <param name="pagina">Fila inicial para paginación.</param>
        /// <param name="paginacion">Cantidad de filas a retornar.</param>
        /// <param name="filtro">Filtro de búsqueda.</param>
        /// <param name="cod_contabilidad">Código de contabilidad.</param>
        /// <returns>Listado de tipos de producto.</returns>
        public ErrorDto<TipoProductoDataLista> TipoProducto_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro, int cod_contabilidad)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var respuesta = CrearTipoProductoListaVacia();
                var parametros = new DynamicParameters();
                parametros.Add("cod_contabilidad", cod_contabilidad);
                parametros.Add("Filtro", string.IsNullOrWhiteSpace(filtro) ? null : $"%{filtro.Trim()}%", DbType.String);

                respuesta.Total = connection.QueryFirstOrDefault<int>(TipoProductoConteoSql, parametros);
                var consulta = TipoProductoConsultaBaseSql;

                if (pagina.HasValue && paginacion.HasValue)
                {
                    consulta = TipoProductoConsultaPaginadaSql;
                    parametros.Add("Offset", pagina.Value);
                    parametros.Add("Fetch", paginacion.Value);
                }

                respuesta.Lista = connection.Query<TipoProductoDto>(consulta, parametros).ToList();
                return respuesta;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? CrearTipoProductoListaVacia())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener tipos de producto.", result.Code.GetValueOrDefault(-1), CrearTipoProductoListaVacia());
        }

        /// <summary>
        /// Obtiene la lista completa de tipos de producto.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="cod_contabilidad">Código de contabilidad.</param>
        /// <returns>Listado de tipos de producto.</returns>
        public List<TipoProductoDto> TipoProducto_ObtenerTodos(int CodEmpresa, int cod_contabilidad)
        {
            var result = DbHelper.ExecuteListQuery<TipoProductoDto>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT T.cod_prodclas,
                         T.descripcion,
                         T.costeo,
                         T.valuacion,
                         C.cod_cuenta_Mask as cod_cuenta,
                         T.cod_Alter,
                         C.descripcion as Cta_Desc,
                         (SELECT COUNT(Cod_Prodclas) FROM PV_PROD_CLASIFICA_SUB WHERE COD_PRODCLAS = T.cod_prodclas) AS Cantidad_Sub
                  FROM pv_prod_clasifica T
                  LEFT JOIN CntX_cuentas C ON T.cod_cuenta = C.cod_cuenta AND C.cod_contabilidad = @cod_contabilidad
                  ORDER BY T.cod_prodclas",
                new { cod_contabilidad });

            return result.Code == 0 ? result.Result ?? new List<TipoProductoDto>() : new List<TipoProductoDto>();
        }

        /// <summary>
        /// Actualiza el tipo de producto.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos del tipo de producto.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto TipoProducto_Actualizar(int CodEmpresa, TipoProductoDto request)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                ActualizarTipoProductoSql,
                new
                {
                    request.Cod_Prodclas,
                    request.Descripcion,
                    request.Costeo,
                    request.Valuacion,
                    Cod_Cuenta = (request.Cod_Cuenta ?? string.Empty).Replace("-", string.Empty),
                    request.Cod_Alter
                });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al actualizar el tipo de producto.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Inserta un tipo de producto.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos del tipo de producto.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto TipoProducto_Insertar(int CodEmpresa, TipoProductoDto request)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                InsertarTipoProductoSql,
                new
                {
                    request.Descripcion,
                    request.Costeo,
                    request.Valuacion,
                    Cod_Cuenta = (request.Cod_Cuenta ?? string.Empty).Replace("-", string.Empty),
                    request.Cod_Alter
                });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al insertar el tipo de producto.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Elimina un tipo de producto.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="producto">Código del tipo de producto.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto TipoProducto_Eliminar(int CodEmpresa, string producto)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                EliminarTipoProductoSql,
                new { Cod_Prodclas = producto });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al eliminar el tipo de producto.", result.Code.GetValueOrDefault(-1));
        }

        #endregion

        #region SubCategorías

        /// <summary>
        /// Obtiene la lista paginada de subcategorías de un tipo de producto.
        /// </summary>
        /// <param name="CodCliente">Código de la empresa cliente.</param>
        /// <param name="ProdClas">Código de clasificación padre.</param>
        /// <param name="pagina">Fila inicial para paginación.</param>
        /// <param name="paginacion">Cantidad de filas a retornar.</param>
        /// <param name="filtro">Filtro de búsqueda.</param>
        /// <returns>Listado de subcategorías.</returns>
        public ErrorDto<TipoProductoSubDataLista> TipoProductoSub_Obtener(int CodCliente, int ProdClas, int? pagina, int? paginacion, string? filtro)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var respuesta = CrearTipoProductoSubListaVacia();
                var parametros = new DynamicParameters();
                parametros.Add("ProdClas", ProdClas);
                parametros.Add("Filtro", string.IsNullOrWhiteSpace(filtro) ? null : $"%{filtro.Trim()}%", DbType.String);

                respuesta.Total = connection.QueryFirstOrDefault<int>(SubcategoriaConteoSql, parametros);
                var consulta = SubcategoriaConsultaBaseSql;

                if (pagina.HasValue && paginacion.HasValue)
                {
                    consulta = SubcategoriaConsultaPaginadaSql;
                    parametros.Add("Offset", pagina.Value);
                    parametros.Add("Fetch", paginacion.Value);
                }

                respuesta.Lista = connection.Query<TipoProductoSubDto>(consulta, parametros).ToList();
                return respuesta;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? CrearTipoProductoSubListaVacia())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener subcategorías.", result.Code.GetValueOrDefault(-1), CrearTipoProductoSubListaVacia());
        }

        /// <summary>
        /// Obtiene todas las subcategorías en estructura jerárquica.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Prodclas">Código de clasificación.</param>
        /// <returns>Estructura jerárquica de subcategorías.</returns>
        public ErrorDto<List<TipoProductoSubGradaData>> TipoProductoSub_ObtenerTodos(int CodEmpresa, string Cod_Prodclas)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var salida = new List<TipoProductoSubGradaData>();
                var info = connection.Query<TipoProductoSubDto>(
                    @"SELECT Cod_Prodclas,
                                 Cod_Linea_Sub,
                                 Descripcion,
                                 Activo,
                                 Cabys,
                                 COD_CUENTA,
                                 NIVEL,
                                 COD_LINEA_SUB_MADRE
                          FROM PV_PROD_CLASIFICA_SUB
                          WHERE Cod_Prodclas = @Cod_Prodclas",
                    new { Cod_Prodclas }).ToList();

                foreach (TipoProductoSubDto dt in info)
                {
                    CompletarEstadoSubcategoria(dt);

                    if (dt.Nivel == 1)
                    {
                        salida.Add(new TipoProductoSubGradaData
                        {
                            key = dt.Cod_Linea_Sub,
                            icon = "",
                            label = dt.Descripcion,
                            data = dt,
                            children = TipoProductoSub_SeguienteNivel(connection, dt)
                        });
                    }
                }

                return salida;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<TipoProductoSubGradaData>())
                : DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al obtener las subcategorías.",
                    result.Code.GetValueOrDefault(-1),
                    new List<TipoProductoSubGradaData>());
        }

        /// <summary>
        /// Actualiza una subcategoría.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos de la subcategoría.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto TipoProductoSub_Actualizar(int CodEmpresa, TipoProductoSubDto request)
        {
            var result = DbHelper.WithConn<ErrorDto>(CreatePortalDb(), CodEmpresa, connection =>
            {
                var lineaMadreActual = connection.QueryFirstOrDefault<string?>(
                    "SELECT CONVERT(VARCHAR(50), COD_LINEA_SUB_MADRE) FROM PV_PROD_CLASIFICA_SUB WHERE Cod_Prodclas = @Cod_Prodclas AND Cod_Linea_Sub = @Cod_Linea_Sub",
                    new { request.Cod_Prodclas, request.Cod_Linea_Sub });
                var nuevaLineaMadre = string.IsNullOrWhiteSpace(request.Cod_Linea_Sub_Madre)
                    ? null
                    : request.Cod_Linea_Sub_Madre.Trim();

                var validacionJerarquia = ValidarCambioCategoriaMadre(
                    connection,
                    request,
                    lineaMadreActual,
                    nuevaLineaMadre);
                if (validacionJerarquia is not null)
                {
                    return validacionJerarquia;
                }

                var validacionNivel = CalcularNivelSubcategoria(connection, nuevaLineaMadre, out int nivel);
                if (validacionNivel is not null)
                {
                    return validacionNivel;
                }

                var parameters = CrearParametrosActualizarSubcategoria(request, nuevaLineaMadre, nivel);
                connection.Execute(ActualizarSubcategoriaSql, parameters);
                return new ErrorDto { Code = 0, Description = "Ok" };
            });

            return result.Code == 0 ? result.Result ?? DbHelper.OkResponse("Ok") : DbHelper.ErrorResponse(result.Description ?? "Error al actualizar la subcategoría.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Inserta una subcategoría.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos de la subcategoría.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto TipoProductoSub_Insertar(int CodEmpresa, TipoProductoSubDto request)
        {
            var result = DbHelper.WithConn<bool>(CreatePortalDb(), CodEmpresa, connection =>
            {
                int consecutivo = ObtenerSiguienteCodLineaSub(connection);

                connection.Execute(
                    @"insert into pv_prod_clasifica_Sub(COD_PRODCLAS, COD_LINEA_SUB, DESCRIPCION, Activo, CABYS, COD_CUENTA, REGISTRO_FECHA, REGISTRO_USUARIO, NIVEL)
                      values(@Cod_Prodclas, @Cod_Linea_Sub, @Descripcion, @Activo, @CABYS, @COD_CUENTA, dbo.MyGetdate(), @Registro_Usuario, 1)",
                    new
                    {
                        request.Cod_Prodclas,
                        Cod_Linea_Sub = consecutivo,
                        request.Descripcion,
                        request.Activo,
                        CABYS = request.Cabys,
                        COD_CUENTA = (request.Cod_Cuenta ?? string.Empty).Replace("-", string.Empty),
                        request.Registro_Usuario,
                    });

                return true;
            });

            return result.Code == 0 && result.Result
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al insertar la subcategoría.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Obtiene todos los CABYS.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de CABYS.</returns>
        public ErrorDto<List<InvCabys>> Cabys_ObtenerTodos(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<InvCabys>(
                CreatePortalDb(),
                CodEmpresa,
                "SELECT COD_BYS, DESCRIPCION FROM vINV_Cabys");
        }

        /// <summary>
        /// Obtiene CABYS según filtro.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="filtro">Filtro de búsqueda.</param>
        /// <returns>Listado de CABYS.</returns>
        public ErrorDto<List<InvCabys>> Cabys_Obtener(int CodEmpresa, string filtro)
        {
            return DbHelper.ExecuteListQuery<InvCabys>(
                CreatePortalDb(),
                CodEmpresa,
                "SELECT TOP 30 COD_BYS, DESCRIPCION FROM vINV_Cabys WHERE COD_BYS like @filtro OR DESCRIPCION like @filtro",
                new { filtro = $"%{filtro}%" });
        }

        #endregion
    }
}
