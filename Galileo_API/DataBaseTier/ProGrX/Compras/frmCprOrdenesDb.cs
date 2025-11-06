using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models;
using PgxAPI.Models.CPR;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.Security;
using System.Data;
using System.Web;

namespace PgxAPI.DataBaseTier
{
    public class frmCprOrdenesDB
    {
        private readonly IConfiguration _config;
        MSecurityMainDb DBBitacora;
        mProGrx_Main mProGrx_Main;
        private readonly EnvioCorreoDB _envioCorreoDB;
        public string sendEmail = "";
        public string Notificaciones = "";
        public string EmailProveedor = "";

        public frmCprOrdenesDB(IConfiguration config)
        {
            _config = config;
            DBBitacora = new MSecurityMainDb(_config);
            mProGrx_Main = new mProGrx_Main(_config);
            _envioCorreoDB = new EnvioCorreoDB(_config);
            sendEmail = _config.GetSection("AppSettings").GetSection("EnviaEmail").Value.ToString();
            Notificaciones = _config.GetSection("AppSettings").GetSection("Notificaciones").Value.ToString();
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return DBBitacora.Bitacora(data);
        }

        /// <summary>
        /// Se obtiene la orden seleccionada
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodOrden"></param>
        /// <returns></returns>
        public ErrorDto<OrdenDto> OrdenesSeleccionada(int CodEmpresa, string CodOrden, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<OrdenDto>();
            response.Result = new OrdenDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {


                    var query = $@"select O.*,rtrim(C.Tipo_Orden) as 'Causa_Id', rtrim(C.descripcion) as 'Causa_Desc', isnull(Prov.Descripcion,'') as 'Proveedor_Desc',
                                    Prov.CEDJUR AS cedula_proveedor, Prov.TELEFONO AS telefono_proveedor, Prov.DIRECCION AS direccion_proveedor 
                                    , RIGHT(REPLICATE('0', 10) + CAST(sp.CPR_ID AS VARCHAR), 10) AS cod_solicitud, s.cod_unidad, s.DIVISA
                                        from cpr_ordenes O inner join cpr_Tipo_Orden C on O.Tipo_Orden = C.Tipo_Orden
                                        left join CXP_Proveedores Prov on O.cod_Proveedor = Prov.Cod_Proveedor
                                        left join CPR_SOLICITUD_PROV sp ON sp.ADJUDICA_ORDEN  = O.COD_ORDEN AND sp.PROVEEDOR_CODIGO = O.COD_PROVEEDOR 
										left join CPR_SOLICITUD s ON s.CPR_ID = sp.CPR_ID
                                        where O.cod_orden ='{CodOrden}'";
                    response.Result = connection.Query<OrdenDto>(query).FirstOrDefault();

                    string UEN = response.Result.cod_unidad;
                    decimal monto = 0;

                    if (response.Result.divisa == "USD")
                    {
                        monto = (decimal)response.Result.Total;
                    }
                    else
                    {
                        var queryTipoCambio = @"SELECT VALOR FROM SIF_PARAMETROS WHERE COD_PARAMETRO = 'TC'";
                        decimal tipoCambio = connection.Query<decimal>(queryTipoCambio).FirstOrDefault();
                        monto = (decimal)response.Result.Total / tipoCambio;
                    }

                    if (!string.IsNullOrWhiteSpace(UEN))
                    {
                        var queryRangos = @"
                                    SELECT r.MONTO_MINIMO, r.MONTO_MAXIMO 
                                    FROM cpr_orden_rangos r  
                                    INNER JOIN CPR_RANGO_USUARIO u ON r.cod_rango = u.cod_rango 
                                    WHERE u.USUARIO = @Usuario AND u.UEN = @UEN AND u.ACTIVO = 1";

                        var rangos = connection.Query<(decimal MONTO_MINIMO, decimal MONTO_MAXIMO)>(
                            queryRangos, new { Usuario = usuario, UEN = UEN }).ToList();

                        bool dentroDeRango = rangos.Any(r => monto >= r.MONTO_MINIMO && monto <= r.MONTO_MAXIMO);

                        if (!dentroDeRango)
                        {
                            response.Code = -2;
                            response.Description = $"No tiene permisos para visualizar la orden #{CodOrden}";
                            return response;
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
        /// Obtiene las lineas de la orden
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="jfiltros"></param>
        /// <returns></returns>
        public ErrorDto<OrdenLineasData> OrdenLineasObtener(int CodEmpresa, string jfiltros)
        {
            OrderLineaTablaFiltros filtros = JsonConvert.DeserializeObject<OrderLineaTablaFiltros>(jfiltros);
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<OrdenLineasData>();
            response.Result = new OrdenLineasData();
            string paginaActual = " ", paginacionActual = " ";
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var qTotal = $@"select Count(D.cod_producto)
                                        from cpr_ordenes_detalle D inner join pv_productos P on D.cod_producto = P.cod_producto
                                        where D.cod_orden = '{filtros.CodOrden}' ";
                    response.Result.total = connection.Query<int>(qTotal).FirstOrDefault();

                    if (response.Result.total != 0)
                    {
                        var qCant = $@"select sum(D.cantidad)
                                        from cpr_ordenes_detalle D inner join pv_productos P on D.cod_producto = P.cod_producto
                                        where D.cod_orden = '{filtros.CodOrden}' ";

                        response.Result.cantidad = connection.Query<long>(qCant).FirstOrDefault();
                    }
                    else
                    {
                        response.Result.cantidad = 0;
                    }



                    string vFiltro = "";
                    //if (filtros.filtro.ToString() != "" )
                    //{
                    //    vFiltro = " AND D.cod_producto LIKE '%" + filtros.filtro + "%' OR P.descripcion LIKE '%" + filtros.filtro + "%' ";
                    //}

                    //if (filtros.pagina != null)
                    //{
                    //    paginaActual = " OFFSET " + filtros.pagina + " ROWS ";
                    //    paginacionActual = " FETCH NEXT " + filtros.paginacion + " ROWS ONLY ";
                    //}

                    var query = $@"select D.cod_producto,P.descripcion,D.cantidad,D.precio,isnull(D.descuento,0) as Descuento
                                       ,D.imp_ventas, 0 as Total,
                                     
                                            CASE WHEN (
									     SELECT U.COD_PRODUCTO FROM CPR_ORDENES_UENS U WHERE U.COD_PRODUCTO = D.cod_producto
					                    AND cod_orden = D.cod_orden
                                         GROUP BY U.COD_ORDEN, U.COD_PRODUCTO
									   ) IS NOT NULL THEN 1
									   ELSE 0
									   END AS i_existe,
									    CASE WHEN (
									     	SELECT COALESCE((SELECT SUM(U.CANTIDAD)
												 FROM CPR_ORDENES_UENS U
												 WHERE U.COD_PRODUCTO = D.cod_producto
												   AND U.COD_ORDEN = D.cod_orden
                                           GROUP BY U.COD_ORDEN, U.COD_PRODUCTO), 0)
									   ) < D.cantidad THEN 0
									   ELSE 1
									   END AS i_completo

                                        from cpr_ordenes_detalle D inner join pv_productos P on D.cod_producto = P.cod_producto
                                        where D.cod_orden = '{filtros.CodOrden}' 
                                       {vFiltro} 
                                        ORDER BY D.cod_producto
                                        {paginaActual}
                                        {paginacionActual} ";
                    response.Result.lineas = connection.Query<OrdenLineas>(query).ToList();
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
        /// Orden Scroll
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="scrollValue"></param>
        /// <param name="cod_Orden"></param>
        /// <returns></returns>
        public ErrorDto<OrdenesData> Orden_scroll(int CodEmpresa, int scrollValue, string? cod_Orden)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<OrdenesData>();
            try
            {
                string filtro = "";

                if (scrollValue == 1)
                {
                    filtro = $"where cod_orden > '{cod_Orden}' order by cod_orden asc";
                }
                else
                {
                    filtro = $"where cod_orden < '{cod_Orden}'  order by cod_orden desc";
                }

                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select Top 1 cod_orden from cpr_ordenes {filtro}";
                    response.Description = connection.QueryFirstOrDefault<string>(query);
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        //Guardado y Editado nuevo

        /// <summary>
        /// Inserta Orden de Compra
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="jOrdenes"></param>
        /// <returns></returns>
        public ErrorDto Orden_Insertar(int CodEmpresa, object jOrdenes)
        {
            ErrorDto errorDto = new ErrorDto();
            errorDto.Code = 0;
            OrdenDatosAcciones ordenes = new OrdenDatosAcciones();
            try
            {
                string jsonString = jOrdenes.ToString();
                ordenes = JsonConvert.DeserializeObject<OrdenDatosAcciones>(jsonString) ?? new OrdenDatosAcciones();
            }
            catch (Exception ex)
            {
                errorDto.Code = 1;
                errorDto.Description = ex.Message;
                return errorDto;
            }
            return OrdenesGuardar(CodEmpresa, ordenes);
        }

        /// <summary>
        /// Actualiza orden de Compra
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="jOrdenes"></param>
        /// <returns></returns>
        public ErrorDto Orden_Actualiza(int CodEmpresa, OrdenDatosAcciones jOrdenes)
        {

            ErrorDto errorDto = new ErrorDto();
            errorDto.Code = 0;
            return OrdenesGuardar(CodEmpresa, jOrdenes);
        }


        /// <summary>
        /// Ordenes guardar
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="ordenes"></param>
        /// <returns></returns>
        private ErrorDto OrdenesGuardar(int CodEmpresa, OrdenDatosAcciones ordenes)
        {
            string mjs = "";
            ErrorDto errorDto = new ErrorDto();
            errorDto.Code = 0;

            mjs = fxInvVerificaLineaDetalle(CodEmpresa, ordenes.lineas, "E");

            if (mjs != "")
            {
                errorDto.Code = 1;
                errorDto.Description = mjs;
            }
            else
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                try
                {
                    float curSubTotal = 0;
                    float curDescuento = 0;
                    float curIV = 0;
                    float curCantidad = 0;
                    float curTotal = 0;

                    ordenes.lineas = sbCalculaTotales(
                        ordenes.lineas,
                        ref curSubTotal,
                        ref curDescuento,
                        ref curIV,
                        ref curCantidad,
                        ref curTotal);

                    using var connection = new SqlConnection(stringConn);
                    {
                        if (ordenes.edita)
                        {
                            if (ordenes.estado != "S")
                            {
                                errorDto.Code = 1;
                                errorDto.Description = "No puede Modificar esta Orden, ya que no se encuentra Solicitada...";
                                return errorDto;
                            }
                            else
                            {
                                var query = $@"update cpr_ordenes set 
                                        nota = '{ordenes.nota}',
                                        descuento = {curDescuento} ,
                                        subtotal = {curSubTotal} 
                                        ,imp_ventas = {curIV} ,
                                        total = {curTotal},
                                        plazo_entrega = '{ordenes.plazo_entrega}',
                                        horario_recepcion = '{ordenes.horario_recepcion}',
                                        plazo_pago = '{ordenes.plazo_pago}',
                                        direccion_entrega = '{ordenes.direccion_entrega}',
                                        garantia = '{ordenes.garantia}',
                                        terminos_condiciones = '{ordenes.terminos_condiciones}',
                                        multa = '{ordenes.multa}'
                                         where cod_orden = '{ordenes.cod_orden}'
                                        and tipo_orden = '{ordenes.tipo_orden}' ";
                                var result = connection.Execute(query);

                                //Bitacora
                                Bitacora(new BitacoraInsertarDto
                                {
                                    EmpresaId = CodEmpresa,
                                    Usuario = ordenes.usuario,
                                    DetalleMovimiento = "Modifica, Orden Compra:" + ordenes.cod_orden,
                                    Movimiento = "Modifica - WEB",
                                    Modulo = 35
                                });

                                errorDto.Description = ordenes.cod_orden;

                            }
                        }
                        else
                        {
                            string vConsecutivo = "";
                            var query = $@"select isnull(max(cod_orden),0) + 1 as Ultimo from cpr_Ordenes";
                            vConsecutivo = connection.Query<string>(query).FirstOrDefault();
                            vConsecutivo = vConsecutivo.PadLeft(10, '0');
                            query = $@"insert cpr_ordenes(
                                            cod_orden,
                                            tipo_orden,
                                            estado,
                                            genera_fecha,
                                            nota,
                                            genera_user,
                                            subtotal,
                                            descuento,
                                            imp_ventas,
                                            total,
                                            pin_autorizacion,
                                            pin_entrada,
                                            proceso,
                                            plazo_entrega,
                                            garantia,
                                            plazo_pago,
                                            direccion_entrega,
                                            horario_recepcion,
                                            terminos_condiciones,
                                            multa)values
                                            ('{vConsecutivo}', 
                                            '{ordenes.tipo_orden}',
                                            'S',
                                            getdate(),
                                            '{ordenes.nota}',
                                            '{ordenes.usuario}',
                                            {curSubTotal} ,
                                             {curDescuento},
                                             {curIV},
                                            {curTotal},
                                            0,
                                            '',
                                            'P',
                                            '{ordenes.plazo_entrega}',
                                            '{ordenes.garantia}',
                                            '{ordenes.plazo_pago}',
                                            '{ordenes.direccion_entrega}',
                                            '{ordenes.horario_recepcion}',
                                            '{ordenes.terminos_condiciones}',
                                            '{ordenes.multa}')";

                            var result = connection.Execute(query);
                            //Bitacora
                            Bitacora(new BitacoraInsertarDto
                            {
                                EmpresaId = CodEmpresa,
                                Usuario = ordenes.usuario,
                                DetalleMovimiento = "Registra, Orden Compra:" + vConsecutivo,
                                Movimiento = "Registra - WEB",
                                Modulo = 35
                            });
                            ordenes.cod_orden = vConsecutivo;
                            errorDto.Description = vConsecutivo;
                        }

                        //Guarda Detalle
                        var qDetalle = $@"delete cpr_ordenes_detalle where cod_orden = '{ordenes.cod_orden}'";
                        connection.Execute(qDetalle);

                        int linea = 0;
                        foreach (OrdenLineas item in ordenes.lineas)
                        {
                            linea++;
                            var qLinea = $@"insert cpr_ordenes_detalle(
                                                linea,
                                                cod_orden,
                                                cod_producto,
                                                cantidad,
                                                estado,
                                                precio,
                                                descuento,
                                                imp_ventas)
                                    values( {linea}, 
                                                '{ordenes.cod_orden}', 
                                                '{item.Cod_Producto}', 
                                                {item.Cantidad}, 
                                                'P', 
                                                {item.Precio}, 
                                                {item.Descuento}, 
                                                {item.Imp_Ventas}) ";

                            connection.Execute(qLinea);
                        }
                    }
                }
                catch (Exception ex)
                {
                    errorDto.Code = -1;
                    errorDto.Description = ex.Message;
                }
            }
            return errorDto;

        }

        /// <summary>
        /// /Verifica que la linea de detalle tenga el producto activo y que la bodega exista
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="vGrid"></param>
        /// <param name="vMov"></param>
        /// <param name="ColBod1"></param>
        /// <param name="ColBod2"></param>
        /// <returns></returns>
        private string fxInvVerificaLineaDetalle(int CodEmpresa, List<OrdenLineas> vGrid,
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

                    foreach (OrdenLineas item in vGrid)
                    {
                        //Verifica que el producto este activo
                        if (item.Cantidad > 0)
                        {
                            var query = $@"select estado from pv_productos where cod_producto = '{item.Cod_Producto}'";
                            var resp = connection.Query<string>(query).FirstOrDefault();
                            if (resp == null)
                            {
                                return "El producto " + item.Cod_Producto + " no existe";
                            }
                            if (resp == "I")
                            {
                                return "El producto " + item.Cod_Producto + " no esta activo";
                            }
                        }

                        //Verifica que la Bodega Exista y que Permita Registrar el Tipo de Movimiento
                        if (ColBod1 > 0)
                        {
                            var query = $@"select permite_entradas,permite_salidas,estado from pv_bodegas where cod_bodega = '{ColBod1}'";
                            List<Models.BodegaDto> exist = connection.Query<Models.BodegaDto>(query).ToList();
                            if (exist.Count == 0)
                            {
                                return "La bodega " + ColBod1 + " - No existe";
                            }
                            else
                            {
                                if (exist[0].estado == "I")
                                {
                                    return "La bodega " + ColBod1 + " - Se encuentra Inactiva";
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
                            var query = $@"select permite_entradas,permite_salidas,estado from pv_bodegas where cod_bodega = '{ColBod1}'";
                            List<Models.BodegaDto> exist = connection.Query<Models.BodegaDto>(query).ToList();
                            if (exist.Count == 0)
                            {
                                return "La bodega " + ColBod2 + " - No existe";
                            }
                            else
                            {
                                if (exist[0].estado == "I")
                                {
                                    return "La bodega " + ColBod2 + " - Se encuentra Inactiva";
                                }
                                else
                                {
                                    switch (vMov)
                                    {
                                        case "E":
                                        case "T":
                                            if (exist[0].permite_entradas != "1")
                                            {
                                                return "La bodega " + ColBod2 + " - No permite Entradas";
                                            }
                                            break;
                                        case "S":
                                        case "R":
                                            if (exist[0].permite_salidas != "1")
                                            {
                                                return "La bodega " + ColBod2 + " - No permite Salidas";
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
        /// Calcula los totales de la orden
        /// </summary>
        /// <param name="vGrid"></param>
        /// <param name="curSubTotal"></param>
        /// <param name="curDescuento"></param>
        /// <param name="curIV"></param>
        /// <param name="curCantidad"></param>
        /// <param name="curTotal"></param>
        /// <returns></returns>
        private List<OrdenLineas> sbCalculaTotales(List<OrdenLineas> vGrid
            , ref float curSubTotal, ref float curDescuento, ref float curIV, ref float curCantidad,
            ref float curTotal)
        {
            foreach (OrdenLineas item in vGrid)
            {
                curSubTotal = curSubTotal + (item.Cantidad * item.Precio);
                float curTmpDesc = ((item.Cantidad * item.Precio) * (item.Descuento / 100));
                curDescuento = curDescuento + curTmpDesc;

                float curTmpIV = (((item.Cantidad * item.Precio) - curTmpDesc) * (item.Imp_Ventas / 100));
                curIV = curIV + curTmpIV;

                item.Total = (item.Cantidad * item.Precio) - curTmpDesc + curTmpIV;
                curCantidad = curCantidad + item.Cantidad;
            }
            curTotal = curSubTotal + curIV - curDescuento;

            return vGrid;
        }


        /// <summary>
        /// Obtiene las UENs de la orden
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodOrden"></param>
        /// <param name="CodProducto"></param>
        /// <returns></returns>
        public ErrorDto<List<OrdenesUensData>> OrdenesUENs_Obtener(int CodEmpresa, string CodOrden, string CodProducto)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<OrdenesUensData>>
            {
                Code = 0
            };
            try
            {
                int existe = 0;
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"SELECT COUNT(*) FROM CPR_ORDENES_UENS where COD_ORDEN = '{CodOrden}' AND COD_PRODUCTO = '{CodProducto}'";
                    existe = connection.Query<int>(query).FirstOrDefault();

                    if (existe > 0)
                    {
                        query = $@"SELECT COD_ORDEN, COD_PRODUCTO, COD_UNIDAD, CANTIDAD, TIPO_PRODUCTO, REGISTRO_USUARIO, REGISTRO_FECHA
                FROM CPR_ORDENES_UENS where COD_ORDEN = '{CodOrden}' AND COD_PRODUCTO = '{CodProducto}'";
                    }
                    else
                    {
                        query = $@"select O.COD_ORDEN, D.COD_PRODUCTO, BS.COD_UNIDAD, BS.CANTIDAD, P.TIPO_PRODUCTO
                from cpr_ordenes O LEFT JOIN CPR_ORDENES_DETALLE D ON D.COD_ORDEN = O.COD_ORDEN
                LEFT JOIN PV_PRODUCTOS P ON P.COD_PRODUCTO = D.COD_PRODUCTO 
                LEFT JOIN CPR_SOLICITUD S ON S.ADJUDICA_PROVEEDOR = O.COD_PROVEEDOR AND S.ADJUDICA_ORDEN = O.COD_ORDEN 
                LEFT JOIN CPR_SOLICITUD_BS BS ON BS.CPR_ID = S.CPR_ID AND BS.COD_PRODUCTO = D.COD_PRODUCTO
                where O.COD_ORDEN = '{CodOrden}' AND D.COD_PRODUCTO = '{CodProducto}'";
                    }
                    response.Result = connection.Query<OrdenesUensData>(query).ToList();
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
        /// Guarda las UENs de la orden
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="lista"></param>
        /// <returns></returns>
        public ErrorDto OrdenesUENs_Guardar(int CodEmpresa, List<OrdenesUensData> lista)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    foreach (var item in lista)
                    {
                        var procedure = "[spCPR_Ordenes_UENS_Upsert]";
                        var values = new
                        {
                            cod_orden = item.cod_orden,
                            cod_producto = item.cod_producto,
                            cod_unidad = item.cod_unidad,
                            cantidad = item.cantidad,
                            usuario = item.registro_usuario
                        };

                        response.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    }

                    response.Description = "Registros agregados correctamente";
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
        /// Eliminar la UEN de la orden
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_orden"></param>
        /// <param name="cod_producto"></param>
        /// <param name="cod_unidad"></param>
        /// <returns></returns>
        public ErrorDto OrdenesUENs_Eliminar(int CodEmpresa, string cod_orden, string cod_producto, string cod_unidad)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"delete from CPR_ORDENES_UENS where COD_ORDEN = '{cod_orden}' 
                        AND COD_PRODUCTO = '{cod_producto}' AND COD_UNIDAD = '{cod_unidad}'";

                    response.Code = connection.Execute(query);
                    response.Description = "Registro eliminado correctamente";
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
        /// Obtiene los horarios de recepcion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<CprHorarioLista>> horarios_Obtener(int CodEmpresa, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<CprHorarioLista>>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select catalogo_id item, DESCRIPCION FROM 
                                        CPR_CATALOGOS_ORDENES 
                                                WHERE tipo_id = 1";
                    response.Result = connection.Query<CprHorarioLista>(query).ToList();
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
        /// Obtiene formas de pago
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<CprFormaPago>> formapago_Obtener(int CodEmpresa, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<CprFormaPago>>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select catalogo_id item, DESCRIPCION FROM 
                                        CPR_CATALOGOS_ORDENES 
                                                WHERE tipo_id = 2";
                    response.Result = connection.Query<CprFormaPago>(query).ToList();
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
        /// Notificacion de orden de compra por correo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_orden"></param>
        /// <param name="proveedor"></param>
        /// <param name="cod_proveedor"></param>
        /// <returns></returns>
        public ErrorDto CorreoNotificaOrdenCompra(int CodEmpresa, string cod_orden, string proveedor, string cod_proveedor)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0
            };
            try
            {
                CorreoNotificaOrdenCompra_Enviar(CodEmpresa, cod_orden, proveedor, cod_proveedor);


            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Envia el correo de notificacion de orden de compra
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_orden"></param>
        /// <param name="proveedor"></param>
        /// <param name="cod_proveedor"></param>
        /// <returns></returns>
        private async Task CorreoNotificaOrdenCompra_Enviar(int CodEmpresa, string cod_orden, string proveedor, string cod_proveedor)
        {

            ErrorDto resp = new ErrorDto();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<OrdenLineas> info = new List<OrdenLineas>();

            int CodCliente = CodEmpresa;

            try
            {

                using var connection = new SqlConnection(stringConn);
                {

                    var queryEmail = @$"SELECT EMAIL FROM CXP_PROVEEDORES";
                    string EmailProveedor = connection.Query<string>(queryEmail).FirstOrDefault();

                }

                EnvioCorreoModels eConfig = _envioCorreoDB.CorreoConfig(CodEmpresa, Notificaciones);
                string body = @$"<html lang=""es"">
                            <head>
                                <meta charset=""UTF-8"">
                                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                                <title>Solicitud de Cotización</title>
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
                                        <h2><strong>Notificación Orden de Compra</strong> </h2>
                                        <p>No. Orden de Compras <strong>{cod_orden}</strong> </p>
                                        <p>Proveedor: {proveedor}</p>
                                        <p>Se le adjunta la Orden de Compra</p>";


  
                List<IFormFile> Attachments = new List<IFormFile>();

                var fileBoleta = await BoletaRegistro(CodCliente, cod_orden);

                if (fileBoleta != null)
                {
                    Attachments.Add(fileBoleta); 
                }

                if (sendEmail == "Y")
                {
                    EmailRequest emailRequest = new EmailRequest();

                    emailRequest.To = EmailProveedor;
                    emailRequest.From = eConfig.User;
                    emailRequest.Subject = "Notificación de Orden de Compra";
                    emailRequest.Body = body;
                    emailRequest.Attachments = Attachments;

                    if (eConfig != null)
                    {
                        await _envioCorreoDB.SendEmailAsync(emailRequest, eConfig, resp);
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
        }



        /// <summary>
        /// Convierte el reporte a PDF
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_orden"></param>
        /// <returns></returns>
        private async Task<IFormFile> BoletaRegistro(int CodCliente, string cod_orden)
         {

            string RepServer = _config.GetSection("ReporteSrv").GetSection("ReportServer").Value.ToString();
            string baseUrl = RepServer + "/frmCprOrdenes/Compras_OrdenesBoleta";
            string jsonParam = @"{""Usuario"":""Usuario de Demostración""}";
            string jsonParamEncoded = HttpUtility.UrlEncode(jsonParam); 

            string parametros = @"?CodEmpresa=61&nombreRepotre=Compras_OrdenesBoleta&parametros=" + jsonParamEncoded + "&cod_orden=" + cod_orden;
            string fullUrl = $"{baseUrl}{parametros}";

            using (HttpClient client = new HttpClient())
            {
                IFormFile formFile;
                try
                {
                    HttpResponseMessage response = await client.GetAsync(fullUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string base64String = await response.Content.ReadAsStringAsync();

                        byte[] fileBytes = Convert.FromBase64String(base64String);
                        MemoryStream stream = new MemoryStream(fileBytes);
                        formFile = new FormFile(stream, 0, fileBytes.Length, "file", $"OrdenCompra_{cod_orden}.pdf");

                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    return null;
                }

                return formFile;
            }
        }
    }
}
