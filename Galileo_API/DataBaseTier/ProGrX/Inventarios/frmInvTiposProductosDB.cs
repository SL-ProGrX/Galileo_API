using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Newtonsoft.Json;
using System.Data;
using System.Text;

namespace Galileo.DataBaseTier
{
    public class FrmInvTiposProductosDB
    {
        private readonly IConfiguration _config;
        private const string ParamCodProdclas = "Cod_Prodclas";
        private const string ParamDescripcion = "Descripcion";

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
        /// Crea una respuesta estándar para operaciones no query.
        /// </summary>
        /// <param name="result">Resultado devuelto por <see cref="DbHelper"/>.</param>
        /// <param name="successMessage">Mensaje de éxito.</param>
        /// <param name="errorMessage">Mensaje de error.</param>
        /// <returns>Respuesta estándar para operaciones no query.</returns>
        private static ErrorDto CrearRespuestaNonQuery(ErrorDto result, string successMessage, string errorMessage)
        {
            return result.Code == 0
                ? DbHelper.OkResponse(successMessage)
                : DbHelper.ErrorResponse(result.Description ?? errorMessage, result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Agrega filtro LIKE al listado de tipos de producto.
        /// </summary>
        /// <param name="filtro">Texto de filtro.</param>
        /// <param name="queryBuilder">Consulta a modificar.</param>
        /// <param name="parametros">Parámetros Dapper.</param>
        private static void AgregarFiltroTipoProducto(string? filtro, StringBuilder queryBuilder, DynamicParameters parametros)
        {
            if (string.IsNullOrWhiteSpace(filtro))
            {
                return;
            }

            queryBuilder.Append(@" WHERE T.cod_prodclas LIKE @Filtro
                                   OR T.DESCRIPCION LIKE @Filtro
                                   OR T.costeo LIKE @Filtro
                                   OR T.valuacion LIKE @Filtro ");
            parametros.Add("Filtro", $"%{filtro.Trim()}%");
        }

        /// <summary>
        /// Agrega filtro LIKE al listado de subcategorías.
        /// </summary>
        /// <param name="filtro">Texto de filtro.</param>
        /// <param name="queryBuilder">Consulta a modificar.</param>
        /// <param name="parametros">Parámetros Dapper.</param>
        private static void AgregarFiltroSubcategoria(string? filtro, StringBuilder queryBuilder, DynamicParameters parametros)
        {
            if (string.IsNullOrWhiteSpace(filtro))
            {
                return;
            }

            queryBuilder.Append(" WHERE Descripcion LIKE @Filtro OR Niveles LIKE @Filtro ");
            parametros.Add("Filtro", $"%{filtro.Trim()}%");
        }

        /// <summary>
        /// Agrega paginación OFFSET/FETCH a la consulta.
        /// </summary>
        /// <param name="pagina">Fila inicial.</param>
        /// <param name="paginacion">Cantidad de filas.</param>
        /// <param name="queryBuilder">Consulta a modificar.</param>
        /// <param name="parametros">Parámetros Dapper.</param>
        private static void AgregarPaginacion(int? pagina, int? paginacion, StringBuilder queryBuilder, DynamicParameters parametros)
        {
            if (!pagina.HasValue || !paginacion.HasValue)
            {
                return;
            }

            queryBuilder.Append(" OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY ");
            parametros.Add("Offset", pagina.Value);
            parametros.Add("Fetch", paginacion.Value);
        }

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
        private static int ObtenerNivelPadre(IDbConnection connection, string codLineaSubMadre)
        {
            return connection.QueryFirstOrDefault<int>(
                "SELECT NIVEL FROM PV_PROD_CLASIFICA_SUB WHERE Cod_Linea_Sub = @Cod_Linea_Sub_Madre",
                new { Cod_Linea_Sub_Madre = codLineaSubMadre });
        }

        /// <summary>
        /// Crea la consulta de actualización de subcategoría según si cambia o no la línea madre.
        /// </summary>
        /// <param name="actualizaLineaMadre">Indica si debe actualizarse COD_LINEA_SUB_MADRE.</param>
        /// <returns>Consulta SQL de actualización.</returns>
        private static string CrearConsultaActualizarSubcategoria(bool actualizaLineaMadre)
        {
            if (actualizaLineaMadre)
            {
                return @"Update pv_prod_clasifica_Sub set
                                    descripcion = @Descripcion,
                                    activo = @Activo,
                                    CABYS = @CABYS,
                                    COD_CUENTA = @COD_CUENTA,
                                    COD_LINEA_SUB_MADRE = @COD_LINEA_SUB_MADRE,
                                    NIVEL = @NIVEL
                                WHERE Cod_Prodclas = @Cod_Prodclas AND COD_LINEA_SUB = @Cod_Linea_Sub";
            }

            return @"Update pv_prod_clasifica_Sub set
                                    descripcion = @Descripcion,
                                    activo = @Activo,
                                    CABYS = @CABYS,
                                    COD_CUENTA = @COD_CUENTA,
                                    NIVEL = @NIVEL
                                WHERE Cod_Prodclas = @Cod_Prodclas AND COD_LINEA_SUB = @Cod_Linea_Sub";
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
                respuesta.Total = connection.QueryFirstOrDefault<int>(
                    @"SELECT Count(*)
                      FROM pv_prod_clasifica T
                      LEFT JOIN CntX_cuentas C ON T.cod_cuenta = C.cod_cuenta AND C.cod_contabilidad = @cod_contabilidad",
                    new { cod_contabilidad });

                var parametros = new DynamicParameters();
                parametros.Add("cod_contabilidad", cod_contabilidad);
                var queryBuilder = new StringBuilder(@"SELECT T.cod_prodclas,
                                                             T.descripcion,
                                                             T.costeo,
                                                             T.valuacion,
                                                             C.cod_cuenta_Mask as cod_cuenta,
                                                             T.cod_Alter,
                                                             C.descripcion as Cta_Desc,
                                                             (SELECT COUNT(Cod_Prodclas) FROM PV_PROD_CLASIFICA_SUB WHERE COD_PRODCLAS = T.cod_prodclas) AS Cantidad_Sub
                                                      FROM pv_prod_clasifica T
                                                      LEFT JOIN CntX_cuentas C ON T.cod_cuenta = C.cod_cuenta AND C.cod_contabilidad = @cod_contabilidad");

                AgregarFiltroTipoProducto(filtro, queryBuilder, parametros);
                queryBuilder.Append(" ORDER BY T.cod_prodclas desc ");
                AgregarPaginacion(pagina, paginacion, queryBuilder, parametros);

                respuesta.Lista = connection.Query<TipoProductoDto>(queryBuilder.ToString(), parametros).ToList();
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
                $"Update pv_prod_clasifica set descripcion = @{ParamDescripcion}, costeo = @Costeo, valuacion = @Valuacion, cod_cuenta = @Cod_Cuenta, cod_alter = @Cod_Alter where Cod_Prodclas = @{ParamCodProdclas}",
                new
                {
                    request.Cod_Prodclas,
                    request.Descripcion,
                    request.Costeo,
                    request.Valuacion,
                    Cod_Cuenta = request.Cod_Cuenta.Replace("-", string.Empty),
                    request.Cod_Alter
                });

            return CrearRespuestaNonQuery(result, "Ok", "Error al actualizar el tipo de producto.");
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
                $"insert into pv_prod_clasifica(descripcion,costeo,valuacion,cod_cuenta,cod_alter) values(@{ParamDescripcion}, @Costeo, @Valuacion, @Cod_Cuenta, @Cod_Alter)",
                new
                {
                    request.Descripcion,
                    request.Costeo,
                    request.Valuacion,
                    Cod_Cuenta = request.Cod_Cuenta.Replace("-", string.Empty),
                    request.Cod_Alter
                });

            return CrearRespuestaNonQuery(result, "Ok", "Error al insertar el tipo de producto.");
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
                $"DELETE pv_prod_clasifica where cod_prodclas = @{ParamCodProdclas}",
                new { Cod_Prodclas = producto });

            return CrearRespuestaNonQuery(result, "Ok", "Error al eliminar el tipo de producto.");
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
                respuesta.Total = connection.QueryFirstOrDefault<int>(
                    "SELECT count(*) FROM PV_PROD_CLASIFICA_SUB WHERE Cod_Prodclas = @ProdClas",
                    new { ProdClas });

                var parametros = new DynamicParameters();
                parametros.Add("ProdClas", ProdClas);
                var queryBuilder = new StringBuilder(@"WITH RecursiveHierarchy AS (
                                SELECT COD_LINEA_SUB,
                                       Cod_Prodclas,
                                       DESCRIPCION,
                                       COD_LINEA_SUB_MADRE,
                                       Activo,
                                       Cabys,
                                       COD_CUENTA,
                                       NIVEL,
                                       COD_LINEA_SUB_MADRE,
                                       CAST(CAST(COD_LINEA_SUB AS VARCHAR(MAX)) AS VARCHAR(MAX)) AS Niveles
                                FROM PV_PROD_CLASIFICA_SUB
                                WHERE COD_LINEA_SUB_MADRE IS NULL AND COD_PRODCLAS = @ProdClas

                                UNION ALL

                                SELECT p.COD_LINEA_SUB,
                                       p.Cod_Prodclas,
                                       p.DESCRIPCION,
                                       p.COD_LINEA_SUB_MADRE,
                                       p.Activo,
                                       p.Cabys,
                                       p.COD_CUENTA,
                                       p.NIVEL,
                                       p.COD_LINEA_SUB_MADRE,
                                       CONCAT(rh.Niveles, '.', p.NIVEL) AS Niveles
                                FROM PV_PROD_CLASIFICA_SUB p
                                INNER JOIN RecursiveHierarchy rh ON p.COD_LINEA_SUB_MADRE = rh.COD_LINEA_SUB
                                WHERE p.COD_PRODCLAS = @ProdClas
                            )
                            SELECT COD_LINEA_SUB,
                                   Cod_Prodclas,
                                   DESCRIPCION,
                                   COD_LINEA_SUB_MADRE,
                                   Activo,
                                   Cabys,
                                   COD_CUENTA,
                                   NIVEL,
                                   COD_LINEA_SUB_MADRE,
                                   Niveles
                            FROM RecursiveHierarchy");

                AgregarFiltroSubcategoria(filtro, queryBuilder, parametros);
                queryBuilder.Append(" ORDER BY Niveles ASC ");
                AgregarPaginacion(pagina, paginacion, queryBuilder, parametros);

                respuesta.Lista = connection.Query<TipoProductoSubDto>(queryBuilder.ToString(), parametros).ToList();
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
            var response = new ErrorDto<List<TipoProductoSubGradaData>>
            {
                Code = 0,
                Result = new List<TipoProductoSubGradaData>()
            };

            try
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

                response.Result = result.Code == 0 ? result.Result ?? new List<TipoProductoSubGradaData>() : new List<TipoProductoSubGradaData>();
                if (result.Code != 0)
                {
                    response.Code = result.Code.GetValueOrDefault(-1);
                    response.Description = result.Description;
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new List<TipoProductoSubGradaData>();
            }

            return response;
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
                var valida = FxValidaProfundidadRaiz(connection, request.Cod_Linea_Sub);
                if (valida.Code == -1)
                {
                    return valida;
                }

                int nivel = 0;
                if (request.Cod_Linea_Sub_Madre != "")
                {
                    nivel = ObtenerNivelPadre(connection, request.Cod_Linea_Sub_Madre);
                }

                bool actualizaLineaMadre = nivel > 0;
                if (actualizaLineaMadre)
                {
                    nivel += 1;

                    if (nivel > 5)
                    {
                        return new ErrorDto { Code = -1, Description = "No se puede agregar mas subcategorias" };
                    }
                }
                else
                {
                    nivel = request.Nivel;
                }

                var query = CrearConsultaActualizarSubcategoria(actualizaLineaMadre);

                var parameters = new DynamicParameters();
                parameters.Add("Cod_Prodclas", request.Cod_Prodclas, DbType.Int32);
                parameters.Add("Cod_Linea_Sub", request.Cod_Linea_Sub, DbType.String);
                parameters.Add("Descripcion", request.Descripcion, DbType.String);
                parameters.Add("Activo", request.Activo, DbType.Int32);
                parameters.Add("CABYS", request.Cabys, DbType.String);
                parameters.Add("COD_CUENTA", request.Cod_Cuenta, DbType.String);
                parameters.Add("NIVEL", nivel, DbType.Int32);

                if (actualizaLineaMadre)
                {
                    parameters.Add("COD_LINEA_SUB_MADRE", request.Cod_Linea_Sub_Madre, DbType.Int32);
                }

                connection.Execute(query, parameters);
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
                    @"insert into pv_prod_clasifica_Sub(COD_PRODCLAS, COD_LINEA_SUB, DESCRIPCION, Activo, CABYS, REGISTRO_FECHA, REGISTRO_USUARIO, NIVEL)
                      values(@Cod_Prodclas, @Cod_Linea_Sub, @Descripcion, @Activo, @CABYS, @Registro_Fecha, @Registro_Usuario, 1)",
                    new
                    {
                        request.Cod_Prodclas,
                        Cod_Linea_Sub = consecutivo,
                        request.Descripcion,
                        request.Activo,
                        CABYS = request.Cabys,
                        request.Registro_Usuario,
                        Registro_Fecha = DateTime.Now
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