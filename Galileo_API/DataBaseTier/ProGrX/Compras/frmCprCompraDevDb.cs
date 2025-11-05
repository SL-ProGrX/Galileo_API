using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.CPR;
using PgxAPI.Models.ERROR;

namespace PgxAPI.DataBaseTier
{
    public class frmCprCompraDevDB
    {
        private readonly IConfiguration _config;
        mProGrX_AuxiliarDB mProGrxAuxiliar;
        mSecurityMainDb DBBitacora;
        mComprasDB mComprasDB;
        public string sendEmail = "";
        public string TestMail = "";
        public string Notificaciones = "";
        private readonly EnvioCorreoDB _envioCorreoDB;
        public frmCprCompraDevDB(IConfiguration config)
        {
            _config = config;
            DBBitacora = new mSecurityMainDb(config);
            _envioCorreoDB = new EnvioCorreoDB(_config);
            mProGrxAuxiliar = new mProGrX_AuxiliarDB(config);
            mComprasDB = new mComprasDB(config);
            sendEmail = _config.GetSection("AppSettings").GetSection("EnviaEmail").Value.ToString();
            TestMail = _config.GetSection("AppSettings").GetSection("TestEmail").Value.ToString();
            Notificaciones = _config.GetSection("AppSettings").GetSection("Notificaciones").Value.ToString();
        }

        public ErrorDto Bitacora(BitacoraInsertarDTO data)
        {
            return DBBitacora.Bitacora(data);
        }

        public ErrorDto<List<FacturasDto>> ObtenerListaFacturas(int CodEmpresa, int CodProveedor)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<FacturasDto>>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select E.cod_factura,P.descripcion as Proveedor,E.total
                                    from cpr_compras E inner join cxp_Proveedores P on E.cod_proveedor = P.cod_proveedor 
                                    and E.cod_proveedor =  {CodProveedor}";
                    response.Result = connection.Query<FacturasDto>(query).ToList();
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

        public ErrorDto<FacturaDto> ObtenerFactura(int CodEmpresa, string CodFactura, int CodProveedor)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<FacturaDto>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select E.*,P.descripcion as Proveedor
                                    from cpr_compras E inner join cxp_Proveedores P on E.cod_proveedor = P.cod_proveedor
                                    where E.cod_factura = '{CodFactura}' and E.cod_proveedor = {CodProveedor}";

                    response.Result = connection.QueryFirstOrDefault<FacturaDto>(query);
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        public ErrorDto<List<FacturaDetalleDto>> ObtenerFacturaDetalle(int CodEmpresa, string CodFactura, int CodProveedor)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<FacturaDetalleDto>>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select D.cod_producto,P.descripcion,(D.cantidad - isnull(D.cantidad_devuelta,0)) as Cantidad
                                     ,D.cod_bodega,D.precio,D.imp_ventas,(((D.cantidad - isnull(D.cantidad_devuelta,0)) * D.precio)
                                      * ((D.imp_ventas / 100) + 1)) as Total
                                      from cpr_compras_detalle D inner join pv_productos P on D.cod_producto = P.cod_producto
                                      where D.cod_factura = '{CodFactura}' and D.cod_proveedor = {CodProveedor}
                                      order by D.Linea";
                    response.Result = connection.Query<FacturaDetalleDto>(query).ToList();
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

        public ErrorDto<List<BodegaDto>> ObtenerBodegas(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<BodegaDto>>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select cod_bodega,descripcion from pv_bodegas where permite_salidas = 1";
                    response.Result = connection.Query<BodegaDto>(query).ToList();
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

        public ErrorDto VerificaFactura(int CodEmpresa, string CodFactura, int CodProveedor)
        {
            ErrorDto response = new ErrorDto();
            string vNum = "";
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            response.Code = 1;
            //Verificar si existen posteriores Cerrados
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select estado from cpr_compras where cod_factura = {CodFactura}  and cod_proveedor = {CodProveedor}  and estado in('P','D')";
                    vNum = connection.Query<string>(query).FirstOrDefault();
                    if (vNum == null)
                    {
                        response.Code = 0;
                        response.Description = " - No se encontró registro de la factura, o se encuentra Anulada, verifique...";
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

        public ErrorDto<DevolucionData> Devolucion_Obtener(int CodEmpresa, string CodDevolucion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<DevolucionData>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select D.*,P.descripcion as Proveedor, rtrim(C.cod_cargo) + ' - ' + rtrim(C.descripcion) as CargoX
                                    from cpr_compras_dev D inner join cxp_Proveedores P on D.cod_proveedor = P.cod_proveedor
                                    inner join cxp_cargos C on D.cod_cargo = C.cod_cargo
                                    where D.cod_compra_dev = '{CodDevolucion}'";
                    response.Result = connection.Query<DevolucionData>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        public ErrorDto<List<FacturaDetalleDto>> ObtenerDevolucionDetalle(int CodEmpresa, string CodDevolucion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<FacturaDetalleDto> resp = new List<FacturaDetalleDto>();
            var response = new ErrorDto<List<FacturaDetalleDto>>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select D.cod_producto,P.descripcion,D.cantidad,D.cod_bodega,D.precio,D.imp_ventas,
                                     (D.cantidad * D.precio) + (D.cantidad * D.precio * (D.imp_ventas / 100)) as Total
                                      from cpr_compra_devDet D inner join pv_productos P on D.cod_producto = P.cod_producto
                                      where D.cod_compra_dev = '{CodDevolucion}' order by D.Linea";
                    response.Result = connection.Query<FacturaDetalleDto>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        public ErrorDto<FacturaDto> ObtenerOrdenCompraDev(int CodEmpresa, string CodFactura, int CodProveedor)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<FacturaDto>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select cod_orden,cod_compra,fecha from cpr_compras where 
                                    cod_factura = '{CodFactura}' and cod_proveedor = {CodProveedor}";
                    response.Result = connection.Query<FacturaDto>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        //Nuevo Metodos
        public ErrorDto Devolucion_Guardar(int CodEmpresa, DevolucionInsert orden)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto result = new ErrorDto();
            result.Code = 0;
            try
            {

             

                string mensaje = "";
                bool valido = fxValida(CodEmpresa, orden, ref mensaje);

                if (mensaje.Length > 0)
                {
                    result.Code = -1;
                    result.Description = mensaje;
                    return result;
                }

                string vDevolucion = "";
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select isnull(max(cod_compra_dev),0) + 1 as Ultimo from cpr_compras_dev";
                    var resp = connection.Query<string>(query).FirstOrDefault();
                    resp = resp.PadLeft(10, '0');
                    vDevolucion = resp.ToString();

                    result.Description = vDevolucion;


                    query = $@"insert cpr_compras_dev(cod_compra_dev,cod_factura,cod_proveedor,fecha,sub_total,descuento,imp_ventas
                                  ,imp_consumo,total,notas,asiento_estado,genera_user,genera_fecha,cod_cargo) values
                                    ('{vDevolucion}','{orden.cod_factura}', {orden.cod_proveedor} ,Getdate(),
                                      {orden.sub_total},{orden.descuento},{orden.imp_ventas},0,
                                      {orden.total},'{orden.notas}','P','{orden.usuario}',Getdate(),'{orden.cargo}')";
                    var insert = connection.Execute(query);
                    if (insert < 0)
                    {
                        result.Code = -1;
                        result.Description += " - Error al registrar la devolución...";
                    }

                    //Bitacora
                    Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = orden.usuario,
                        DetalleMovimiento = "Registra, Devolucion Fact Compra:" + orden.cod_factura + " Dev " + vDevolucion,
                        Movimiento = "Registra - WEB",
                        Modulo = 35
                    });

                    //Guardo detalle de la orden
                    query = $@"delete cpr_compra_devDet where cod_compra_dev = '{vDevolucion}' ";
                    var delete = connection.Execute(query);

                    int count = 0;
                    foreach (FacturaDetalleDto item in orden.lineas)
                    {
                        count++;
                        //query = $@"select cantidad from cpr_compras_detalle
                        //                            where linea = {count} 
                        //                             and cod_factura = '{orden.cod_factura}' and cod_proveedor = {orden.cod_proveedor} ";
                        //var cantidad = connection.Query<int>(query).FirstOrDefault();

                        //cantidad = cantidad - item.cantidad;

                        //query = $@"update cpr_compras_detalle set cantidad_devuelta = isnull(cantidad_devuelta,0) + {item.cantidad}  where linea = {count} 
                        //                 and cod_factura = '{codigo}' and cod_proveedor = {codProveedor} ";

                        query = $@" insert cpr_compra_devDet(linea,cod_compra_dev,cod_producto,cantidad,cod_bodega 
                                     ,precio,imp_ventas,imp_consumo) 
                                        values({count}, '{vDevolucion}', '{item.cod_producto}', {item.cantidad}, '{item.cod_bodega}' 
                                     , {item.precio}, {item.imp_ventas}, {0})";
                        connection.Execute(query);

                        //Actualizar Aqui el Inventario y la Factura
                        query = $@"update cpr_compras_detalle set cantidad_devuelta = 
                                        isnull(cantidad_devuelta,0) + {item.cantidad}  
                                        where linea =  {count}
                                        and cod_factura = '{orden.cod_factura}'
                                        and cod_proveedor = {orden.cod_proveedor} ";
                        var update = connection.Execute(query);

                        CompraInventarioDTO inventarioDTO = new CompraInventarioDTO
                        {
                            CodProducto = item.cod_producto,
                            Cantidad = item.cantidad,
                            CodBodega = item.cod_bodega,
                            CodTipo = vDevolucion,
                            Origen = "Compra.Dev",
                            Fecha = orden.fecha,
                            Precio = Convert.ToDecimal(item.precio),
                            ImpConsumo = 0,
                            ImpVentas = Convert.ToDecimal(item.imp_ventas),
                            TipoMov = "D",
                            Usuario = orden.usuario
                        };
                        ErrorDto inv = mProGrxAuxiliar.sbInvInventario(
                            CodEmpresa,
                            inventarioDTO);
                    }

                    //Crear Cargo Flotante por el Monto de la Devolucion
                    query = $@"select isnull(max(ID),0) as ultimo from cxp_cargosper where cod_proveedor = {orden.cod_proveedor}";
                    int? vNum = connection.Query<int?>(query).FirstOrDefault();
                    if (vNum.HasValue)
                    {
                        vNum++;

                        string detalle = $"FACTURA : {orden.cod_factura} \nUSUARIO : {orden.usuario}";

                        query = $@"insert cxp_cargosper(id,cod_proveedor,cod_cargo,tipo,valor,vence,saldo,concepto,detalle,recaudado) 
                                    values({vNum},{orden.cod_proveedor},'{orden.cargo}','M',{orden.total},Getdate(),{orden.total},
                                          'DEVOLUCION MERCADERIA - FACTURA DE COMPRA', '{detalle}', 0 )";
                        int resp1 = connection.Execute(query);

                        if (resp1 < 0)
                        {

                        }
                        else
                        {
                            query = $@"update cxp_proveedores set saldo = isnull(saldo,0) - {orden.total}  where cod_proveedor = {orden.cod_proveedor} ";
                            int resp2 = connection.Execute(query);

                            if (resp2 < 0)
                            {
                                result.Code = -1;
                                result.Description += " - Error en actualizacion de saldo...";
                            }
                        }
                    }

                    query = $@"update cpr_compras set cxp_estado = 'D' where cod_factura = '{orden.cod_factura}' and cod_proveedor = {orden.cod_proveedor}";
                    var qestado = connection.Execute(query);
                    if (qestado < 0)
                    {
                        result.Code = -1;
                        result.Description = " - Error al actualizar el estado de la factura...";
                    }

                    CorreoNotificacionDevolucion_Enviar(CodEmpresa, orden);
                }


            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result;
        }

        private bool fxValida(int CodEmpresa, DevolucionInsert orden, ref string mensaje)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            bool result = true;
            try
            {
                if (orden.cod_factura == null)
                {
                    mensaje = " - Devolucion no puede ser nula";
                    result = false;
                }
                else
                {
                    if (orden.cod_factura.Length == 0)
                    {
                        mensaje = " - Devolucion no puede ser nula";
                        result = false;
                    }
                }

                mensaje += fxInvVerificaLineaDetalle(CodEmpresa, orden.lineas, "S");

                if (mProGrxAuxiliar.fxInvPeriodos(CodEmpresa, orden.fecha))
                {
                    mensaje += " - El Periodo del Movimiento no es válido ...";
                }

                if (mensaje.Length > 0)
                {
                    result = false;
                }

                //Verifiqua que exista la factura y que no se encuentre anulada
                if (orden.cod_proveedor > 0)
                {
                    using var connection = new SqlConnection(stringConn);
                    {
                        var query = $@"select estado from cpr_compras where cod_factura = '{orden.cod_factura}' 
                                   and cod_proveedor = '{orden.cod_proveedor}'  and estado in('P','D')";
                        var resp = connection.Query<string>(query).FirstOrDefault();

                        if (resp == null)
                        {
                            mensaje += " - No se encontró registro de la factura, o se encuentra Anulada, verifique...";
                            result = false;
                        }
                    }
                }
                else
                {
                    mensaje += " - El codigo del Proveedor no es válido, verifique...";
                }

                //Verifica que las cantidades de las devoluciones no sean mayores al original pendiente
                int i = 0;
                foreach (FacturaDetalleDto linea in orden.lineas)
                {
                    i++;
                    using var connection = new SqlConnection(stringConn);
                    {
                        var query = $@"SELECT 
                                        CASE 
                                            WHEN cantidad > {linea.cantidad} THEN 'Menor'
                                            WHEN cantidad < {linea.cantidad} THEN 'Mayor'
                                            ELSE 'Igual'
                                        END AS Comparacion
                                    FROM cpr_compras_detalle
                                    where cod_factura = '{orden.cod_factura}' and 
                                          cod_proveedor = {orden.cod_proveedor} and 
                                          cod_producto = '{linea.cod_producto}' and 
                                          linea =  {i} ";

                        string resp = connection.Query<string>(query).FirstOrDefault();
                        if (resp == "Mayor")
                        {
                            mensaje += " - Las Cantidad devoluciones en la Linea " + i + ", es mayor al remanente...";
                        }
                    }
                }

                if (orden.total == 0)
                {
                    mensaje += " - El Total de la Devolución no puede ser 0...";
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

        private string fxInvVerificaLineaDetalle(int CodEmpresa, List<FacturaDetalleDto> vGrid,
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

                    foreach (FacturaDetalleDto item in vGrid)
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


        public async Task<ErrorDto> CorreoNotificacionDevolucion_Enviar(int CodCliente, DevolucionInsert orden)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;
            EnvioCorreoModels eConfig = new();
            string emailCobros = "";
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                eConfig = _envioCorreoDB.CorreoConfig(CodCliente, "1");


                    var query = $@"SELECT DESCRIPCION FROM CXP_PROVEEDORES WHERE COD_PROVEEDOR = {orden.cod_proveedor}";
                    string proveedor = connection.QueryFirstOrDefault<string>(query);

                    var query2 = $@"SELECT email FROM CXP_PROVEEDORES WHERE COD_PROVEEDOR = {orden.cod_proveedor}";
                    string email = connection.QueryFirstOrDefault<string>(query);


                    string body = @$"<html lang=""es"">
                                    <head>
                                        <meta charset=""UTF-8"">
                                        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                                        <title>Prueba de correo</title>
                                    </head>
                                    <body>
                                        <p>Estimado Proveedor: {proveedor} </p>
                                            <p></p>
                                        <p>Se le comunica la devolución de la mercadería de la factura #{orden.cod_factura} </p>
                                         <p>Debido a {orden.notas}</p>
                                        <p>ASECCSS</p>
                                    </body>
                                    </html>";

                List<IFormFile> Attachments = new List<IFormFile>();

                //var file = ConvertByteArrayToIFormFileList(parametros.filecontent, parametros.filename);

                //Attachments.AddRange(file);

                if (sendEmail == "Y")
                {
                    EmailRequest emailRequest = new EmailRequest();

                    emailRequest.To = email;
                    emailRequest.From = eConfig.User;
                    emailRequest.Subject = "Devolución de Solicitud de Compra";
                    emailRequest.Body = body;
                    //emailRequest.Attachments = Attachments;

                    if (eConfig != null)
                    {
                        await _envioCorreoDB.SendEmailAsync(emailRequest, eConfig, info);
                    }

                }

            }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }
    }
}
