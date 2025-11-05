using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.BusinessLogic;
using PgxAPI.Models;
using PgxAPI.Models.CPR;
using PgxAPI.Models.ERROR;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmCprComprasOrdenDB
    {
        private readonly IConfiguration _config;

        mProGrX_AuxiliarDB mProGrxAuxiliar;
        mSecurityMainDb DBBitacora;
        mComprasDB mComprasDB;
        public string sendEmail = "";
        public string TestMail = "";
        public string Notificaciones = "";
        public string EmailProveedor = "";
        private readonly EnvioCorreoDB _envioCorreoDB;
        public string Proveedor = "";

        public frmCprComprasOrdenDB(IConfiguration config)
        {
            _config = config;
            DBBitacora = new mSecurityMainDb(config);
            mProGrxAuxiliar = new mProGrX_AuxiliarDB(config);
            mComprasDB = new mComprasDB(config);
            _envioCorreoDB = new EnvioCorreoDB(_config);
            sendEmail = _config.GetSection("AppSettings").GetSection("EnviaEmail").Value.ToString();
            Notificaciones = _config.GetSection("AppSettings").GetSection("Notificaciones").Value.ToString();
        }

        public ErrorDto Bitacora(BitacoraInsertarDTO data)
        {
            return DBBitacora.Bitacora(data);
        }

        /// <summary>
        /// Ordenes Obtener
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodOrden"></param>
        /// <returns></returns>
        public ErrorDto<OrdenCompraSinFacturaData> Orden_Obtener(int CodEmpresa, string CodOrden)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<OrdenCompraSinFacturaData>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select O.*,rtrim(C.tipo_orden) as 'Causa_ID', Rtrim(C.descripcion) as 'Causa_Desc', Prov.Descripcion as 'Proveedor'
                               ,  RIGHT(REPLICATE('0', 10) + CAST(sp.CPR_ID AS VARCHAR), 10) AS no_solicitud
                                        from cpr_ordenes O inner join cpr_Tipo_Orden C on O.tipo_orden = C.tipo_orden
                                         inner join CxP_Proveedores Prov on O.cod_Proveedor = Prov.cod_proveedor
                                  left join CPR_SOLICITUD_PROV sp ON sp.ADJUDICA_ORDEN  = O.COD_ORDEN AND sp.PROVEEDOR_CODIGO = O.COD_PROVEEDOR 
                                        where
	                                	O.cod_orden = '{CodOrden}'  and 
		                                O.estado = 'A' and O.Proceso in('A','D','X')";
                    response.Result = connection.Query<OrdenCompraSinFacturaData>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        
        /// <summary>
        /// Facturas obtener
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodOrden"></param>
        /// <returns></returns>
        public ErrorDto<OrdenCompraFacturaData> OrdenFactura_Obtener(int CodEmpresa, string CodOrden)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<OrdenCompraFacturaData>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"SELECT E.*, 
                                           (RTRIM(C.Tipo_Orden) + ' - ' + C.descripcion) AS Causa, 
                                           P.descripcion AS Proveedor,
                                           O.nota, 
                                           E.notas AS CompraNotas, 
                                           RIGHT(REPLICATE('0', 10) + CAST(sp.CPR_ID AS VARCHAR), 10) AS no_solicitud
                                    FROM cpr_ordenes O 
                                    INNER JOIN cpr_Tipo_Orden C ON O.Tipo_Orden = C.Tipo_Orden
                                    INNER JOIN cpr_Compras E ON O.cod_orden = E.cod_orden
                                    left join CPR_SOLICITUD_PROV sp ON sp.ADJUDICA_ORDEN  = O.COD_ORDEN AND sp.PROVEEDOR_CODIGO = O.COD_PROVEEDOR 
                                    INNER JOIN cxp_proveedores P ON E.cod_proveedor = P.cod_proveedor
                                    where E.cod_compra = '{CodOrden}'";
                    response.Result = connection.Query<OrdenCompraFacturaData>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        /// <summary>
        /// Ordenes Detalle Factura Obtener
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<CompraOrdenLineasData> OrdenesDetalleF_Obtener(int CodEmpresa, CompraOrderLineaTablaFiltros filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<CompraOrdenLineasData>();
            response.Result = new CompraOrdenLineasData();

            string paginaActual = " ", paginacionActual = " ";
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var qTotal = $@"select Count(D.cod_producto)
                                          from cpr_Compras_detalle D inner join pv_productos P on D.cod_producto = P.cod_producto
                                          where D.cod_factura = '{filtros.CodOrden}' and D.cod_proveedor = {filtros.CodProveedor}";

                    response.Result.total = connection.Query<int>(qTotal).FirstOrDefault();

                    var qCantidad = $@"select isnull(sum(D.cantidad),0)  from cpr_Compras_detalle D inner join pv_productos P on D.cod_producto = P.cod_producto
                                          where D.cod_factura = '{filtros.CodOrden}' and D.cod_proveedor = {filtros.CodProveedor}";

                    response.Result.cantidad = connection.Query<long>(qCantidad).FirstOrDefault();

                    string vFiltro = "";
                    //if (filtros.filtro.ToString() != "")
                    //{
                    //    vFiltro = " Where cod_producto LIKE '%" + filtros.filtro + "%' OR descripcion LIKE '%" + filtros.filtro + "%' ";
                    //}

                    //if (filtros.pagina != null)
                    //{
                    //    paginaActual = " OFFSET " + filtros.pagina + " ROWS ";
                    //    paginacionActual = " FETCH NEXT " + filtros.paginacion + " ROWS ONLY ";
                    //}

                    var query = $@"select D.cod_producto,P.descripcion,
    P.COD_UNIDAD as unidad,
     od.CANTIDAD as 'qtyOrg',
    0 as 'qtyPend',
    D.cantidad,D.cod_bodega,D.precio,isnull(D.descuento,0) as descuento,D.imp_ventas,0 as Total, null as tipoProd ,ppc.DESCRIPCION as familia
                                          from cpr_Compras_detalle D inner join pv_productos P on D.cod_producto = P.cod_producto
                                          left join CPR_COMPRAS C ON C.COD_FACTURA = D.COD_FACTURA 
                                          LEFT join cpr_ordenes_detalle od ON od.COD_ORDEN = C.COD_ORDEN  AND od.COD_PRODUCTO = D.COD_PRODUCTO
                                        LEFT JOIN PV_PROD_CLASIFICA ppc ON ppc.COD_PRODCLAS = P.COD_PRODCLAS                                          
                                        where D.cod_factura = '{filtros.CodOrden}' and D.cod_proveedor = {filtros.CodProveedor}
                                          order by D.cod_producto";
                    response.Result.lineas = connection.Query<OrdenCompraDetalleData>(query).ToList();
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
        /// Ordenes Detalle Ordenes Obtener
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<CompraOrdenLineasData> OrdenesDetalleO_Obtener(int CodEmpresa, CompraOrderLineaTablaFiltros filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<CompraOrdenLineasData>();
            response.Result = new CompraOrdenLineasData();
            string paginaActual = " ", paginacionActual = " ";
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var qTotal = $@"select count(D.cod_producto) from cpr_ordenes_detalle 
                                        D inner join pv_productos P on D.cod_producto = P.cod_producto
                                        where D.cod_orden = '{filtros.CodOrden}' ";

                    response.Result.total = connection.Query<int>(qTotal).FirstOrDefault();

                    var qCantidad = $@"select isnull(sum(D.cantidad),0) from cpr_ordenes_detalle 
                                        D inner join pv_productos P on D.cod_producto = P.cod_producto
                                        where D.cod_orden = '{filtros.CodOrden}' ";

                    response.Result.cantidad = connection.Query<long>(qCantidad).FirstOrDefault();

                    string vFiltro = "";
                    if (filtros.filtro.ToString() != "")
                    {
                        vFiltro = " Where cod_producto LIKE '%" + filtros.filtro + "%' OR descripcion LIKE '%" + filtros.filtro + "%' ";
                    }

                    if (filtros.pagina != null)
                    {
                        paginaActual = " OFFSET " + filtros.pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + filtros.paginacion + " ROWS ONLY ";
                    }

                    var query = $@"SELECT 
                                        cod_producto,
                                        descripcion,
                                        unidad,
                                        qtyOrg,
                                        qtyPend,
                                        Cantidad,
                                        cod_bodega,
                                        precio,
                                        Descuento,
                                        imp_ventas,
                                        Total,
                                        tipoProd,
                                        familia
                                    FROM (
                                        SELECT 
                                            D.cod_producto,
                                            P.descripcion,
                                            P.COD_UNIDAD AS unidad,
                                            D.cantidad AS qtyOrg,
                                            (D.cantidad - SUM(ISNULL(ccd.cantidad, 0))) AS qtyPend,
                                            (D.cantidad - SUM(ISNULL(ccd.cantidad, 0))) AS Cantidad,
                                            '' AS cod_bodega,
                                            D.precio,
                                            ISNULL(D.descuento, 0) AS Descuento,
                                            D.imp_ventas,
                                            0 AS Total,
                                            P.TIPO_PRODUCTO as tipoProd,
                                            ppc.DESCRIPCION as familia
                                        FROM cpr_ordenes_detalle D
                                        INNER JOIN pv_productos P ON D.cod_producto = P.cod_producto
                                        LEFT JOIN CPR_COMPRAS cc ON cc.COD_ORDEN = D.cod_orden 
                                        LEFT JOIN cpr_compras_detalle ccd ON ccd.cod_factura = cc.cod_factura
                                            AND ccd.cod_producto = D.cod_producto
                                        LEFT JOIN PV_PROD_CLASIFICA ppc ON ppc.COD_PRODCLAS = P.COD_PRODCLAS
                                   where D.cod_orden = '{filtros.CodOrden}'
                                         GROUP BY 
                                            D.cod_producto, P.descripcion, P.COD_UNIDAD, D.cantidad,
                                            D.precio, D.descuento, D.imp_ventas, P.TIPO_PRODUCTO, ppc.DESCRIPCION
                                        ) T  ORDER BY cod_producto
                                        {paginaActual}
                                        {paginacionActual}
                                      {vFiltro}";

                    response.Result.lineas = connection.Query<OrdenCompraDetalleData>(query).ToList();
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
        /// Orden Costo Actualizar
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodCompra"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        private ErrorDto OrdenCosto_Actualiza(int CodEmpresa, string CodCompra, string Usuario)
        {
            ErrorDto result = new ErrorDto();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //var procedure = "spCRPActualizaCts";
                    var procedure = "spCRP_W_CostosArticulos_Actualizar";
                    var parameters = new
                    {
                        @Boleta = CodCompra,
                        @Usuario = Usuario,
                    };

                    connection.Execute(procedure, parameters, commandType: CommandType.StoredProcedure);
                    result.Code = 0;
                    result.Description = "ok";
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result;
        }

        //Nuevos Metodos para la tabla de Ordenes

        /// <summary>
        /// Compras Orden Guardar
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="orden"></param>
        /// <returns></returns>
        public ErrorDto ComprasOrden_Guardar(int CodEmpresa, ComprasOrdenDatos orden)
        {
            ErrorDto result = new ErrorDto();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            result.Code = 0;
            string mensaje = "";

            if (orden.cod_factura == "")
            {
                result.Code = -1;
                result.Description = "El campo Factura no puede ser nulo";
                return result;
            }

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    fxValida(CodEmpresa, orden, ref mensaje);
                    if (mensaje.Length == 0)
                    {
                        if (orden.pin.Length == 0)
                        {
                            var qPin = $@"select pin_autorizacion from cpr_ordenes where cod_orden = '{orden.cod_orden}'";
                            int pin = connection.Query<int>(qPin).FirstOrDefault();

                            if (pin == 1)
                            {
                                result.Code = 2;
                                result.Description = mensaje;
                                return result;
                            }
                        }
                        else
                        {
                            //Valido Pin
                            var qPinVerifica = $@"select isnull(count(*),0) as Existe from cpr_ordenes 
                                          where cod_orden = '{orden.cod_orden}'  and pin_entrada = '{orden.pin}' ";
                            int pinVerifica = connection.Query<int>(qPinVerifica).FirstOrDefault();

                            if (pinVerifica == 0)
                            {
                                result.Code = -1;
                                result.Description = "El Pin de Compra suministrado no es correcto...";
                                return result;
                            }
                        }

                        //Consecutivo de la Compra
                        var qConsecutivo = $@"select isnull(max(cod_compra),0) + 1 as Ultimo from cpr_compras";
                        string consecutivo = connection.Query<string>(qConsecutivo).FirstOrDefault();
                        consecutivo = consecutivo.PadLeft(10, '0');
                        consecutivo = consecutivo.ToString();
                        //calcula totales 
                        float curSubTotal = 0, curDescuento = 0, curIV = 0, curCantidad = 0, curTotal = 0;
                        sbCalculaTotales(orden.lineas, ref curSubTotal, ref curDescuento, ref curIV, ref curCantidad, ref curTotal);

                        // Toma los primeros dos caracteres, conviértelos a mayúsculas y compáralos con "CR"
                        string tipo_pago = orden.tipo_pago == "CR" ? "P" : "G";

                        var query = $@"insert cpr_Compras (
                                    estado,
                                    cod_factura,
                                    forma_pago,
                                    cod_proveedor,
                                    cod_compra,
                                    cod_orden,
                                    genera_user,
                                    genera_fecha,
                                    fecha,
                                    sub_total,
                                    descuento,
                                    imp_ventas,
                                    imp_consumo,
                                    total,
                                    cxp_estado,
                                    asiento_estado,
                                    notas
                                    )values(
                                    'P',
                                    '{orden.cod_factura}',
                                    '{orden.forma_pago}',
                                     {orden.cod_proveedor} ,
                                     '{consecutivo}',
                                     '{orden.cod_orden}',
                                     '{orden.genera_user}',
                                     '{orden.fecha}',
                                     '{orden.fecha}',
                                     {orden.sub_total},
                                     {orden.descuento},
                                     {orden.imp_ventas},
                                     0,
                                     {orden.total},
                                     '{tipo_pago}',
                                     'P',
                                     '{orden.notas}');";
                        var insert = connection.Execute(query);

                        //Bitacora
                        Bitacora(new BitacoraInsertarDTO
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = orden.genera_user,
                            DetalleMovimiento = "Registra, Compra:" + consecutivo,
                            Movimiento = "Registra - WEB",
                            Modulo = 35
                        });

                        //Actualiza Saldo Proveedores / Si es A credito (CR)
                        if (tipo_pago == "CR")
                        {
                            var qsaldo = $@"update cxp_proveedores set saldo = isnull(saldo,0) + {curTotal} where cod_proveedor = {orden.cod_proveedor}";
                        }
                        else
                        {
                            //Registrar Pagos al Contado, como pagados
                            var queryPagoContado = $@"insert cxp_pagoProv(NPago,Cod_Proveedor,Cod_Factura,Fecha_Vencimiento,Monto,Frecuencia
                                     ,Tipo_Transac,User_TrasLada,Fecha_Traslada,Tesoreria,Pago_Tercero,Apl_Cargo_Flotante
                                     ,Pago_Anticipado,forma_pago, IMPORTE_DIVISA_REAL) values(1,{orden.cod_proveedor},'{orden.cod_factura}',Getdate(),
                                        {curTotal} ,0,0,'{orden.genera_user}',Getdate(),0,'',0,0,'CO', {curTotal})";
                            var resultPagoContado = connection.Execute(queryPagoContado);
                        }

                        //Guardar Detalle de la Orden
                        var qLimpia = $@"delete cpr_Compras_detalle  where cod_factura = '{orden.cod_factura}' and cod_proveedor = {orden.cod_proveedor} ";
                        var resultLimpia = connection.Execute(qLimpia);

                        int countLn = 0;
                        foreach (OrdenCompraDetalleData item in orden.lineas)
                        {
                            countLn++;
                            //Inserta linea
                            query = $@"insert cpr_Compras_detalle(
                                    linea,
                                    cod_factura,
                                    cod_proveedor,
                                    cod_producto,
                                    cantidad,
                                    cod_bodega,precio,descuento,imp_ventas,imp_consumo) values(
                                     {countLn},
                                    '{orden.cod_factura}',
                                    {orden.cod_proveedor},
                                    '{item.cod_producto}',
                                    {item.cantidad},
                                    '{item.cod_bodega}',
                                    {item.precio},
                                    {item.descuento},
                                    {item.imp_ventas},
                                    0 ) ";
                            var insertLinea = connection.Execute(query);

                            if (insertLinea == 1)
                            {
                                //Actualiza Inventario
                                query = $@"update cpr_ordenes_detalle set cantidad_despachada = isnull(cantidad_despachada,0) 
                                        where cod_producto = '{item.cod_producto}' and cod_orden = '{orden.cod_orden}' ";
                                var update = connection.Execute(query);
                            }

                            var artActivos = BuscoArticulosActivos(
                                     CodEmpresa,
                                     item,
                                     consecutivo,
                                     orden.cod_factura,
                                     orden.genera_user,
                                     orden.cod_orden,
                                     orden.cod_proveedor);

                            //LLAMO mProGrX_AuxiliarDB para actualizar el inventario
                            mProGrX_AuxiliarDB mProGrX_AuxiliarDB = new mProGrX_AuxiliarDB(_config);
                            CompraInventarioDTO compraInventario = new CompraInventarioDTO();
                            compraInventario.CodProducto = item.cod_producto;
                            compraInventario.Cantidad = item.cantidad;
                            compraInventario.CodBodega = item.cod_bodega;
                            compraInventario.CodTipo = orden.cod_orden;
                            compraInventario.Origen = "Compra";
                            compraInventario.Fecha = DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss");
                            compraInventario.Precio = decimal.Parse(item.precio.ToString());
                            compraInventario.ImpVentas = decimal.Parse(item.imp_ventas.ToString());
                            compraInventario.ImpConsumo = 0;
                            compraInventario.TipoMov = "E";
                            mProGrX_AuxiliarDB.sbInvInventario(CodEmpresa, compraInventario);

                        }

                        //Indica si la Orden fue Total/Parcialmente despachada
                        mComprasDB.sbCprOrdenesDespacho(CodEmpresa, orden.cod_orden);

                        //Actualiza Costo de Articulos
                        OrdenCosto_Actualiza(CodEmpresa, consecutivo, orden.genera_user);

                        mComprasDB.FacturaOrdenes_Actualizar(CodEmpresa, orden.cod_factura, orden.cod_proveedor);

                        result.Description = consecutivo;

                        //Envio de Correo

                        CorreoNotificaRegistraFactura_Enviar(CodEmpresa, orden.factura, orden.genera_user,orden.cod_proveedor);

                    }
                    else
                    {
                        result.Code = -1;
                        result.Description = mensaje;
                    }
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }


        /// <summary>
        /// Funcion Valida
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="orden"></param>
        /// <param name="mensaje"></param>
        /// <returns></returns>
        private bool fxValida(int CodEmpresa, ComprasOrdenDatos orden, ref string mensaje)
        {
            bool result = true;
            try
            {
                if (orden.factura == null)
                {
                    mensaje = " - Factura no puede ser nulo";
                    result = false;
                }
                else
                {
                    if (orden.factura.Length == 0)
                    {
                        mensaje = " - Factura no puede ser vacio";
                        result = false;
                    }
                }

                var factura = fxVerificaNumeroFactura(CodEmpresa, orden.cod_factura, orden.cod_proveedor);
                if (factura != null) { 
                    
                    if(factura.Code == -1)
                    {
                        mensaje += factura.Description;
                    }

                }

                mensaje += fxInvVerificaLineaDetalle(CodEmpresa, orden.lineas, "E");
                mensaje += fxVerificaTotalesFac(CodEmpresa, orden).Description;

                if (mProGrxAuxiliar.fxInvPeriodos(CodEmpresa, orden.fecha) == false)
                {
                    mensaje += " - El periodo en el que desea realizar el movimiento se encuentra cerrado ...";
                }

                if (mensaje.Length > 0)
                {
                    result = false;
                }

            }
            catch (Exception ex)
            {
                mensaje = ex.Message;
                result = false;
            }
            return result;
        }


       /// <summary>
       /// Funcion Verifica Linea Detalle
       /// </summary>
       /// <param name="CodEmpresa"></param>
       /// <param name="vGrid"></param>
       /// <param name="vMov"></param>
       /// <returns></returns>
        private string fxInvVerificaLineaDetalle(int CodEmpresa, List<OrdenCompraDetalleData> vGrid,
          string vMov)
        {
            string mensaje = "";
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    if (vGrid.Count == 0)
                    {
                        return "No hay productos en la orden";
                    }

                    foreach (OrdenCompraDetalleData item in vGrid)
                    {
                        //Verifica que el producto este activo
                        if (item.cantidad > 0)
                        {
                            var query = $@"select estado from pv_productos where cod_producto = '{item.cod_producto}'";
                            var resp = connection.Query<string>(query).FirstOrDefault();
                            if (resp == null)
                            {
                                return "El producto " + item.cod_producto + " no existe";
                            }
                            if (resp == "I")
                            {
                                return "El producto " + item.cod_producto + " no esta activo";
                            }
                        }

                        //Verifica que la Bodega Exista y que Permita Registrar el Tipo de Movimiento
                        if (item.cod_bodega.Length > 0)
                        {
                            var query = $@"select permite_entradas,permite_salidas,estado from pv_bodegas where cod_bodega = '{item.cod_bodega}'";
                            List<BodegaDTO> exist = connection.Query<BodegaDTO>(query).ToList();
                            if (exist.Count == 0)
                            {
                                return "La bodega " + item.cod_bodega + " - No existe";
                            }
                            else
                            {
                                if (exist[0].estado == "I")
                                {
                                    return "La bodega " + item.cod_bodega + " - Se encuentra Inactiva";
                                }
                                else
                                {
                                    switch (vMov)
                                    {
                                        case "E":
                                            if (exist[0].permite_entradas != "1")
                                            {
                                                return "La bodega " + item.cod_bodega + " - No permite Entradas";
                                            }
                                            break;
                                        case "S":
                                        case "R":
                                        case "T":
                                            if (exist[0].permite_salidas != "1")
                                            {
                                                return "La bodega " + item.cod_bodega + " - No permite Salidas";
                                            }
                                            break;
                                    }
                                }
                            }
                        }

                        if (item.cod_bodega.Length > 0)
                        {
                            var query = $@"select permite_entradas,permite_salidas,estado from pv_bodegas where cod_bodega = '{item.cod_bodega}'";
                            List<BodegaDTO> exist = connection.Query<BodegaDTO>(query).ToList();
                            if (exist.Count == 0)
                            {
                                return "La bodega " + item.cod_bodega + " - No existe";
                            }
                            else
                            {
                                if (exist[0].estado == "I")
                                {
                                    return "La bodega " + item.cod_bodega + " - Se encuentra Inactiva";
                                }
                                else
                                {
                                    switch (vMov)
                                    {
                                        case "E":
                                        case "T":
                                            if (exist[0].permite_entradas != "1")
                                            {
                                                return "La bodega " + item.cod_bodega + " - No permite Entradas";
                                            }
                                            break;
                                        case "S":
                                        case "R":
                                            if (exist[0].permite_salidas != "1")
                                            {
                                                return "La bodega " + item.cod_bodega + " - No permite Salidas";
                                            }
                                            break;
                                    }
                                }
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }

            return mensaje;
        }


        /// <summary>
        /// Calcula totales
        /// </summary>
        /// <param name="vGrid"></param>
        /// <param name="curSubTotal"></param>
        /// <param name="curDescuento"></param>
        /// <param name="curIV"></param>
        /// <param name="curCantidad"></param>
        /// <param name="curTotal"></param>
        /// <returns></returns>
        private List<OrdenCompraDetalleData> sbCalculaTotales(List<OrdenCompraDetalleData> vGrid
            , ref float curSubTotal, ref float curDescuento, ref float curIV, ref float curCantidad,
            ref float curTotal)
        {
            foreach (OrdenCompraDetalleData item in vGrid)
            {
                curSubTotal = curSubTotal + (item.cantidad * item.precio);
                float curTmpDesc = ((item.cantidad * item.precio) * (item.descuento / 100));
                curDescuento = curDescuento + curTmpDesc;

                float curTmpIV = (((item.cantidad * item.precio) - curTmpDesc) * (item.imp_ventas / 100));
                curIV = curIV + curTmpIV;

                item.total = (item.cantidad * item.precio) - curTmpDesc + curTmpIV;
                curCantidad = curCantidad + item.cantidad;
            }
            curTotal = curSubTotal + curIV - curDescuento;

            return vGrid;
        }


        /// <summary>
        /// Obtiene PIN
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodOrden"></param>
        /// <returns></returns>
        public ErrorDto OrdenPin_Obtener(int CodEmpresa, string CodOrden)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto();
            resp.Code = 1;
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select pin_autorizacion from cpr_ordenes where cod_orden = '{CodOrden}'";
                    resp.Description = connection.Query<string>(query).FirstOrDefault();

                    if (resp.Description != "0")
                    {
                        resp.Code = 2;
                    }

                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "Error en obtener Pin de autorización";
                _ = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Orden Pin Verificar
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodOrden"></param>
        /// <param name="OrdPin"></param>
        /// <returns></returns>
        public ErrorDto OrdenPin_Verifica(int CodEmpresa, string CodOrden, string OrdPin)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto();
            resp.Code = 1;
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select isnull(count(*),0) as Existe from cpr_ordenes 
                                          where cod_orden = '{CodOrden}'  and pin_entrada = '{OrdPin}' ";
                    resp.Description = connection.Query<string>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "Error en Verificacion Pin de autorización";
                _ = ex.Message;
            }
            return resp;
        }


        /// <summary>
        /// Orden consecutivo obtener
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto OrdenConsecutivo_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto();
            resp.Code = 1;
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select isnull(max(cod_orden),0) + 1 as Ultimo from cpr_compras ";
                    var consecutivo = connection.Query<string>(query).FirstOrDefault();
                    consecutivo = consecutivo.PadLeft(10, '0');
                    resp.Description = consecutivo.ToString();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "Error en Verificacion Pin de autorización";
                _ = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Busco articulos activos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="linea"></param>
        /// <param name="consecutivo"></param>
        /// <param name="cod_factura"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_orden"></param>
        /// <param name="cod_proveedor"></param>
        /// <returns></returns>
        private ErrorDto BuscoArticulosActivos(
            int CodEmpresa,
            OrdenCompraDetalleData linea,
            string consecutivo,
            string cod_factura,
            string usuario,
            string cod_orden,
            int cod_proveedor)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select TIPO_PRODUCTO from pv_productos where cod_producto = '{linea.cod_producto}'";
                    var resp = connection.Query<string>(query).FirstOrDefault();

                    //Busco UEN con numero de orden:
                    query = $@"select COD_UNIDAD from CPR_ORDENES_UENS where cod_orden = '{cod_orden}' AND 
                              COD_PRODUCTO = '{linea.cod_producto}' ";

                    var cod_uen = connection.Query<string>(query).FirstOrDefault();

                    if (resp == "A")
                    {
                        for (int i = 0; i < linea.cantidad; i++)
                        {
                            var procedure = "[spCPR_CONTROL_ACTIVOS_GUARDAR]";
                            var values = new
                            {
                                COD_PRODUCTO = linea.cod_producto,
                                COD_UEN = cod_uen,
                                COD_PROVEEDOR = cod_proveedor,
                                COD_COMPRA = consecutivo,
                                COSTO_TOTAL = linea.cantidad * linea.precio,
                                COSTO_UNITARIO = linea.precio,
                                FACTURA = cod_factura,
                                FECHA_COMPRA = DateTime.Now,
                                COD_BODEGA = linea.cod_bodega,
                                ESTADO = 'P',
                                NUMERO_PLACA = "",
                                COD_LOCALIZACION = "",
                                MARCA = "",
                                MODELO = "",
                                SERIE = "",
                                OBSERVACIONES = "",
                                ID_RESPONSABLE = "",
                                COD_REQUISICION = "",
                                ENTREGA_USUARIO = "",
                                ENTREGA_FECHA = "",
                                ACTIVO_USUARIO = "",
                                ACTIVO_FECHA = "",
                                REGISTRO_USUARIO = usuario
                            };

                            response.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                            response.Description = "Ok";
                        }



                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;

            }
            return response;
        }


        /// <summary>
        /// Verifica totales fact
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="orden"></param>
        /// <returns></returns>
        public ErrorDto fxVerificaTotalesFac(int CodEmpresa, ComprasOrdenDatos orden)
        {
            ErrorDto result = new ErrorDto();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //Obtengo Total de la orden
                    string query = $@"SELECT  TOTAL FROM CPR_ORDENES co 
                                       WHERE COD_ORDEN = '{orden.cod_orden}' ";

                    float totalOrd = connection.Query<float>(query).FirstOrDefault();

                    //Obtengo Total de la Factura
                    query = $@"SELECT  ISNULL(SUM(TOTAL), 0)  FROM CPR_COMPRAS co 
                                       WHERE COD_ORDEN = '{orden.cod_orden}' ";
                    float totalFac = connection.Query<float>(query).FirstOrDefault();

                    //sumo el total de la orden nueva
                    float total = 0;
                    foreach (OrdenCompraDetalleData item in orden.lineas)
                    {
                        float curTmpDesc = ((item.cantidad * item.precio) * (item.descuento / 100));
                        float curTmpIV = (((item.cantidad * item.precio) - curTmpDesc) * (item.imp_ventas / 100));
                        total = total + (item.cantidad * item.precio) - curTmpDesc + curTmpIV;
                    }

                    //Si la suma entre total y totalFac es menor o igual al totalOrd dejo pasar si no da error.

                    if (total + totalFac > totalOrd)
                    {
                        result.Code = -1;
                        result.Description = "El total de la factura no puede ser mayor al total de la orden";
                    }
                    else
                    {
                        result.Code = 0;
                        result.Description = null;
                    }

                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;

        }

        /// <summary>
        /// Facturas autorizar obtener
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="proveedor"></param>
        /// <returns></returns>
        public ErrorDto<List<FacturasAutorizarDTO>> FacturasAutorizar_Obtener(int CodEmpresa, string usuario, int proveedor)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<FacturasAutorizarDTO>>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"SELECT 
                                        f.ID,
                                        f.COD_DOCUMENTO, 
                                        f.NOMBRE_PROV, 
                                        f.MONTO_TOTAL,
                                        CASE f.ESTADO
                                            WHEN 'R' THEN 'Rechazada'
                                            WHEN 'P' THEN 'Pendiente'
                                            WHEN 'A' THEN 'Autorizada'
                                            WHEN 'E' THEN 'Emitida'
                                            ELSE 'DESCONOCIDO'
                                        END AS ESTADO_DESCRIPCION
                                    FROM 
                                        CPR_FACTURAS_XML f
                                    JOIN 
                                        CXP_PROVEEDORES p ON f.CED_JUR_PROV = REPLACE(p.CEDJUR, '-', '')
                                    WHERE 
                                        f.ESTADO IN ('P', 'E')
                                        AND cod_proveedor = {proveedor};";
                    response.Result = connection.Query<FacturasAutorizarDTO>(query).ToList();
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
        /// Facturas autorizar rechazar
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod"></param>
        /// <param name="cod_factura"></param>
        /// <param name="justificacion"></param>
        /// <returns></returns>
        public ErrorDto Factura_AutorizarRechazar(int CodEmpresa, string usuario, string cod, string cod_factura, string justificacion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto info = new ErrorDto();
            info.Code = 0;

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"UPDATE CPR_FACTURAS_XML SET estado = '{cod}', JUSTIFICACION = '{justificacion}'
                                  WHERE COD_DOCUMENTO = '{cod_factura}'";
                    info.Code = connection.Execute(query);

                }

                if (cod == "A")
                {
                    CorreoNotificaAutorizaFactura_Enviar(CodEmpresa, cod_factura, usuario);
                }


            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message.ToString();
            }
            return info;
        }

        /// <summary>
        /// Valida autorizacion factura
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_orden"></param>
        /// <returns></returns>
        public ErrorDto ValidaAutorizacion(int CodEmpresa, string usuario, string cod_orden)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto info = new ErrorDto();
            info.Code = 0;

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"
                                    SELECT 1
                                    WHERE EXISTS (
                                        SELECT 1
                                        FROM CPR_SOLICITUD s
                                        JOIN CPR_ORDENES o ON s.adjudica_orden = o.cod_orden
                                        JOIN CORE_UENS_USUARIOS_ROLES r ON r.COD_UNIDAD = s.COD_UNIDAD
                                        WHERE o.COD_ORDEN = @cod_orden
                                          AND r.CORE_USUARIO = @usuario
                                        AND (r.ROL_AUTORIZA = 1));";

                    // Ejecutar el query como escalar para obtener si devuelve 1 (autorizado)
                    var result = connection.ExecuteScalar<int?>(query, new { cod_orden, usuario });

                    info.Code = result.HasValue ? 1 : 0;

                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }

            return info;
        }

        /// <summary>
        /// Correo envio notificacion aprobacion de factura
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_factura"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        private async Task CorreoNotificaAutorizaFactura_Enviar(int CodEmpresa, string cod_factura, string usuario)
        {

            ErrorDto resp = new ErrorDto();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<OrdenLineas> info = new List<OrdenLineas>();

            int CodCliente = CodEmpresa;

            try
            {

                using var connection = new SqlConnection(stringConn);
                {

                    var queryProveedor = @$"SELECT NOMBRE_PROV FROM CPR_FACTURAS_XML WHERE cod_documento = '{cod_factura}'";
                    Proveedor = connection.Query<string>(queryProveedor).FirstOrDefault();

                    var queryCed = @$"SELECT CED_JUR_PROV FROM CPR_FACTURAS_XML WHERE cod_documento = '{cod_factura}'";
                    string ced_jur = connection.Query<string>(queryCed).FirstOrDefault();

                    var queryEmail = @$"SELECT EMAIL FROM CXP_PROVEEDORES WHERE cedjur = '{ced_jur}'";
                    string EmailProveedor = connection.Query<string>(queryEmail).FirstOrDefault();

                }


                EnvioCorreoModels eConfig = _envioCorreoDB.CorreoConfig(CodEmpresa, Notificaciones);
                string body = @$"<html lang=""es"">
                        <head>
                            <meta charset=""UTF-8"">
                            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                            <title>Aprobación de factura</title>
                            <style>
                                body {{
                                    font-family: Arial, sans-serif;
                                }}
                                .container {{
                                    width: 600px;
                                    margin: 0 auto;
                                    border: 1px solid #eaeaea;
                                    padding: 20px;
                                }}
                                .header {{
                                    background-color: #e8f3ff;
                                    padding: 10px;
                                }}
                                .header img {{
                                    width: auto;
                                    height: 50px;
                                }}
                                .content {{
                                    margin-top: 20px;
                                }}
                                .content h2 {{
                                    font-size: 16px;
                                    color: #0072ce;
                                }}
                                .table {{
                                    width: 100%;
                                    margin-top: 20px;
                                    border-collapse: collapse;
                                }}
                                .table th, .table td {{
                                    padding: 10px;
                                    border: 1px solid #dcdcdc;
                                    text-align: left;
                                }}
                                .table th {{
                                    background-color: #0072ce;
                                    color: white;
                                }}

                            </style>
                        </head>
                        <body>
                            <div class=""container"">
                                <div class=""header"">
                                    <img src=""https://www.aseccssenlinea.com/Content/Login/ASECCSSLogo.png"" alt=""Logo"">
                                </div>
                                <div class=""content"">
                                    <h2><strong>Aprobación de factura</strong></h2>
                                    <p>Estimado/a {Proveedor} la factura número #{cod_factura} se ha aprobado.</p>";



                List<IFormFile> Attachments = new List<IFormFile>();


                if (sendEmail == "Y")
                {
                    EmailRequest emailRequest = new EmailRequest();

                    emailRequest.To = EmailProveedor;
                    emailRequest.From = eConfig.User;
                    emailRequest.Subject = "Aprobación de factura";
                    emailRequest.Body = body;
                    emailRequest.Attachments = Attachments;

                    if (eConfig != null)
                    {
                        await _envioCorreoDB.SendEmailAsync(emailRequest, eConfig, resp);
                    }

                    BitacoraEnvioCorreo(new BitacoraComprasInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        consec = 0,
                        movimiento = "Registra",
                        detalle = $@"Envío de correo de aprobacion de factura #{cod_factura}",
                        registro_usuario = usuario
                    });
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
        }

        /// <summary>
        /// Bitacora de envio de correo
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto BitacoraEnvioCorreo(BitacoraComprasInsertarDTO req)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(req.EmpresaId);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {

                using var connection = new SqlConnection(stringConn);
                {


                    var strSQL = $@"INSERT INTO [dbo].[BITACORA_COMPRAS]
                                           ([ID_COMPRA]
                                           ,[CONSEC]
                                           ,[MOVIMIENTO]
                                           ,[DETALLE]
                                           ,[REGISTRO_FECHA]
                                           ,[REGISTRO_USUARIO])
                                     VALUES
                                           ('{req.id_bitacora}'
                                           ,{req.consec}
                                           ,'{req.movimiento}' 
                                           , '{req.detalle}'
                                           , getdate()
                                           , '{req.registro_usuario}' )";

                    resp.Code = connection.Execute(strSQL);
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
        /// Envia registro de factura
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_factura"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_proveedor"></param>
        /// <returns></returns>
        private async Task CorreoNotificaRegistraFactura_Enviar(int CodEmpresa, string cod_factura, string usuario, int cod_proveedor)
        {

            ErrorDto resp = new ErrorDto();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<OrdenLineas> info = new List<OrdenLineas>();

            int CodCliente = CodEmpresa;

            try
            {

                using var connection = new SqlConnection(stringConn);
                {

                    var queryNombreProveedor = @$"SELECT descripcion FROM CXP_PROVEEDORES WHERE cod_proveedor = '{cod_proveedor}'";
                    Proveedor = connection.Query<string>(queryNombreProveedor).FirstOrDefault();

                    var queryEmail = @$"SELECT EMAIL FROM CXP_PROVEEDORES WHERE cod_proveedor = '{cod_proveedor}'";
                    EmailProveedor = connection.Query<string>(queryEmail).FirstOrDefault();

                }


                EnvioCorreoModels eConfig = _envioCorreoDB.CorreoConfig(CodEmpresa, Notificaciones);
                string body = @$"<html lang=""es"">
                        <head>
                            <meta charset=""UTF-8"">
                            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                            <title>Registro de factura</title>
                            <style>
                                body {{
                                    font-family: Arial, sans-serif;
                                }}
                                .container {{
                                    width: 600px;
                                    margin: 0 auto;
                                    border: 1px solid #eaeaea;
                                    padding: 20px;
                                }}
                                .header {{
                                    background-color: #e8f3ff;
                                    padding: 10px;
                                }}
                                .header img {{
                                    width: auto;
                                    height: 50px;
                                }}
                                .content {{
                                    margin-top: 20px;
                                }}
                                .content h2 {{
                                    font-size: 16px;
                                    color: #0072ce;
                                }}
                                .table {{
                                    width: 100%;
                                    margin-top: 20px;
                                    border-collapse: collapse;
                                }}
                                .table th, .table td {{
                                    padding: 10px;
                                    border: 1px solid #dcdcdc;
                                    text-align: left;
                                }}
                                .table th {{
                                    background-color: #0072ce;
                                    color: white;
                                }}

                            </style>
                        </head>
                        <body>
                            <div class=""container"">
                                <div class=""header"">
                                    <img src=""https://www.aseccssenlinea.com/Content/Login/ASECCSSLogo.png"" alt=""Logo"">
                                </div>
                                <div class=""content"">
                                    <h2><strong>Registro de factura</strong></h2>
                                    <p>Estimado/a {Proveedor} la factura número #{cod_factura} se ha registrado.</p>";



                List<IFormFile> Attachments = new List<IFormFile>();


                if (sendEmail == "Y")
                {
                    EmailRequest emailRequest = new EmailRequest();

                    emailRequest.To = EmailProveedor;
                    emailRequest.From = eConfig.User;
                    emailRequest.Subject = "Registro de factura";
                    emailRequest.Body = body;
                    emailRequest.Attachments = Attachments;

                    if (eConfig != null)
                    {
                        await _envioCorreoDB.SendEmailAsync(emailRequest, eConfig, resp);
                    }

                    BitacoraEnvioCorreo(new BitacoraComprasInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        consec = 0,
                        movimiento = "Registra",
                        detalle = $@"Envío de correo de registro de factura #{cod_factura}",
                        registro_usuario = usuario
                    });
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
        }

        /// <summary>
        /// Verifica si el número de factura ya existe para el proveedor seleccionado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_factura"></param>
        /// <param name="cod_proveedor"></param>
        /// <returns></returns>
        private ErrorDto fxVerificaNumeroFactura(int CodEmpresa, string cod_factura, int cod_proveedor)
        {
            ErrorDto resp = new ErrorDto();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "select COUNT(*) FROM CPR_COMPRAS where cod_factura = @factura AND cod_proveedor = @proveedor";
                    var existe = connection.Query<int>(query, new { factura = cod_factura, proveedor = cod_proveedor }).FirstOrDefault();

                    if (existe > 0)
                    {
                        resp.Code = -1;
                        resp.Description = " - El número de factura ya existe para el proveedor seleccionado.";
                    }
                    else
                    {
                        resp.Code = 0;
                        resp.Description = "El número de factura es válido.";
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


        

    }
}
