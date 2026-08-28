using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using System.Data;
using System.Text;

namespace Galileo.DataBaseTier
{
    public partial class FrmInvProductosDB
    {
        #region Constructor y helpers

        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Crea una respuesta estándar para operaciones de consulta única.
        /// </summary>
        /// <typeparam name="T">Tipo del resultado esperado.</typeparam>
        /// <param name="result">Resultado devuelto por <see cref="DbHelper"/>.</param>
        /// <param name="errorMessage">Mensaje cuando ocurre un error.</param>
        /// <param name="notFoundMessage">Mensaje cuando no se encuentra información.</param>
        /// <returns>Respuesta estándar para una sola entidad.</returns>
        private static ErrorDto<T> CrearRespuestaSingle<T>(ErrorDto<T?> result, string errorMessage, string notFoundMessage)
            where T : class
        {
            if (result.Code != 0)
            {
                return new ErrorDto<T>
                {
                    Code = result.Code,
                    Description = result.Description ?? errorMessage,
                    Result = default
                };
            }

            if (result.Result is not null)
            {
                return DbHelper.CreateOkResponse(result.Result);
            }

            return new ErrorDto<T>
            {
                Code = -2,
                Description = notFoundMessage,
                Result = default
            };
        }

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
        /// Crea una respuesta estándar para operaciones de listado.
        /// </summary>
        /// <typeparam name="T">Tipo de elemento de la lista.</typeparam>
        /// <param name="result">Resultado devuelto por <see cref="DbHelper"/>.</param>
        /// <param name="errorMessage">Mensaje de error.</param>
        /// <returns>Respuesta estándar con lista inicializada.</returns>
        private static ErrorDto<List<T>> CrearRespuestaLista<T>(ErrorDto<List<T>> result, string errorMessage)
        {
            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<T>())
                : DbHelper.CreateErrorResponse(result.Description ?? errorMessage, result.Code.GetValueOrDefault(-1), new List<T>());
        }

        /// <summary>
        /// Crea parámetros para un código de producto.
        /// </summary>
        /// <param name="codProducto">Código del producto.</param>
        /// <returns>Objeto de parámetros para Dapper.</returns>
        private static object CrearParametrosProducto(string codProducto) => new
        {
            Cod_Producto = codProducto,
            Cod_Prodclas = codProducto
        };

        /// <summary>
        /// Crea parámetros para producto y consecutivo.
        /// </summary>
        /// <param name="codProducto">Código del producto.</param>
        /// <param name="consec">Consecutivo.</param>
        /// <returns>Objeto de parámetros para Dapper.</returns>
        private static object CrearParametrosProductoConsec(string codProducto, int consec) => new
        {
            Cod_Producto = codProducto,
            Consec = consec
        };

        /// <summary>
        /// Convierte los indicadores del producto a propiedades booleanas derivadas.
        /// </summary>
        /// <param name="producto">Producto a normalizar.</param>
        private static void NormalizarProducto(ProductoDto producto)
        {
            producto.Inventario_Calculabool = producto.Inventario_Calcula == "S";
            producto.Lotesbool = producto.Lotes == 1;
        }

        /// <summary>
        /// Convierte los indicadores del listado de productos a propiedades booleanas derivadas.
        /// </summary>
        /// <param name="productos">Listado de productos.</param>
        private static void NormalizarProductos(IEnumerable<ProductoDto> productos)
        {
            foreach (ProductoDto item in productos)
            {
                NormalizarProducto(item);
            }
        }

        /// <summary>
        /// Obtiene el siguiente código de producto disponible.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <returns>Código de producto generado.</returns>
        private static string ObtenerSiguienteCodigoProducto(IDbConnection connection)
        {
            return connection.QueryFirstOrDefault<string>("select FORMAT(NEXT VALUE FOR SeqPv_Productos, '000000')") ?? string.Empty;
        }

        /// <summary>
        /// Registra una bitácora de producto.
        /// </summary>
        /// <param name="empresaId">Código de empresa.</param>
        /// <param name="codProducto">Código del producto.</param>
        /// <param name="movimiento">Movimiento realizado.</param>
        /// <param name="detalle">Detalle de la operación.</param>
        /// <param name="usuario">Usuario responsable.</param>
        private void RegistrarBitacoraProducto(int empresaId, string codProducto, string movimiento, string detalle, string usuario)
        {
            mAuxiliarDB.BitacoraProducto(new BitacoraProductoInsertarDto
            {
                EmpresaId = empresaId,
                cod_producto = codProducto,
                consec = 0,
                movimiento = movimiento,
                detalle = detalle,
                registro_usuario = usuario
            });
        }

        /// <summary>
        /// Ejecuta un procedimiento almacenado que devuelve un código entero y lo transforma en <see cref="ErrorDto"/>.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="procedure">Nombre del procedimiento.</param>
        /// <param name="values">Parámetros del procedimiento.</param>
        /// <param name="errorMessage">Mensaje de error estándar.</param>
        /// <returns>Respuesta estándar.</returns>
        private ErrorDto EjecutarProcedimientoConCodigo(int codEmpresa, string procedure, object values, string errorMessage)
        {
            var result = DbHelper.WithConn<int>(CreatePortalDb(), codEmpresa, connection =>
                connection.QueryFirstOrDefault<int>(procedure, values, commandType: CommandType.StoredProcedure));

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? errorMessage, result.Code.GetValueOrDefault(-1));
            }

            return result.Result == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(errorMessage, result.Result);
        }

        /// <summary>
        /// Obtiene el total de proveedores para el listado paginado.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <returns>Total de proveedores.</returns>
        private static int ObtenerTotalProveedores(IDbConnection connection)
        {
            return connection.QueryFirstOrDefault<int>("Select COUNT(*) from CXP_PROVEEDORES");
        }

        /// <summary>
        /// Agrega el filtro de proveedores al query paginado.
        /// </summary>
        /// <param name="producto">Código del producto.</param>
        /// <param name="filtro">Texto filtro.</param>
        /// <param name="queryBuilder">Builder del query.</param>
        /// <param name="parametros">Parámetros Dapper.</param>
        private static void AgregarFiltroProveedores(string producto, string? filtro, StringBuilder queryBuilder, DynamicParameters parametros)
        {
            queryBuilder.Append(@" WHERE 1 = 1 ");
            parametros.Add("Cod_Producto", producto);

            if (string.IsNullOrWhiteSpace(filtro) || filtro == "undefined")
            {
                return;
            }

            queryBuilder.Append(" AND (P.CEDJUR LIKE @Filtro OR UPPER(P.DESCRIPCION) LIKE @FiltroMayus) ");
            parametros.Add("Filtro", $"%{filtro.Trim()}%");
            parametros.Add("FiltroMayus", $"%{filtro.Trim().ToUpper()}%");
        }

        /// <summary>
        /// Agrega paginación OFFSET/FETCH a una consulta.
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
        /// Traduce el tipo de movimiento a su descripción legible.
        /// </summary>
        /// <param name="tipo">Tipo de movimiento.</param>
        /// <returns>Descripción del tipo.</returns>
        private static string ObtenerDescripcionMovimiento(string? tipo)
        {
            return tipo switch
            {
                "E" => "ENTRADA",
                "S" => "SALIDA",
                "T" => "TRASLADO",
                _ => "N/A"
            };
        }

        #endregion

        #region Productos

        /// <summary>
        /// Consulta el siguiente o anterior producto según el tipo indicado.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Producto">Código actual del producto.</param>
        /// <param name="tipo">Dirección del desplazamiento.</param>
        /// <returns>Producto encontrado.</returns>
        public ErrorDto<Producto> ConsultaAscDesc(int CodEmpresa, string Cod_Producto, string tipo)
        {
            string query;
            object? parametros = null;

            if (tipo == "desc")
            {
                if (Cod_Producto == "0")
                {
                    query = @"select Top 1 cod_producto from pv_productos order by cod_producto desc";
                }
                else
                {
                    query = @"select Top 1 cod_producto from pv_productos where cod_producto < @Cod_Producto order by cod_producto desc";
                    parametros = new { Cod_Producto };
                }
            }
            else
            {
                if (Cod_Producto == "0")
                {
                    query = @"select Top 1 cod_producto from pv_productos order by cod_producto asc";
                }
                else
                {
                    query = @"select Top 1 cod_producto from pv_productos where cod_producto > @Cod_Producto order by cod_producto asc";
                    parametros = new { Cod_Producto };
                }
            }

            var result = DbHelper.ExecuteSingleQuery<Producto>(
                CreatePortalDb(),
                CodEmpresa,
                query,
                new Producto(),
                parametros);

            return CrearRespuestaSingle(result, "Error al consultar el producto.", "No se encontró un producto para el desplazamiento solicitado.");
        }

        /// <summary>
        /// Obtiene la lista de productos para selección.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de productos.</returns>
        public ErrorDto<List<ProductoDto>> Producto_ObtenerTodos(int CodEmpresa)
        {
            var result = DbHelper.ExecuteListQuery<ProductoDto>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT P.*,C.descripcion AS ProdClas,U.descripcion AS UnidadDesc,M.Descripcion AS MarcaDesc,
                         isnull(Cs.Descripcion,'') AS LineaSub,
                         isnull(P.COD_LINEA_SUB,'') AS LineaSubCod
                  FROM pv_productos P
                  INNER JOIN pv_unidades U ON P.cod_unidad = U.cod_unidad
                  INNER JOIN pv_prod_clasifica C ON P.cod_prodclas = C.cod_prodclas
                  INNER JOIN pv_marcas M ON P.cod_marca = M.cod_marca
                  INNER JOIN PV_PROD_CLASIFICA_SUB Cs ON P.cod_prodclas = Cs.cod_prodclas
                                                    AND P.COD_LINEA_SUB = Cs.COD_LINEA_SUB");

            if (result.Code == 0 && result.Result is not null)
            {
                NormalizarProductos(result.Result);
            }

            return CrearRespuestaLista(result, "Error al obtener los productos.");
        }

        /// <summary>
        /// Obtiene el detalle del producto.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Producto">Código del producto.</param>
        /// <returns>Detalle del producto.</returns>
        public ErrorDto<ProductoDto> Producto_ObtenerDetalle(int CodEmpresa, string Cod_Producto)
        {
            var result = DbHelper.ExecuteSingleQuery<ProductoDto>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT CONCAT(FORMAT(Cs.COD_PRODCLAS, ' 0'),
                                FORMAT(ISNULL(Cs.NIVEL,'00'), ' 0'),
                                FORMAT(ISNULL(Cs.COD_LINEA_SUB_MADRE,1), ' 0'),
                                FORMAT(ISNULL(Cs.COD_LINEA_SUB,1), ' ')) AS CODIGO,
                         P.*,C.descripcion AS ProdClass,U.descripcion AS UnidadDesc,M.Descripcion AS MarcaDesc,
                         isnull(Cs.Descripcion,'') AS LineaSub,
                         isnull(P.COD_LINEA_SUB,'') AS LineaSubCod
                  FROM pv_productos P
                  INNER JOIN pv_unidades U ON P.cod_unidad = U.cod_unidad
                  INNER JOIN pv_prod_clasifica C ON P.cod_prodclas = C.cod_prodclas
                  INNER JOIN pv_marcas M ON P.cod_marca = M.cod_marca
                  INNER JOIN PV_PROD_CLASIFICA_SUB Cs ON P.cod_prodclas = Cs.cod_prodclas
                                                    AND P.COD_LINEA_SUB = Cs.COD_LINEA_SUB
                  WHERE P.cod_producto = @Cod_Producto",
                new ProductoDto
                {
                    Cod_Prodclas = default,
                    Lotes = default,
                    Lotesbool = default,
                    Costo_Regular = default,
                    Precio_Regular = default,
                    Comision_Monto = default,
                    Comision_Unidad = default,
                    Impuesto_Ventas = default,
                    Impuesto_Consumo = default,
                    Inventario_Calculabool = default,
                    Inventario_Minimo = default,
                    Inventario_Maximo = default,
                    Fracciones = default,
                    Precio_Compra = default,
                    Descuento_Valor = default,
                    Existencia = default,
                    Porc_Utilidad = default,
                    Tipo_Cambio = default,
                    Similar = default
                },
                new { Cod_Producto });

            var respuesta = CrearRespuestaSingle(result, "Error al obtener el detalle del producto.", "No se encontró el producto indicado.");
            if (respuesta.Result != null)
            {
                NormalizarProducto(respuesta.Result);
            }

            return respuesta;
        }

        /// <summary>
        /// Obtiene el código CABYS a heredar en el producto.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Prodclas">Código de clasificación.</param>
        /// <param name="Cod_Linea_Sub">Código de sublínea.</param>
        /// <returns>Código CABYS heredado.</returns>
        public ErrorDto<CabysHereda> Producto_ObtenerCabys(int CodEmpresa, int Cod_Prodclas, string Cod_Linea_Sub)
        {
            var result = DbHelper.ExecuteSingleQuery<CabysHereda>(
                CreatePortalDb(),
                CodEmpresa,
                "select Cabys from pv_prod_clasifica_Sub where COD_PRODCLAS = @Cod_Prodclas and COD_LINEA_SUB = @Cod_Linea_Sub",
                new CabysHereda(),
                new { Cod_Prodclas, Cod_Linea_Sub });

            return CrearRespuestaSingle(result, "Error al obtener el CABYS heredado.", "No se encontró CABYS para la línea indicada.");
        }

        /// <summary>
        /// Inserta un nuevo producto.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos del producto.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Producto_Insertar(int CodEmpresa, ProductoDto request)
        {
            var result = DbHelper.WithConn<int>(CreatePortalDb(), CodEmpresa, connection =>
            {
                if (request.Cod_Producto == "0")
                {
                    request.Cod_Producto = ObtenerSiguienteCodigoProducto(connection);
                }

                return connection.QueryFirstOrDefault<int>(
                    "[spINV_W_Producto_Agregar]",
                    new
                    {
                        Cod_Producto = request.Cod_Producto,
                        Descripcion = request.Descripcion,
                        Observacion = request.Observacion,
                        Estado = request.Estado,
                        Cod_Barras = request.Cod_Barras,
                        Cabys = request.Cabys,
                        Cod_Unidad = request.Cod_Unidad,
                        Cod_Marca = request.Cod_Marca,
                        Cod_Prodclas = request.Cod_Prodclas,
                        Tipo_Producto = request.Tipo_Producto,
                        Cod_Fabricante = request.Cod_Fabricante,
                        Inventario_Minimo = request.Inventario_Minimo,
                        Inventario_Maximo = request.Inventario_Maximo,
                        Inventario_Calcula = request.Inventario_Calcula,
                        Costo_Regular = request.Costo_Regular,
                        Precio_Regular = request.Precio_Regular,
                        Impuesto_Consumo = request.Impuesto_Consumo,
                        Impuesto_Ventas = request.Impuesto_Ventas,
                        Comision_Monto = request.Comision_Monto,
                        Comision_Unidad = request.Comision_Unidad,
                        User_Crea = request.User_Crea,
                        Porc_Utilidad = request.Porc_Utilidad,
                        Modelo = request.Modelo,
                        Lotes = request.Lotes,
                        Cod_Linea_Sub = request.Cod_Linea_Sub,
                        I_Filtrado = request.i_filtrado,
                        Punto_Reorden = request.punto_reorden,
                        Tiempo_entrega_dias = request.tiempo_entrega_dias,
                        tipo_activo = request.tipo_activo,
                        Presentacion = request.Presentacion,
                        Cant_Presentacion = request.Cant_Presentacion,
                        Volumen = request.Volumen
                    },
                    commandType: CommandType.StoredProcedure);
            });

            if (result.Code == 0 && result.Result == 0)
            {
                RegistrarBitacoraProducto(
                    CodEmpresa,
                    request.Cod_Producto,
                    "Inserta",
                    $@"Se inserta el producto {request.Cod_Producto}",
                    request.User_Modifica);

                return DbHelper.OkResponse("Ok");
            }

            return DbHelper.ErrorResponse(result.Description ?? "Error al insertar el producto.", result.Code != 0 ? result.Code.GetValueOrDefault(-1) : result.Result);
        }

        /// <summary>
        /// Actualiza el producto.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos del producto.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Producto_Actualizar(int CodEmpresa, ProductoDto request)
        {
            var result = DbHelper.WithConn<int>(CreatePortalDb(), CodEmpresa, connection =>
                connection.QueryFirstOrDefault<int>(
                    "[spINV_W_Producto_Editar]",
                    new
                    {
                        Cod_Producto = request.Cod_Producto,
                        Descripcion = request.Descripcion,
                        Observacion = request.Observacion,
                        Estado = request.Estado,
                        Tipo_Producto = request.Tipo_Producto,
                        Cabys = request.Cabys,
                        Cod_Barras = request.Cod_Barras,
                        Cod_Unidad = request.Cod_Unidad,
                        Cod_Prodclas = request.Cod_Prodclas,
                        Cod_Marca = request.Cod_Marca,
                        Modelo = request.Modelo,
                        Cod_Fabricante = request.Cod_Fabricante,
                        Inventario_Minimo = request.Inventario_Minimo,
                        Inventario_Maximo = request.Inventario_Maximo,
                        Inventario_Calcula = request.Inventario_Calcula,
                        Costo_Regular = request.Costo_Regular,
                        Precio_Regular = request.Precio_Regular,
                        Porc_Utilidad = request.Porc_Utilidad,
                        Impuesto_Consumo = request.Impuesto_Consumo,
                        Impuesto_Ventas = request.Impuesto_Ventas,
                        Comision_Monto = request.Comision_Monto,
                        Comision_Unidad = request.Comision_Unidad,
                        Lotes = request.Lotes,
                        User_Modifica = request.User_Modifica,
                        Cod_Linea_Sub = request.Cod_Linea_Sub,
                        I_Filtrado = request.i_filtrado,
                        Punto_Reorden = request.punto_reorden,
                        Tiempo_entrega_dias = request.tiempo_entrega_dias,
                        Tipo_activo = request.tipo_activo,
                        Presentacion = request.Presentacion,
                        Cant_Presentacion = request.Cant_Presentacion,
                        Volumen = request.Volumen
                    },
                    commandType: CommandType.StoredProcedure));

            if (result.Code == 0 && result.Result == 0)
            {
                if (request.justificacion_estado != "")
                {
                    RegistrarBitacoraProducto(
                        CodEmpresa,
                        request.Cod_Producto,
                        "Inserta",
                        $@"{request.justificacion_estado}",
                        request.User_Modifica);
                }

                RegistrarBitacoraProducto(
                    CodEmpresa,
                    request.Cod_Producto,
                    "Actualiza",
                    $@"Se actualiza datos del producto {request.Cod_Producto}",
                    request.User_Modifica);

                return DbHelper.OkResponse("Ok");
            }

            return DbHelper.ErrorResponse(result.Description ?? "Error al actualizar el producto.", result.Code != 0 ? result.Code.GetValueOrDefault(-1) : result.Result);
        }

        #endregion

        #region Precios

        /// <summary>
        /// Obtiene todos los precios del producto.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Producto">Código del producto.</param>
        /// <returns>Listado de precios del producto.</returns>
        public ErrorDto<List<PrecioProducto>> PreciosProducto_ObtenerTodos(int CodEmpresa, string Cod_Producto)
        {
            var result = DbHelper.ExecuteListQuery<PrecioProducto>(
                CreatePortalDb(),
                CodEmpresa,
                @"select P.*,isnull(X.monto,0) as Monto,isnull(X.porc_utilidad,0) as Utilidad
                  from pv_tipos_precios P
                  left join pv_producto_precios X on P.cod_precio = X.cod_precio
                                                and X.cod_producto = @Cod_Prodclas
                  order by P.defecto desc, X.monto",
                CrearParametrosProducto(Cod_Producto));

            return CrearRespuestaLista(result, "Error al obtener los precios del producto.");
        }

        /// <summary>
        /// Agrega o actualiza el precio del producto.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos del precio.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto PrecioProducto_AgregarActualizar(int CodEmpresa, PrecioProducto request)
        {
            return EjecutarProcedimientoConCodigo(
                CodEmpresa,
                "[spINV_W_ProductoPrecio_AgregarEditar]",
                new
                {
                    Cod_Producto = request.Cod_Producto,
                    Cod_Precio = request.Cod_Precio,
                    Monto = request.Monto,
                    Porc_Utilidad = request.Utilidad
                },
                "Error al agregar o actualizar el precio del producto.");
        }

        #endregion
    }
}

