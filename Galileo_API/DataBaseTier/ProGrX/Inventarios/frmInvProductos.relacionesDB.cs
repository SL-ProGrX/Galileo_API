using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using System.Data;
using System.Text;

namespace Galileo.DataBaseTier
{
    public partial class FrmInvProductosDB
    {
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

