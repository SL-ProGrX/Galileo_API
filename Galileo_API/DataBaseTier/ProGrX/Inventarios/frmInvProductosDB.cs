using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using System.Data;
using System.Text;

namespace Galileo.DataBaseTier
{
    public class FrmInvProductosDB
    {
        private readonly IConfiguration _config;
        private readonly MProGrXAuxiliarDB mAuxiliarDB;

        #region Constructor y helpers

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmInvProductosDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmInvProductosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            mAuxiliarDB = new MProGrXAuxiliarDB(_config);
        }

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
            where T : class, new()
        {
            if (result.Code != 0)
            {
                return new ErrorDto<T>
                {
                    Code = result.Code,
                    Description = result.Description ?? errorMessage,
                    Result = new T()
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
                Result = new T()
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
                new ProductoDto(),
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

        #region Bonificaciones

        /// <summary>
        /// Obtiene las bonificaciones del producto.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Producto">Código del producto.</param>
        /// <returns>Listado de bonificaciones.</returns>
        public ErrorDto<List<BonificacionProducto>> BonificacionProducto_ObtenerTodos(int CodEmpresa, string Cod_Producto)
        {
            var result = DbHelper.ExecuteListQuery<BonificacionProducto>(
                CreatePortalDb(),
                CodEmpresa,
                "select cod_producto,consec,desde,hasta,bonificacion from pv_producto_bonif where cod_producto = @Cod_Producto order by desde",
                new { Cod_Producto });

            return CrearRespuestaLista(result, "Error al obtener las bonificaciones del producto.");
        }

        /// <summary>
        /// Actualiza la bonificación del producto.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos de la bonificación.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto BonificacionProducto_Actualizar(int CodEmpresa, BonificacionProducto request)
        {
            return EjecutarProcedimientoConCodigo(
                CodEmpresa,
                "[spINV_W_ProductoBonificacion_Editar]",
                new
                {
                    Cod_Producto = request.Cod_Producto,
                    Consec = request.Consec,
                    Desde = request.Desde,
                    Hasta = request.Hasta,
                    Bonificacion = request.Bonificacion
                },
                "Error al actualizar la bonificación del producto.");
        }

        /// <summary>
        /// Agrega una bonificación al producto.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos de la bonificación.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto BonificacionProducto_Agregar(int CodEmpresa, BonificacionProducto request)
        {
            return EjecutarProcedimientoConCodigo(
                CodEmpresa,
                "[spINV_W_ProductoBonificacion_Agregar]",
                new
                {
                    Cod_Producto = request.Cod_Producto,
                    Consec = request.Consec,
                    Desde = request.Desde,
                    Hasta = request.Hasta,
                    Bonificacion = request.Bonificacion
                },
                "Error al agregar la bonificación del producto.");
        }

        /// <summary>
        /// Elimina la bonificación del producto.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos de la bonificación.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto BonificacionProducto_Eliminar(int CodEmpresa, BonificacionProducto request)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                "DELETE pv_producto_bonif WHERE consec = @Consec AND cod_producto = @Cod_Producto",
                CrearParametrosProductoConsec(request.Cod_Producto, request.Consec));

            return CrearRespuestaNonQuery(result, "Ok", "Error al eliminar la bonificación del producto.");
        }

        #endregion

        #region Descuentos

        /// <summary>
        /// Obtiene todos los descuentos del producto.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Producto">Código del producto.</param>
        /// <returns>Listado de descuentos.</returns>
        public ErrorDto<List<DescuentoProducto>> DescuentoProducto_ObtenerTodos(int CodEmpresa, string Cod_Producto)
        {
            var result = DbHelper.ExecuteListQuery<DescuentoProducto>(
                CreatePortalDb(),
                CodEmpresa,
                "select cod_producto,consec,desde,hasta,porcentaje from pv_producto_desc where cod_producto = @Cod_Producto order by desde",
                new { Cod_Producto });

            return CrearRespuestaLista(result, "Error al obtener los descuentos del producto.");
        }

        /// <summary>
        /// Actualiza el descuento del producto.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos del descuento.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto DescuentoProducto_Actualizar(int CodEmpresa, DescuentoProducto request)
        {
            return EjecutarProcedimientoConCodigo(
                CodEmpresa,
                "[spINV_W_ProductoDescuento_Editar]",
                new
                {
                    Cod_Producto = request.Cod_Producto,
                    Consec = request.Consec,
                    Desde = request.Desde,
                    Hasta = request.Hasta,
                    Porcentaje = request.Porcentaje
                },
                "Error al actualizar el descuento del producto.");
        }

        /// <summary>
        /// Agrega un descuento al producto.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos del descuento.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto DescuentoProducto_Agregar(int CodEmpresa, DescuentoProducto request)
        {
            return EjecutarProcedimientoConCodigo(
                CodEmpresa,
                "[spINV_W_ProductoDescuento_Agregar]",
                new
                {
                    Cod_Producto = request.Cod_Producto,
                    Consec = request.Consec,
                    Desde = request.Desde,
                    Hasta = request.Hasta,
                    Porcentaje = request.Porcentaje
                },
                "Error al agregar el descuento del producto.");
        }

        /// <summary>
        /// Elimina el descuento del producto.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos del descuento.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto DescuentoProducto_Eliminar(int CodEmpresa, DescuentoProducto request)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                "DELETE pv_producto_desc WHERE consec = @Consec AND cod_producto = @Cod_Producto",
                CrearParametrosProductoConsec(request.Cod_Producto, request.Consec));

            return CrearRespuestaNonQuery(result, "Ok", "Error al eliminar el descuento del producto.");
        }

        #endregion

        #region Proveedores

        /// <summary>
        /// Obtiene la lista paginada de proveedores de un producto.
        /// </summary>
        /// <param name="CodCliente">Código de la empresa cliente.</param>
        /// <param name="producto">Código del producto.</param>
        /// <param name="pagina">Fila inicial para paginación.</param>
        /// <param name="paginacion">Cantidad de filas a retornar.</param>
        /// <param name="filtro">Filtro por cédula o descripción.</param>
        /// <returns>Listado de proveedores del producto.</returns>
        public ErrorDto<ProvProductoDataLista> ProveedoresProducto_Obtener(int CodCliente, string producto, int? pagina, int? paginacion, string? filtro)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var respuesta = new ProvProductoDataLista
                {
                    Total = ObtenerTotalProveedores(connection),
                    Lista = new List<ProveedorProducto>()
                };

                var parametros = new DynamicParameters();
                var queryBuilder = new StringBuilder(@"SELECT COD_PROVEEDOR, DESCRIPCION, CEDJUR, FECHA_FACTURA,
                                                            CASE WHEN CodX IS NULL THEN 0 ELSE CodX END AS CodX
                                                     FROM (
                                                         Select P.COD_PROVEEDOR, P.DESCRIPCION, P.CEDJUR, max(F.fecha) as FECHA_FACTURA,
                                                                X.cod_proveedor as CodX
                                                         from CXP_PROVEEDORES P
                                                         left join pv_producto_prov X on P.COD_PROVEEDOR = X.COD_PROVEEDOR and X.cod_producto = @Cod_Producto
                                                         left join vCxP_Facturas F on P.COD_PROVEEDOR = F.COD_PROVEEDOR");

                AgregarFiltroProveedores(producto, filtro, queryBuilder, parametros);
                queryBuilder.Append(@" GROUP BY P.COD_PROVEEDOR, P.DESCRIPCION, P.CEDJUR, X.cod_proveedor
                                      ) T
                                      order by CodX DESC");
                AgregarPaginacion(pagina, paginacion, queryBuilder, parametros);

                respuesta.Lista = connection.Query<ProveedorProducto>(queryBuilder.ToString(), parametros).ToList();
                return respuesta;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new ProvProductoDataLista())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener proveedores del producto.", result.Code.GetValueOrDefault(-1), new ProvProductoDataLista());
        }

        /// <summary>
        /// Obtiene todos los proveedores de un producto.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Producto">Código del producto.</param>
        /// <returns>Listado de proveedores.</returns>
        public ErrorDto<List<ProveedorProducto>> ProveedorProducto_ObtenerTodos(int CodEmpresa, string Cod_Producto)
        {
            var result = DbHelper.ExecuteListQuery<ProveedorProducto>(
                CreatePortalDb(),
                CodEmpresa,
                @"Select P.COD_PROVEEDOR, P.DESCRIPCION, P.CEDJUR, max(F.fecha) as FECHA_FACTURA, X.cod_proveedor as CodX
                  from CXP_PROVEEDORES P
                  left join pv_producto_prov X on P.COD_PROVEEDOR = X.COD_PROVEEDOR and X.cod_producto = @Cod_Producto
                  left join vCxP_Facturas F on P.COD_PROVEEDOR = F.COD_PROVEEDOR
                  GROUP BY P.COD_PROVEEDOR, P.DESCRIPCION, P.CEDJUR, X.cod_proveedor
                  order by X.cod_proveedor desc, P.Descripcion",
                new { Cod_Producto });

            return CrearRespuestaLista(result, "Error al obtener todos los proveedores del producto.");
        }

        /// <summary>
        /// Inserta un nuevo proveedor para el producto.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos del proveedor.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto ProveedorProducto_Insertar(int CodEmpresa, ProveedorProducto request)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                "insert into pv_producto_prov(cod_producto, cod_proveedor) values(@Cod_Producto, @Cod_Proveedor)",
                new { request.Cod_Producto, request.Cod_Proveedor });

            return CrearRespuestaNonQuery(result, "Ok", "Error al insertar el proveedor del producto.");
        }

        /// <summary>
        /// Elimina un proveedor del producto.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos del proveedor.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto ProveedorProducto_Eliminar(int CodEmpresa, ProveedorProducto request)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                "DELETE pv_producto_prov where cod_producto = @Cod_Producto and cod_proveedor = @Cod_Proveedor",
                new { request.Cod_Producto, request.Cod_Proveedor });

            return CrearRespuestaNonQuery(result, "Ok", "Error al eliminar el proveedor del producto.");
        }

        #endregion

        #region Existencias

        /// <summary>
        /// Obtiene la lista de bodegas y las existencias del producto en cada una.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="data">Datos de consulta de existencias.</param>
        /// <returns>Listado de existencias por bodega.</returns>
        public ErrorDto<List<BodegaExistenciaProducto>> BodegaExistenciaProducto_Obtener(int CodEmpresa, BodegaExistenciaProducto data)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var lista = connection.Query<BodegaExistenciaProducto>("SELECT cod_bodega, descripcion FROM pv_bodegas").ToList();
                foreach (BodegaExistenciaProducto dt in lista)
                {
                    data.Cod_Bodega = dt.Cod_Bodega;
                    dt.Existencias = ObtenerExistencias(connection, data);
                }

                return lista;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<BodegaExistenciaProducto>())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener existencias por bodega.", result.Code.GetValueOrDefault(-1), new List<BodegaExistenciaProducto>());
        }

        /// <summary>
        /// Obtiene la existencia de un producto en una bodega.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="request">Datos de consulta.</param>
        /// <returns>Existencia calculada.</returns>
        private decimal ObtenerExistencias(IDbConnection connection, BodegaExistenciaProducto request)
        {
            try
            {
                string fechaCorte = MProGrXAuxiliarDB.validaFechaGlobal(request.Fecha_Corte, "yyyy-MM-dd") ?? string.Empty;
                return connection.QueryFirstOrDefault<decimal>(
                    "[spINVProcesoProd]",
                    new
                    {
                        CodProd = request.Cod_Producto,
                        Bodega = request.Cod_Bodega,
                        Fecha = fechaCorte,
                        Usuario = request.Usuario,
                        Muestra = 1,
                    },
                    commandType: CommandType.StoredProcedure);
            }
            catch
            {
                return 0;
            }
        }

        #endregion

        #region Barras

        /// <summary>
        /// Actualiza el código de barras del producto.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="codigoBarras">Código de barras.</param>
        /// <param name="Cod_Producto">Código del producto.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto BarrasProducto_Actualizar(int CodEmpresa, string codigoBarras, string Cod_Producto)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                "UPDATE pv_productos SET cod_barras = @codigoBarras WHERE cod_producto = @Cod_Producto",
                new { codigoBarras, Cod_Producto });

            return CrearRespuestaNonQuery(result, "Ok", "Error al actualizar el código de barras del producto.");
        }

        #endregion

        #region Movimientos

        /// <summary>
        /// Obtiene todos los movimientos de un producto.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Producto">Código del producto.</param>
        /// <returns>Listado de movimientos.</returns>
        public ErrorDto<List<MovimientoProducto>> MovimientosProducto_ObtenerTodos(int CodEmpresa, string Cod_Producto)
        {
            var result = DbHelper.ExecuteListQuery<MovimientoProducto>(
                CreatePortalDb(),
                CodEmpresa,
                "[spINV_W_Producto_Movimientos_Obtener]",
                new { producto = Cod_Producto });

            if (result.Code == 0 && result.Result is not null)
            {
                foreach (MovimientoProducto dt in result.Result)
                {
                    dt.TipoDesc = ObtenerDescripcionMovimiento(dt.Tipo);
                }
            }

            return CrearRespuestaLista(result, "Error al obtener los movimientos del producto.");
        }

        #endregion

        #region Similares

        /// <summary>
        /// Obtiene todos los productos similares.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Producto">Código del producto.</param>
        /// <returns>Listado de productos similares.</returns>
        public ErrorDto<List<SimilarProducto>> SimilaresProducto_ObtenerTodos(int CodEmpresa, string Cod_Producto)
        {
            var result = DbHelper.ExecuteListQuery<SimilarProducto>(
                CreatePortalDb(),
                CodEmpresa,
                @"select cod_producto as cod_producto_similar, descripcion, cabys
                  from pv_productos
                  where cod_producto <> @Cod_Prodclas
                    and similar in (select isnull(similar, 0) from pv_productos where cod_producto = @Cod_Prodclas)",
                CrearParametrosProducto(Cod_Producto));

            return CrearRespuestaLista(result, "Error al obtener los productos similares.");
        }

        /// <summary>
        /// Actualiza el producto similar.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos del producto similar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto SimilaresProducto_Actualizar(int CodEmpresa, SimilarProducto request)
        {
            return EjecutarProcedimientoConCodigo(
                CodEmpresa,
                "[spINV_W_ProductoSimilar_Editar]",
                new
                {
                    Cod_Producto = request.Cod_Producto,
                    Cod_Producto_Similar = request.Cod_Producto_Similar
                },
                "Error al actualizar el producto similar.");
        }

        /// <summary>
        /// Elimina el producto similar.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos del producto similar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto SimilaresProducto_Eliminar(int CodEmpresa, SimilarProducto request)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                "UPDATE pv_productos SET similar = null WHERE cod_producto = @Cod_Producto",
                new { Cod_Producto = request.Cod_Producto_Similar });

            return CrearRespuestaNonQuery(result, "Ok", "Error al eliminar el producto similar.");
        }

        #endregion

        #region UENS

        /// <summary>
        /// Obtiene la lista de UENS de un producto.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Producto">Código del producto.</param>
        /// <returns>Listado de UENS.</returns>
        public ErrorDto<List<UensProductos>> UensProducto_Obtener(int CodEmpresa, string Cod_Producto)
        {
            var result = DbHelper.ExecuteListQuery<UensProductos>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT U.COD_UNIDAD, U.DESCRIPCION, U.CNTX_UNIDAD, PU.REGISTRO_USUARIO,
                         CASE WHEN PU.REGISTRO_USUARIO IS NULL THEN 0 ELSE 1 END AS asignado
                  FROM CORE_UENS U
                  LEFT JOIN CPR_PRODUCTOS_UENS PU ON PU.COD_UNIDAD = U.COD_UNIDAD AND PU.COD_PRODUCTO = @Cod_Producto
                  LEFT JOIN PV_PRODUCTOS P ON P.COD_PRODUCTO = PU.COD_PRODUCTO",
                new { Cod_Producto });

            return CrearRespuestaLista(result, "Error al obtener las UENS del producto.");
        }

        /// <summary>
        /// Actualiza la asignación UENS del producto.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos de la UENS del producto.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto UensProducto_Actualizar(int CodEmpresa, UensProductos request)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                request.asignado
                    ? @"INSERT INTO [dbo].[CPR_PRODUCTOS_UENS] ([COD_PRODUCTO],[COD_UNIDAD],[REGISTRO_FECHA],[REGISTRO_USUARIO])
                        VALUES (@cod_producto, @cod_unidad, GetDate(), @registro_usuario)"
                    : @"DELETE FROM CPR_PRODUCTOS_UENS WHERE COD_PRODUCTO = @cod_producto AND COD_UNIDAD = @cod_unidad",
                new
                {
                    request.cod_producto,
                    request.cod_unidad,
                    request.registro_usuario
                });

            return CrearRespuestaNonQuery(result, "Ok", "Error al actualizar las UENS del producto.");
        }

        /// <summary>
        /// Obtiene la lista de tipos de activos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de tipos de activos.</returns>
        public ErrorDto<List<TipoActivoList>> TipoActivoLista_Obtener(int CodEmpresa)
        {
            var result = DbHelper.ExecuteListQuery<TipoActivoList>(
                CreatePortalDb(),
                CodEmpresa,
                "select TIPO_ACTIVO AS ITEM, DESCRIPCION from ACTIVOS_TIPO_ACTIVO");

            return CrearRespuestaLista(result, "Error al obtener la lista de tipos de activo.");
        }

        /// <summary>
        /// Elimina un producto.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="cod_producto">Código del producto.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto producto_Eliminar(int CodEmpresa, string cod_producto)
        {
            var result = DbHelper.WithConn<bool>(CreatePortalDb(), CodEmpresa, connection =>
            {
                connection.Execute("DELETE FROM PV_INVENTARIO_PROCESO WHERE COD_PRODUCTO = @cod_producto", new { cod_producto });
                connection.Execute("DELETE FROM PV_PRODUCTOS WHERE COD_PRODUCTO = @cod_producto", new { cod_producto });
                return true;
            });

            return result.Code == 0 && result.Result
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse("No se puede eliminar porque tiene registros contables asociados", -1);
        }

        /// <summary>
        /// Obtiene la bitácora de productos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="cod_producto">Código del producto.</param>
        /// <returns>Listado de bitácora del producto.</returns>
        public ErrorDto<List<BitacoraProductosDto>> BitacoraProducto_Obtener(int CodEmpresa, string cod_producto)
        {
            var result = DbHelper.ExecuteListQuery<BitacoraProductosDto>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT ID_BITACORA, CONSEC, REGISTRO_FECHA, COD_PRODUCTO, REGISTRO_USUARIO, DETALLE, MOVIMIENTO
                  FROM BITACORA_PRODUCTOS
                  WHERE cod_producto = @cod_producto
                  ORDER BY 1 ASC",
                new { cod_producto });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<BitacoraProductosDto>())
                : DbHelper.CreateErrorResponse("BitacoraBeneficioIntegral_Obtener: " + (result.Description ?? string.Empty), result.Code.GetValueOrDefault(-1), new List<BitacoraProductosDto>());
        }

        #endregion
    }
}