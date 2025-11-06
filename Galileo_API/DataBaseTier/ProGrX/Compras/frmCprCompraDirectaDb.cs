using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models;
using PgxAPI.Models.CPR;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.Security;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmCprCompraDirectaDB
    {
        private readonly IConfiguration _config;
        mProGrX_AuxiliarDB mProGrxAuxiliar;
        MSecurityMainDb DBBitacora;
        mComprasDB mComprasDB;

        public frmCprCompraDirectaDB(IConfiguration config)
        {
            _config = config;
            DBBitacora = new MSecurityMainDb(config);
            mProGrxAuxiliar = new mProGrX_AuxiliarDB(config);
            mComprasDB = new mComprasDB(config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return DBBitacora.Bitacora(data);
        }

        public ErrorDto<CompraDirectaData> CompraDirecta_Obtener(int CodEmpresa, string CodCompra, string CodOrden, int Codproveedor)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<CompraDirectaData>();
            response.Result = new CompraDirectaData();
            try
            {
                string filter = "";
                if (CodOrden != "0")
                {
                    filter += $"  and E.cod_orden = '{CodOrden}' ";
                }
                if (Codproveedor != 0)
                {
                    filter += $"  and E.cod_proveedor = '{Codproveedor}' ";
                }

                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select E.*,rtrim(C.descripcion) as 'Causa_Desc', rtrim(C.Tipo_Orden) as 'Causa_Id'
                                   ,P.descripcion as Proveedor,O.nota
                                    from cpr_ordenes O inner join cpr_Tipo_Orden C on O.Tipo_Orden = C.Tipo_Orden
                                    inner join cpr_compras E on O.cod_orden = E.cod_orden
                                    inner join cxp_proveedores P on E.cod_proveedor = P.cod_proveedor
                                    where E.cod_compra = '{CodCompra}' {filter}";
                    response.Result = connection.Query<CompraDirectaData>(query).FirstOrDefault();
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


        public ErrorDto<CompraDirectaListaData> CompraDirectaDetalle_Obtener(int CodEmpresa, string jfiltros, string? CodFactura, int? Codproveedor)
        {
            OrderLineaTablaFiltros filtros = JsonConvert.DeserializeObject<OrderLineaTablaFiltros>(jfiltros) ?? new OrderLineaTablaFiltros();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            string paginaActual = " ", paginacionActual = " ";

            var response = new ErrorDto<CompraDirectaListaData>();
            response.Result = new CompraDirectaListaData();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var qTotal = $@"select Count(D.cod_producto)
                                         from cpr_compras_detalle D inner join pv_productos P on D.cod_producto = P.cod_producto
                                      where D.cod_factura = '{CodFactura}' and D.cod_proveedor = {Codproveedor} ";
                    response.Result.total = connection.Query<int>(qTotal).FirstOrDefault();


                    if (response.Result.total != 0)
                    {
                        var qCant = $@"select sum(D.cantidad)
                                        from cpr_compras_detalle D inner join pv_productos P on D.cod_producto = P.cod_producto
                                      where D.cod_factura = '{CodFactura}' and D.cod_proveedor = {Codproveedor} ";

                        response.Result.cantidad = connection.Query<long>(qCant).FirstOrDefault();
                    }
                    else
                    {
                        response.Result.cantidad = 0;
                    }
                    string vFiltro = "";
                    //if (filtros.filtro.ToString() != "")
                    //{
                    //    vFiltro = " AND D.cod_producto LIKE '%" + filtros.filtro + "%' OR P.descripcion LIKE '%" + filtros.filtro + "%' ";
                    //}

                    //if (filtros.pagina != null)
                    //{
                    //    paginaActual = " OFFSET " + filtros.pagina + " ROWS ";
                    //    paginacionActual = " FETCH NEXT " + filtros.paginacion + " ROWS ONLY ";
                    //}

                    //var query = $@"select D.cod_producto,P.descripcion,D.cantidad,D.cod_bodega,D.precio,isnull(D.descuento,0),D.imp_ventas,0
                    //                  from cpr_compras_detalle D inner join pv_productos P on D.cod_producto = P.cod_producto
                    //                  where D.cod_factura = '{CodFactura}' and D.cod_proveedor = {Codproveedor}
                    //                    {vFiltro}
                    //                  order by D.Linea 
                    //                    {paginaActual}
                    //                    {paginacionActual}";


                    var query = $@"select D.cod_producto,P.descripcion,D.cantidad,D.cod_bodega,D.precio,isnull(D.descuento,0) 
as descuento,D.imp_ventas,0 as total
, CASE WHEN (
									     SELECT U.COD_PRODUCTO FROM CPR_ORDENES_UENS U WHERE U.COD_PRODUCTO = D.cod_producto
					                    AND U.COD_ORDEN = C.COD_ORDEN
                                         GROUP BY U.COD_ORDEN, U.COD_PRODUCTO
									   ) IS NOT NULL THEN 1
									   ELSE 0
									   END AS i_existe,
									    CASE WHEN (
									     	SELECT COALESCE((SELECT SUM(U.CANTIDAD)
												 FROM CPR_ORDENES_UENS U
												 WHERE U.COD_PRODUCTO = D.cod_producto
												   AND U.COD_ORDEN = C.COD_ORDEN
                                           GROUP BY U.COD_ORDEN, U.COD_PRODUCTO), 0)
									   ) < D.cantidad THEN 0
									   ELSE 1
									   END AS i_completo
                                      from cpr_compras_detalle D 
									  inner join pv_productos P on D.cod_producto = P.cod_producto
									  left join cpr_compras C ON C.COD_FACTURA = D.COD_FACTURA
                                      where D.cod_factura = '{CodFactura}' and D.cod_proveedor = {Codproveedor}
                                      order by D.Linea";


                    response.Result.lineas = connection.Query<CompraDirectaDetalle>(query).ToList();
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

        private ErrorDto Proveedor_Saldo_Actualiza(int CodEmpresa, int CodProveedor, float Saldo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"update cxp_proveedores set saldo = isnull(saldo,0) + {Saldo} where cod_proveedor = {CodProveedor}";
                    var result = connection.Execute(query);
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "Error en Actualizar Saldo Proveedor";
                _ = ex.Message;
            }
            return resp;
        }

        private ErrorDto PagoContado_Regristra(int CodEmpresa, CompraDirectaInsert compra)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto();
            resp.Code = 1;
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"insert cxp_pagoProv(
                                            NPago,
                                            Cod_Proveedor,
                                            Cod_Factura,
                                            Fecha_Vencimiento,
                                            Monto,
                                            Frecuencia,
                                            Tipo_Transac,
                                            User_TrasLada,
                                            Fecha_Traslada,
                                            Tesoreria,
                                            Pago_Tercero,
                                            Apl_Cargo_Flotante,
                                            Pago_Anticipado,
                                            forma_pago, 
                                            IMPORTE_DIVISA_REAL
                                            )values(
                                            1,
                                            {compra.cod_proveedor},
                                            '{compra.cod_factura}',
                                            GETDATE(),
                                            {compra.total} ,
                                            0,
                                            0,
                                            Null,
                                            Null,
                                            Null,
                                            '',
                                            0,
                                            0,
                                            'CO', 
                                            {compra.total} )";
                    var result = connection.Execute(query);
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "Error en Actualizar Saldo Proveedor";
                _ = ex.Message;
            }
            return resp;
        }

        public ErrorDto CostoArticulos_Actualiza(int CodEmpresa, string Usuario, string CodCompra)
        {
            ErrorDto result = new ErrorDto();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var procedure = "spCRPActualizaCts";
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

        //Nuevos Metodos de Guardado

        public ErrorDto CompraDirecta_Insertar(int CodEmpresa, CompraDirectaInsert orden)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            string mensaje = "";
            bool valida = true;
            valida = fxValida(CodEmpresa, orden, ref mensaje);

            ErrorDto resp = new ErrorDto();
            resp.Code = 0;

            if (!valida)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = mensaje
                };
            }

            try
            {
                /*'Solo se puede Insertar y no Editar
                  '01 - Guardar el registro en Ordenes y Detalles como procesado
                  '02 - Guardar el registro en las Entradas y Afectar Inventarios*/
                float curSubTotal = 0;
                float curDescuento = 0;
                float curIV = 0;
                float curCantidad = 0;
                float curTotal = 0;

                sbCalculaTotales(orden.lineas,
                    ref curSubTotal,
                        ref curDescuento,
                        ref curIV,
                        ref curCantidad,
                        ref curTotal);

                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select isnull(max(cod_orden),0) + 1 as Ultimo from cpr_Ordenes";
                    var vCodigo = connection.Query<string>(query).FirstOrDefault();
                    vCodigo = vCodigo.PadLeft(10, '0');
                    resp.Description = vCodigo.ToString();

                    query = $@"insert cpr_ordenes(
                                    cod_orden,
                                    Tipo_Orden,
                                    estado,
                                    nota,
                                    genera_user,
                                    genera_fecha,
                                    subtotal,
                                    descuento,
                                    imp_ventas,
                                    total,
                                    autoriza_fecha,
                                    autoriza_user,
                                    pin_autorizacion,
                                    pin_entrada,
                                    proceso,
                                    cod_proveedor
                                    ) values(
                                    '{vCodigo}',
                                    '{orden.causa}',
                                    'A',
                                    '{orden.notas}',
                                    '{orden.usuario.ToUpper()}',
                                    GETDATE(),
                                    {orden.sub_total},
                                    {orden.descuento},
                                    {orden.imp_ventas},
                                    {orden.total},
                                    GETDATE(),
                                    '{orden.usuario.ToUpper()}',
                                    0,
                                    '',
                                    'D',
                                    '{orden.cod_proveedor}')";
                    var result = connection.Execute(query);


                    query = $@"Insert CPR_ORDENES_PROCESO(
                                    COD_ORDEN, 
                                    COD_PROVEEDOR, 
                                    REGISTRO_FECHA, 
                                    REGISTRO_USUARIO, 
                                    COTIZA_FECHA,
                                    COTIZA_USUARIO,
                                    ADJUDICA_FECHA,
                                    ADJUDICA_USUARIO, 
                                    NOTAS
                                    ) values(
                                    '{vCodigo}', 
                                    {orden.cod_proveedor}, 
                                    GETDATE(),
                                    '{orden.usuario}', 
                                    GETDATE(),
                                    '{orden.usuario}', 
                                    GETDATE(),
                                    '{orden.usuario}', 
                                    'Compra Directa!')";
                    result = connection.Execute(query);

                    //Bitacora
                    Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = orden.usuario,
                        DetalleMovimiento = "Registra, Orden Compra: " + vCodigo,
                        Movimiento = "Registra - WEB",
                        Modulo = 35
                    });

                    //Guarda el Detalle
                    query = $@"delete cpr_ordenes_detalle where cod_orden = '{vCodigo}'";
                    connection.Execute(query);

                    int contador = 0;
                    foreach (CompraDirectaDetalle item in orden.lineas)
                    {
                        contador++;
                        query = $@"insert cpr_ordenes_detalle(
                                            linea,
                                            cod_orden,
                                            cod_producto,
                                            cantidad,
                                            estado,
                                            cantidad_despachada,
                                            precio,
                                            descuento,
                                            imp_ventas,
                                            imp_consumo 
                                            )values (
                                            {contador},
                                            '{vCodigo}',
                                            '{item.cod_producto}',
                                            {item.cantidad},
                                            'D',
                                             0,
                                            {item.precio},
                                            {item.descuento},
                                            {item.imp_ventas},
                                            0
                                            )";

                        connection.Execute(query);
                    }

                    //*************** Guardar la Entrada a partir de aqui ****************
                    query = $@"select isnull(max(cod_compra),0) + 1 as Ultimo from cpr_compras";
                    var vCompra = connection.Query<string>(query).FirstOrDefault();
                    vCompra = vCompra.PadLeft(10, '0');

                    resp.Description += "-" + vCompra;

                    string cxp_estado = "";
                    if (orden.forma_pago == "CR")
                    {
                        cxp_estado = "P";
                    }
                    else
                    {
                        cxp_estado = "G";
                    }

                    query = $@" insert cpr_compras(
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
                                     divisa,
                                     tipo_pago               
                                     )values(
                                     'P',
                                     '{orden.cod_factura}',
                                     '{orden.tipo_pago}',
                                     {orden.cod_proveedor},
                                     '{vCompra}',
                                     '{vCodigo}',
                                     '{orden.usuario}',
                                     '{orden.fecha}',
                                     '{orden.fecha}',
                                     {orden.sub_total},
                                     {orden.descuento},
                                     {orden.imp_ventas},
                                     0,
                                     {orden.total},
                                     '{cxp_estado}',
                                     'P', '{orden.divisa}', '{orden.forma_pago}')";
                    connection.Execute(query);

                    //Actualiza Saldo Proveedores
                    ErrorDto pvSaldo = Proveedor_Saldo_Actualiza(CodEmpresa, orden.cod_proveedor, orden.total);
                    if (pvSaldo.Code != 0)
                    {
                        resp = pvSaldo;
                    }

                    if (orden.tipo_pago == "CO")
                    {
                        PagoContado_Regristra(CodEmpresa, orden);
                    }

                    //Bitacora
                    Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = orden.usuario,
                        DetalleMovimiento = "Registra, Compra Directa: " + vCompra,
                        Movimiento = "Registra - WEB",
                        Modulo = 35
                    });

                    //Guardar Detalle de la Orden
                    query = $@"delete cpr_compras_detalle where cod_factura = '{orden.cod_factura}'  and cod_proveedor = {orden.cod_proveedor} ";
                    connection.Execute(query);

                    contador = 0;
                    foreach (CompraDirectaDetalle item in orden.lineas)
                    {
                        contador++;
                        query = $@" insert cpr_compras_detalle(
                                             linea,
                                             cod_factura,
                                             cod_proveedor,
                                             cod_producto,
                                             cantidad,
                                             cod_bodega,
                                             precio,
                                             descuento,
                                             imp_ventas,
                                             imp_consumo
                                             )values(
                                            {contador}, 
                                            '{orden.cod_factura}',
                                            {orden.cod_proveedor},
                                            '{item.cod_producto}',
                                            {item.cantidad},
                                            '{item.cod_bodega}',
                                            {item.precio},
                                            {item.descuento},
                                            {item.imp_ventas},
                                            0
                                             )";

                        connection.Execute(query);

                        //LLAMO mProGrX_AuxiliarDB para actualizar el inventario
                        mProGrX_AuxiliarDB mProGrX_AuxiliarDB = new mProGrX_AuxiliarDB(_config);
                        CompraInventarioDto compraInventario = new CompraInventarioDto();
                        compraInventario.CodProducto = item.cod_producto;
                        compraInventario.Cantidad = Decimal.Parse(item.cantidad.ToString());
                        compraInventario.CodBodega = item.cod_bodega;
                        compraInventario.CodTipo = vCompra;
                        compraInventario.Origen = "Compra";
                        compraInventario.Fecha = DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss");
                        compraInventario.Precio = decimal.Parse(item.precio.ToString());
                        compraInventario.ImpVentas = decimal.Parse(item.imp_ventas.ToString());
                        compraInventario.ImpConsumo = 0;
                        compraInventario.TipoMov = "E";
                        mProGrX_AuxiliarDB.sbInvInventario(CodEmpresa, compraInventario);

                        CostoArticulos_Actualiza(CodEmpresa, orden.usuario, vCompra);
                    }

                    mComprasDB.FacturaOrdenes_Actualizar(CodEmpresa, orden.cod_factura, orden.cod_proveedor);
                }

            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        private bool fxValida(int CodEmpresa, CompraDirectaInsert orden, ref string mensaje)
        {
            mensaje = "";
            bool resp = true;

            mensaje = fxInvVerificaLineaDetalle(CodEmpresa, orden.lineas, "E", 1, 4);

            if (orden.cod_factura.Length == 0)
            {
                mensaje += " - Debe ingresar el número de factura";
            }

            if (!mProGrxAuxiliar.fxInvPeriodos(CodEmpresa, orden.fecha))
            {
                mensaje += " - El periodo en el que desea realizar el movimiento se encuentra cerrado ...";
            }

            if (mensaje.Length > 0)
            {
                resp = false;
            }
            return resp;
        }

        private string fxInvVerificaLineaDetalle(int CodEmpresa, List<CompraDirectaDetalle> vGrid,
          string vMov, int? ColBod1 = 0, int ColBod2 = 0)
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

                    foreach (CompraDirectaDetalle item in vGrid)
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
                        if (ColBod1 > 0)
                        {
                            var query = $@"select permite_entradas,permite_salidas,estado from pv_bodegas where cod_bodega = '{item.cod_bodega}'";
                            List<Models.BodegaDto> exist = connection.Query<Models.BodegaDto>(query).ToList();
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
                                                return "La bodega " + ColBod1 + " - No permite Entradas";
                                            }
                                            break;
                                        case "S":
                                        case "R":
                                        case "T":
                                            if (exist[0].permite_salidas != "1")
                                            {
                                                return "La bodega " + ColBod1 + " - No permite Salidas";
                                            }
                                            break;
                                    }
                                }
                            }
                        }

                        if (ColBod2 > 0)
                        {
                            var query = $@"select permite_entradas,permite_salidas,estado from pv_bodegas where cod_bodega = '{item.cod_bodega}'";
                            List<Models.BodegaDto> exist = connection.Query<Models.BodegaDto>(query).ToList();
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

        private List<CompraDirectaDetalle> sbCalculaTotales(List<CompraDirectaDetalle> vGrid
           , ref float curSubTotal, ref float curDescuento, ref float curIV, ref float curCantidad,
           ref float curTotal)
        {
            foreach (CompraDirectaDetalle item in vGrid)
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
    }
}
