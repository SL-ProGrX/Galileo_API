using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.INV;
using System.Data;


namespace PgxAPI.DataBaseTier
{
    public class frmInvProductosDB
    {
        private readonly IConfiguration _config;
        private mProGrX_AuxiliarDB mAuxiliarDB;

        public frmInvProductosDB(IConfiguration config)
        {
            _config = config;
            mAuxiliarDB = new mProGrX_AuxiliarDB(_config);
        }


        #region Productos

        /// <summary>
        /// Consulta siguiente producto
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Cod_Producto"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto<Producto> ConsultaAscDesc(int CodEmpresa, string Cod_Producto, string tipo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto<Producto> info = new ErrorDto<Producto>();
            info.Result = new Producto();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = "";

                    if (tipo == "desc")
                    {
                        if (Cod_Producto == "0")
                        {
                            query = $@"select Top 1 cod_producto from pv_productos
                                    order by cod_producto desc";
                        }
                        else
                        {
                            query = $@"select Top 1 cod_producto from pv_productos
                                    where cod_producto < '{Cod_Producto}' order by cod_producto desc";
                        }

                    }
                    else
                    {
                        if (Cod_Producto == "0")
                        {
                            query = $@"select Top 1 cod_producto from pv_productos
                                    order by cod_producto asc";
                        }
                        else
                        {
                            query = $@"select Top 1 cod_producto from pv_productos
                                    where cod_producto > '{Cod_Producto}' order by cod_producto ";
                        }

                    }


                    info.Result = connection.Query<Producto>(query).FirstOrDefault();

                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
                info.Result = new Producto();
            }
            return info;
        }

        /// <summary>
        /// Obtiene la lista de productos para seleccion
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<ProductoDto>> Producto_ObtenerTodos(int CodEmpresa)
        {

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<List<ProductoDto>>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    // var query = "Select cod_producto,descripcion from pv_productos";
                    var query = "SELECT" +
                        " P.*,C.descripcion AS ProdClas,U.descripcion AS UnidadDesc,M.Descripcion AS MarcaDesc, " +
                         "isnull(Cs.Descripcion,'') AS 'LineaSub', isnull(P.COD_LINEA_SUB,'') AS 'LineaSubCod' " +
                         "FROM pv_productos P INNER JOIN pv_unidades U ON P.cod_unidad = U.cod_unidad " +
                         "INNER JOIN pv_prod_clasifica C ON P.cod_prodclas = C.cod_prodclas " +
                         "INNER JOIN pv_marcas M ON P.cod_marca = M.cod_marca " +
                         "INNER JOIN PV_PROD_CLASIFICA_SUB Cs ON P.cod_prodclas = Cs.cod_prodclas " +
                         "AND P.COD_LINEA_SUB = Cs.COD_LINEA_SUB";

                    response.Result = connection.Query<ProductoDto>(query).ToList();
                    foreach (ProductoDto dt in response.Result)
                    {
                        //dt.Activo = dt.Estado == "A" ? true : false;
                        dt.Inventario_Calculabool = dt.Inventario_Calcula == "S" ? true : false;
                        dt.Lotesbool = dt.Lotes == 1 ? true : false;

                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new List<ProductoDto>();
            }
            return response;
        }

        /// <summary>
        /// Obtiene el detalle del producto
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Cod_Producto"></param>
        /// <returns></returns>
        public ErrorDto<ProductoDto> Producto_ObtenerDetalle(int CodEmpresa, string Cod_Producto)
        {

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<ProductoDto>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"SELECT CONCAT(FORMAT(Cs.COD_PRODCLAS, ' 0') , 
                            FORMAT(ISNULL(Cs.NIVEL,'00'), ' 0'), 
                            FORMAT(ISNULL(Cs.COD_LINEA_SUB_MADRE,1) , ' 0'),
                            FORMAT(ISNULL(Cs.COD_LINEA_SUB,1) , ' ')
                            ) AS CODIGO,
                            P.*,C.descripcion AS ProdClass,U.descripcion AS UnidadDesc,M.Descripcion AS MarcaDesc, " +
                        "isnull(Cs.Descripcion,'') AS 'LineaSub', isnull(P.COD_LINEA_SUB,'') AS 'LineaSubCod' " +
                        "FROM pv_productos P INNER JOIN pv_unidades U ON P.cod_unidad = U.cod_unidad " +
                        "INNER JOIN pv_prod_clasifica C ON P.cod_prodclas = C.cod_prodclas " +
                        "INNER JOIN pv_marcas M ON P.cod_marca = M.cod_marca " +
                        "INNER JOIN PV_PROD_CLASIFICA_SUB Cs ON P.cod_prodclas = Cs.cod_prodclas " +
                        "AND P.COD_LINEA_SUB = Cs.COD_LINEA_SUB " +
                        "WHERE P.cod_producto = @Cod_Producto";

                    var parameters = new DynamicParameters();
                    parameters.Add("Cod_Producto", Cod_Producto, DbType.String);

                    response.Result = connection.Query<ProductoDto>(query, parameters).FirstOrDefault();

                    if (response.Result != null)
                    {
                        //response.Result.Activo = response.Result.Estado == "A" ? true : false;
                        response.Result.Inventario_Calculabool = response.Result.Inventario_Calcula == "S" ? true : false;
                        response.Result.Lotesbool = response.Result.Lotes == 1 ? true : false;
                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new ProductoDto();
            }
            return response;
        }

        /// <summary>
        /// Trae el código cabys a heredar en el producto
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Cod_Prodclas"></param>
        /// <param name="Cod_Linea_Sub"></param>
        /// <returns></returns>
        public ErrorDto<CabysHereda> Producto_ObtenerCabys(int CodEmpresa, int Cod_Prodclas, string Cod_Linea_Sub)
        {

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<CabysHereda>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "select Cabys from pv_prod_clasifica_Sub where COD_PRODCLAS = @Cod_Prodclas and COD_LINEA_SUB = @Cod_Linea_Sub";

                    var parameters = new DynamicParameters();
                    parameters.Add("Cod_Prodclas", Cod_Prodclas, DbType.Int32);
                    parameters.Add("Cod_Linea_Sub", Cod_Linea_Sub, DbType.String);

                    response.Result = connection.Query<CabysHereda>(query, parameters).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new CabysHereda();
            }
            return response;
        }

        /// <summary>
        /// Inserta un nuevo producto
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Producto_Insertar(int CodEmpresa, ProductoDto request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {

                

                using var connection = new SqlConnection(stringConn);
                {
                    if (request.Cod_Producto == "0")
                    {
                        var queryConsecutivo = $@"select FORMAT(NEXT VALUE FOR SeqPv_Productos, '000000')";
                        request.Cod_Producto = connection.Query<string>(queryConsecutivo).FirstOrDefault();
                    }

                    var procedure = "[spINV_W_Producto_Agregar]";
                    var values = new
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

                    };

                    resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    resp.Description = "Ok";

                    mAuxiliarDB.BitacoraProducto(new BitacoraProductoInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        cod_producto = request.Cod_Producto,
                        consec = 0,
                        movimiento = "Inserta",
                        detalle = $@"Se inserta el producto {request.Cod_Producto}",
                        registro_usuario = request.User_Modifica
                    });

                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;



        }
        /// <summary>
        /// Actualiza el producto
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Producto_Actualizar(int CodEmpresa, ProductoDto request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var procedure = "[spINV_W_Producto_Editar]";
                    var values = new
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
                    };

                    resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    resp.Description = "Ok";


                    if (request.justificacion_estado != "")
                    {

                        mAuxiliarDB.BitacoraProducto(new BitacoraProductoInsertarDto
                        {
                            EmpresaId = CodEmpresa,
                            cod_producto = request.Cod_Producto,
                            consec = 0,
                            movimiento = "Inserta",
                            detalle = $@"{request.justificacion_estado}",
                            registro_usuario = request.User_Modifica
                        });
                    }

                    mAuxiliarDB.BitacoraProducto(new BitacoraProductoInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        cod_producto = request.Cod_Producto,
                        consec = 0,
                        movimiento = "Actualiza",
                        detalle = $@"Se actualiza datos del producto {request.Cod_Producto}",
                        registro_usuario = request.User_Modifica
                    });

                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        #endregion


        #region Precios

        /// <summary>
        /// Obtiene todos los precios de los productos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Cod_Producto"></param>
        /// <returns></returns>
        public ErrorDto<List<PrecioProducto>> PreciosProducto_ObtenerTodos(int CodEmpresa, string Cod_Producto)
        {

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<PrecioProducto>>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = "select P.*,isnull(X.monto,0) as Monto,isnull(X.porc_utilidad,0) as Utilidad " +
                        "from pv_tipos_precios P left join pv_producto_precios X on P.cod_precio = X.cod_precio " +
                        "and X.cod_producto = @Cod_Prodclas order by P.defecto desc, X.monto";

                    var parameters = new DynamicParameters();
                    parameters.Add("Cod_Prodclas", Cod_Producto, DbType.String);


                    response.Result = connection.Query<PrecioProducto>(query, parameters).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new List<PrecioProducto>();
            }
            return response;
        }


        /// <summary>
        /// Agrega o actualiza el precio del producto
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto PrecioProducto_AgregarActualizar(int CodEmpresa, PrecioProducto request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var procedure = "[spINV_W_ProductoPrecio_AgregarEditar]";
                    var values = new
                    {
                        Cod_Producto = request.Cod_Producto,
                        Cod_Precio = request.Cod_Precio,
                        Monto = request.Monto,
                        Porc_Utilidad = request.Utilidad
                    };

                    resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    resp.Description = "Ok";

                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        #endregion


        #region Bonificaciones 

        /// <summary>
        /// Obtiene la bonificación de productos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Cod_Producto"></param>
        /// <returns></returns>
        public ErrorDto<List<BonificacionProducto>> BonificacionProducto_ObtenerTodos(int CodEmpresa, string Cod_Producto)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<BonificacionProducto>>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "select cod_producto,consec,desde,hasta,bonificacion from pv_producto_bonif where cod_producto = @Cod_Producto order by desde";

                    var parameters = new DynamicParameters();
                    parameters.Add("Cod_Producto", Cod_Producto, DbType.String);
                    response.Result = connection.Query<BonificacionProducto>(query, parameters).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new List<BonificacionProducto>();
            }
            return response;
        }

        
        /// <summary>
        /// Actualiza la bonificación del producto
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto BonificacionProducto_Actualizar(int CodEmpresa, BonificacionProducto request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var procedure = "[spINV_W_ProductoBonificacion_Editar]";
                    var values = new
                    {
                        Cod_Producto = request.Cod_Producto,
                        Consec = request.Consec,
                        Desde = request.Desde,
                        Hasta = request.Hasta,
                        Bonificacion = request.Bonificacion
                    };

                    resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    resp.Description = "Ok";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }


        /// <summary>
        /// Agrega una bonificación al producto
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto BonificacionProducto_Agregar(int CodEmpresa, BonificacionProducto request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var procedure = "[spINV_W_ProductoBonificacion_Agregar]";
                    var values = new
                    {
                        Cod_Producto = request.Cod_Producto,
                        Consec = request.Consec,
                        Desde = request.Desde,
                        Hasta = request.Hasta,
                        Bonificacion = request.Bonificacion
                    };

                    resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    resp.Description = "Ok";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }


        /// <summary>
        /// Elimina la bonificación del producto
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto BonificacionProducto_Eliminar(int CodEmpresa, BonificacionProducto request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "DELETE pv_producto_bonif WHERE consec = @Consec AND cod_producto = @Cod_Producto";

                    var parameters = new DynamicParameters();
                    parameters.Add("Cod_Producto", request.Cod_Producto, DbType.String);
                    parameters.Add("Consec", request.Consec, DbType.Int32);

                    resp.Code = connection.ExecuteAsync(query, parameters).Result;
                    resp.Description = "Ok";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        #endregion


        #region Descuentos


        /// <summary>
        /// Obtiene todos los descuentos de productos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Cod_Producto"></param>
        /// <returns></returns>
        public ErrorDto<List<DescuentoProducto>> DescuentoProducto_ObtenerTodos(int CodEmpresa, string Cod_Producto)
        {

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<DescuentoProducto>>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = "select cod_producto,consec,desde,hasta,porcentaje from pv_producto_desc where cod_producto = @Cod_Producto order by desde";

                    var parameters = new DynamicParameters();
                    parameters.Add("Cod_Producto", Cod_Producto, DbType.String);


                    response.Result = connection.Query<DescuentoProducto>(query, parameters).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new List<DescuentoProducto>();
            }
            return response;
        }


        /// <summary>
        /// Actualiza el descuento del producto
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto DescuentoProducto_Actualizar(int CodEmpresa, DescuentoProducto request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var procedure = "[spINV_W_ProductoDescuento_Editar]";
                    var values = new
                    {
                        Cod_Producto = request.Cod_Producto,
                        Consec = request.Consec,
                        Desde = request.Desde,
                        Hasta = request.Hasta,
                        Porcentaje = request.Porcentaje
                    };

                    resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    resp.Description = "Ok";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Agrega un descuento al producto
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto DescuentoProducto_Agregar(int CodEmpresa, DescuentoProducto request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var procedure = "[spINV_W_ProductoDescuento_Agregar]";
                    var values = new
                    {
                        Cod_Producto = request.Cod_Producto,
                        Consec = request.Consec,
                        Desde = request.Desde,
                        Hasta = request.Hasta,
                        Porcentaje = request.Porcentaje
                    };

                    resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    resp.Description = "Ok";

                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }


        /// <summary>
        /// Elimina el descuento del producto
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto DescuentoProducto_Eliminar(int CodEmpresa, DescuentoProducto request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "DELETE pv_producto_desc WHERE consec = @Consec AND cod_producto = @Cod_Producto";

                    var parameters = new DynamicParameters();
                    parameters.Add("Cod_Producto", request.Cod_Producto, DbType.String);
                    parameters.Add("Consec", request.Consec, DbType.Int32);

                    resp.Code = connection.ExecuteAsync(query, parameters).Result;
                    resp.Description = "Ok";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        #endregion


        #region Proveedores


        /// <summary>
        /// Obtiene la lista lazy de proveedores producto
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="producto"></param>
        /// <param name="pagina"></param>
        /// <param name="paginacion"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<ProvProductoDataLista> ProveedoresProducto_Obtener(int CodCliente, string producto, int? pagina, int? paginacion, string? filtro)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<ProvProductoDataLista>();
            response.Result = new ProvProductoDataLista();
            try
            {

                filtro = filtro == "undefined" ? null : filtro;

                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);
                {

                    //Busco Total
                    query = $@"Select COUNT(*) from CXP_PROVEEDORES";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    if (filtro != null)
                    {
                        filtro = " WHERE P.CEDJUR LIKE '%" + filtro + "%' OR UPPER(P.DESCRIPCION) LIKE '%" + filtro.ToUpper() + "%' ";
                    }

                    if (pagina != null)
                    {
                        paginaActual = " OFFSET " + pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
                    }

                    query = $@"SELECT COD_PROVEEDOR, DESCRIPCION, CEDJUR, FECHA_FACTURA, 
                                    CASE WHEN CodX IS NULL THEN 0
                                    ELSE CodX END
                                    AS CodX FROM (
		                        Select P.COD_PROVEEDOR, P.DESCRIPCION, P.CEDJUR, max(F.fecha) as 'FECHA_FACTURA',X.cod_proveedor as 'CodX' 
                                                    from CXP_PROVEEDORES P left join pv_producto_prov X on P.COD_PROVEEDOR = X.COD_PROVEEDOR and X.cod_producto = '{producto}'
                                                    left join vCxP_Facturas F on P.COD_PROVEEDOR = F.COD_PROVEEDOR 
                                                         {filtro} 
                                                         GROUP BY P.COD_PROVEEDOR, P.DESCRIPCION, P.CEDJUR,X.cod_proveedor  
                                                          ) T
								                           order by CodX DESC
                                                                 {paginaActual}
                                                             {paginacionActual}
                                        ";


                    response.Result.Lista = connection.Query<ProveedorProducto>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new ProvProductoDataLista();
            }
            return response;
        }

        /// <summary>
        /// Obtiene todos los proveedores de un producto
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Cod_Producto"></param>
        /// <returns></returns>
        public ErrorDto<List<ProveedorProducto>> ProveedorProducto_ObtenerTodos(int CodEmpresa, string Cod_Producto)
        {

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<ProveedorProducto>>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = "Select P.COD_PROVEEDOR, P.DESCRIPCION, P.CEDJUR, max(F.fecha) as 'FECHA_FACTURA',X.cod_proveedor as 'CodX' " +
                        "from CXP_PROVEEDORES P left join pv_producto_prov X on P.COD_PROVEEDOR = X.COD_PROVEEDOR and X.cod_producto = @Cod_Producto " +
                        "left join vCxP_Facturas F on P.COD_PROVEEDOR = F.COD_PROVEEDOR GROUP BY P.COD_PROVEEDOR, P.DESCRIPCION, P.CEDJUR,X.cod_proveedor " +
                        "order by X.cod_proveedor desc, P.Descripcion";

                    var parameters = new DynamicParameters();
                    parameters.Add("Cod_Producto", Cod_Producto, DbType.String);

                    response.Result = connection.Query<ProveedorProducto>(query, parameters).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new List<ProveedorProducto>();
            }
            return response;
        }


        /// <summary>
        /// Inserta un nuevo proveedor para el producto
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto ProveedorProducto_Insertar(int CodEmpresa, ProveedorProducto request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "insert into pv_producto_prov(cod_producto, cod_proveedor)values(@Cod_Producto, @Cod_Proveedor)";

                    var parameters = new DynamicParameters();
                    parameters.Add("Cod_Producto", request.Cod_Producto, DbType.String);
                    parameters.Add("Cod_Proveedor", request.Cod_Proveedor, DbType.Int32);

                    resp.Code = connection.ExecuteAsync(query, parameters).Result;
                    resp.Description = "Ok";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }


        /// <summary>
        /// Elimina un proveedor del producto
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto ProveedorProducto_Eliminar(int CodEmpresa, ProveedorProducto request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "DELETE pv_producto_prov where cod_producto = @Cod_Producto and cod_proveedor = @Cod_Proveedor";

                    var parameters = new DynamicParameters();
                    parameters.Add("Cod_Producto", request.Cod_Producto, DbType.String);
                    parameters.Add("Cod_Proveedor", request.Cod_Proveedor, DbType.Int32);

                    resp.Code = connection.ExecuteAsync(query, parameters).Result;
                    resp.Description = "Ok";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        #endregion


        #region Existencias

        /// <summary>
        /// Obtiene la lista de bodegas y las existencias de cada una
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto<List<BodegaExistenciaProducto>> BodegaExistenciaProducto_Obtener(int CodEmpresa, BodegaExistenciaProducto data)
        {

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<BodegaExistenciaProducto>>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = "SELECT cod_bodega, descripcion FROM pv_bodegas";

                    //var parameters = new DynamicParameters();
                    //parameters.Add("Cod_Producto", Cod_Producto, DbType.String);

                    response.Result = connection.Query<BodegaExistenciaProducto>(query).ToList();
                    foreach (BodegaExistenciaProducto dt in response.Result)
                    {
                        data.Cod_Bodega = dt.Cod_Bodega;
                        dt.Existencias = ObtenerExistencias(CodEmpresa, data);

                    }


                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new List<BodegaExistenciaProducto>();
            }
            return response;
        }

        /// <summary>
        /// Obtiene la existencia de un producto en una bodega
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private decimal ObtenerExistencias(int CodEmpresa, BodegaExistenciaProducto request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            decimal info = 0;
            try
            {

                string Fecha_Corte = mAuxiliarDB.validaFechaGlobal(request.Fecha_Corte);

                using var connection = new SqlConnection(stringConn);
                {
                    var procedure = "[spINVProcesoProd]";

                    var values = new
                    {
                        CodProd = request.Cod_Producto,
                        Bodega = request.Cod_Bodega,
                        Fecha = Fecha_Corte,
                        Usuario = request.Usuario,
                        Muestra = 1,
                    };

                    info = connection.Query<decimal>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                info = 0;
            }
            return info;
        }

        #endregion


        #region Barras

       /// <summary>
       /// Actualiza el código de barras del producto
       /// </summary>
       /// <param name="CodEmpresa"></param>
       /// <param name="codigoBarras"></param>
       /// <param name="Cod_Producto"></param>
       /// <returns></returns>
        public ErrorDto BarrasProducto_Actualizar(int CodEmpresa, string codigoBarras, string Cod_Producto)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "UPDATE pv_productos SET cod_barras = @codigoBarras WHERE cod_producto = @Cod_Producto";

                    var parameters = new DynamicParameters();
                    parameters.Add("codigoBarras", codigoBarras, DbType.String);
                    parameters.Add("Cod_Producto", Cod_Producto, DbType.String);

                    resp.Code = connection.ExecuteAsync(query, parameters).Result;
                    resp.Description = "Ok";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        #endregion


        #region Movimientos

        /// <summary>
        /// Obtiene todos los movimientos de un producto
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Cod_Producto"></param>
        /// <returns></returns>
        public ErrorDto<List<MovimientoProducto>> MovimientosProducto_ObtenerTodos(int CodEmpresa, string Cod_Producto)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<MovimientoProducto>>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var procedure = "[spINV_W_Producto_Movimientos_Obtener]";

                    var values = new
                    {
                        producto = Cod_Producto,
                    };
                    response.Result = connection.Query<MovimientoProducto>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                    foreach (MovimientoProducto dt in response.Result)
                    {
                        switch (dt.Tipo)
                        {
                            case "E":
                                dt.TipoDesc = "ENTRADA";
                                break;
                            case "S":
                                dt.TipoDesc = "SALIDA";
                                break;
                            case "T":
                                dt.TipoDesc = "TRASLADO";
                                break;
                            default:
                                dt.TipoDesc = "N/A";
                                break;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new List<MovimientoProducto>();
            }
            return response;
        }

        #endregion


        #region Similares


        /// <summary>
        /// Obtiene todos los productos similares
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Cod_Producto"></param>
        /// <returns></returns>
        public ErrorDto<List<SimilarProducto>> SimilaresProducto_ObtenerTodos(int CodEmpresa, string Cod_Producto)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<SimilarProducto>>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = "select cod_producto as cod_producto_similar,descripcion,cabys from pv_productos " +
                        "where cod_producto<> @Cod_Prodclas and similar in(select isnull(similar, 0) " +
                        "from pv_productos where cod_producto = @Cod_Prodclas)";

                    var parameters = new DynamicParameters();
                    parameters.Add("Cod_Prodclas", Cod_Producto, DbType.String);

                    response.Result = connection.Query<SimilarProducto>(query, parameters).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new List<SimilarProducto>();
            }
            return response;
        }


        /// <summary>
        /// Actualiza el producto similar
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto SimilaresProducto_Actualizar(int CodEmpresa, SimilarProducto request)

        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var procedure = "[spINV_W_ProductoSimilar_Editar]";
                    var values = new
                    {
                        Cod_Producto = request.Cod_Producto,
                        Cod_Producto_Similar = request.Cod_Producto_Similar

                    };

                    resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    resp.Description = "Ok";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }


       /// <summary>
       /// Elimina el producto similar
       /// </summary>
       /// <param name="CodEmpresa"></param>
       /// <param name="request"></param>
       /// <returns></returns>
        public ErrorDto SimilaresProducto_Eliminar(int CodEmpresa, SimilarProducto request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "UPDATE pv_productos SET similar = null WHERE cod_producto = @Cod_Producto";

                    var parameters = new DynamicParameters();
                    parameters.Add("Cod_Producto", request.Cod_Producto_Similar, DbType.String);

                    resp.Code = connection.ExecuteAsync(query, parameters).Result;
                    resp.Description = "Ok";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        #endregion


        #region UENS

        /// <summary>
        /// Obtiene la lista de UENS de un producto
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Cod_Producto"></param>
        /// <returns></returns>
        public ErrorDto<List<UensProductos>> UensProducto_Obtener(int CodEmpresa, string Cod_Producto)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<UensProductos>>();
            response.Result = new List<UensProductos>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"SELECT U.COD_UNIDAD, U.DESCRIPCION, U.CNTX_UNIDAD, PU.REGISTRO_USUARIO,
                                      CASE 
                                      WHEN PU.REGISTRO_USUARIO IS NULL THEN 0
                                      ELSE 1
                                      END AS asignado
                                      FROM CORE_UENS U LEFT JOIN CPR_PRODUCTOS_UENS PU
                                      ON PU.COD_UNIDAD = U.COD_UNIDAD AND PU.COD_PRODUCTO = '{Cod_Producto}'
                                      LEFT JOIN PV_PRODUCTOS P ON P.COD_PRODUCTO = PU.COD_PRODUCTO
                                      ";

                    response.Result = connection.Query<UensProductos>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Actualiza el producto UENS
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto UensProducto_Actualizar(int CodEmpresa, UensProductos request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    if (request.asignado == true)
                    {
                        var insert = $@"INSERT INTO [dbo].[CPR_PRODUCTOS_UENS]
                                               ([COD_PRODUCTO]
                                               ,[COD_UNIDAD]
                                               ,[REGISTRO_FECHA]
                                               ,[REGISTRO_USUARIO])
                                         VALUES
                                               ('{request.cod_producto}'
                                               ,'{request.cod_unidad}'
                                               ,GetDate()
                                               ,'{request.registro_usuario}')";

                        resp.Code = connection.Execute(insert);
                    }
                    else
                    {
                        var delete = $@"DELETE FROM CPR_PRODUCTOS_UENS WHERE COD_PRODUCTO = '{request.cod_producto}' AND COD_UNIDAD = '{request.cod_unidad}'";
                        resp.Code = connection.Execute(delete);
                    }

                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }


        /// <summary>
        /// Obtiene la lista de tipos de activos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<TipoActivoList>> TipoActivoLista_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<TipoActivoList>>();
            response.Result = new List<TipoActivoList>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select TIPO_ACTIVO AS ITEM, DESCRIPCION from ACTIVOS_TIPO_ACTIVO";

                    response.Result = connection.Query<TipoActivoList>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }


        /// <summary>
        /// Elimina un producto
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_producto"></param>
        /// <returns></returns>
        public ErrorDto producto_Eliminar(int CodEmpresa, string cod_producto)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);

                {
                    var queryInventarioProceso = $@"DELETE FROM PV_INVENTARIO_PROCESO WHERE COD_PRODUCTO = '{cod_producto}' ";
                    resp.Code = connection.Query<int>(queryInventarioProceso).FirstOrDefault();
                    resp.Description = "Ok";


                    var query = $@"DELETE FROM PV_PRODUCTOS WHERE COD_PRODUCTO = '{cod_producto}' ";
                    resp.Code = connection.Query<int>(query).FirstOrDefault();
                    resp.Description = "Ok";

                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "No se puede eliminar porque tiene registros contables asociados";
            }
            return resp;
        }

        /// <summary>
        /// Obtiene la bitácora de productos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_producto"></param>
        /// <returns></returns>
        public ErrorDto<List<BitacoraProductosDto>> BitacoraProducto_Obtener(int CodEmpresa, string cod_producto)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<BitacoraProductosDto>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = $@"SELECT ID_BITACORA, CONSEC, REGISTRO_FECHA, COD_PRODUCTO,REGISTRO_USUARIO, DETALLE, MOVIMIENTO
                  FROM BITACORA_PRODUCTOS WHERE cod_producto = '{cod_producto}' ORDER BY 1 ASC";

                    response.Result = connection.Query<BitacoraProductosDto>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "BitacoraBeneficioIntegral_Obtener: " + ex.Message;
                response.Result = null;
            }
            return response;
        }


        #endregion
    }
}
