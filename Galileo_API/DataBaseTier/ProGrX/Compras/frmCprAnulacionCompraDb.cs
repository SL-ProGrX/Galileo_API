using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.CPR;
using PgxAPI.Models.ERROR;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmCprAnulacionCompraDB
    {
        private readonly IConfiguration _config;

        public frmCprAnulacionCompraDB(IConfiguration config)
        {
            _config = config;
        }


        public ErrorDTO<List<CompraDto>> Compras_Obtener(int CodEmpresa, string filtro)
        {

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<CompraDto>>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = "SELECT TOP 30 (E.Cod_Factura +' - ' + CONVERT(VARCHAR(10), E.Cod_Proveedor) ) as dataKey, E.cod_compra,E.cod_orden,E.cod_factura,P.descripcion as Proveedor " +
                        " FROM cpr_compras E inner join cxp_proveedores P on E.cod_proveedor = P.cod_proveedor" +
                        " WHERE E.cod_compra like @filtro OR E.cod_orden like @filtro OR E.cod_factura like @filtro OR P.descripcion like @filtro";

                    var parameters = new DynamicParameters();
                    parameters.Add("filtro", "%" + filtro + "%", DbType.String);


                    response.Result = connection.Query<CompraDto>(query, parameters).ToList();

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

        public ErrorDTO<CompraAnulacionDatosDto> Compra_Datos_Obtener(int CodEmpresa, string Cod_Compra)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<CompraAnulacionDatosDto>();
            response.Result = new CompraAnulacionDatosDto();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = string.Empty;
                    query = "select E.Cod_Factura, E.Cod_Proveedor, E.Cod_Orden, E.Cod_Compra, E.Estado, E.Forma_Pago, E.Fecha,"
                        + "E.genera_user,E.ANULA_FECHA,E.ANULA_FEC_AFECTA,"
                        + "E.Sub_Total, E.Notas, E.Descuento, E.Imp_Ventas, E.Imp_Consumo, E.Total, E.Cxp_Estado, E.Asiento_Estado,"
                        + "E.Asiento_Fecha, (rtrim(C.Tipo_Orden) + ' - ' + C.descripcion) as Causa "
                        + ",P.descripcion as Proveedor,O.nota"
                        + " from cpr_ordenes O inner join cpr_Tipo_Orden C on O.Tipo_Orden = C.Tipo_Orden"
                        + " inner join cpr_compras E on O.cod_orden = E.cod_orden"
                        + " inner join cxp_proveedores P on E.cod_proveedor = P.cod_proveedor"
                        + " where E.cod_compra = @codigoCompra";

                    var parameters = new DynamicParameters();
                    parameters.Add("codigoCompra", Cod_Compra, DbType.String);

                    response.Result = connection.Query<CompraAnulacionDatosDto>(query, parameters).FirstOrDefault();
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

        public ErrorDTO Compra_Anular(int CodEmpresa, CompraAnulacionDto compraDto)
        {
            ErrorDTO ErrorDTO = new ErrorDTO();
            ErrorDTO result = new ErrorDTO();

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                var query = string.Empty;

                using var connection = new SqlConnection(stringConn);
                {
                    //Inicia Proceso de Anulacion
                    if (compraDto.Forma_Pago == "CR")
                    {
                        var parameters = new
                        {
                            CodProv = compraDto.Cod_Proveedor
                        };

                        query = "update cxp_proveedores set saldo = isnull(saldo, 0) - @Total where cod_proveedor = @CodProv";

                        connection.Execute(query, parameters);
                    }

                    var parameters2 = new
                    {
                        Fecha = compraDto.Fecha,
                        Usuario = "",
                        CodCompra = compraDto.Cod_Compra
                    };

                    query = "update cpr_compras set estado = 'A', anula_fecha = Getdate(), anula_fec_afecta = @Fecha, anula_user = @Usuario where cod_compra = @CodCompra";

                    connection.Execute(query, parameters2);


                    if (compraDto.Cxp_Estado == "G")
                    {
                        //Calcula Monto Programado Bruto, pagado
                        var parameters3 = new
                        {
                            CodProveedor = compraDto.Cod_Proveedor,
                            CodFactura = compraDto.Cod_Factura
                        };

                        query = "select isnull(sum(monto), 0) as MontoX from cxp_pagoProv where cod_proveedor = @CodProveedor and cod_factura = @CodFactura and tesoreria is not null";

                        var dapperinfo = connection.Query(query, parameters3).FirstOrDefault();


                        decimal curMonto = (decimal)dapperinfo.MontoX;

                        if (compraDto.Forma_Pago == "CR")
                        {
                            //Genera Cargo Periodico x el monto Programado (Bruto), pagado
                            var parameters4 = new
                            {
                                CodProveedor = compraDto.Cod_Proveedor
                            };

                            query = "select isnull(max(ID),0) as ultimo from cxp_cargosper where cod_proveedor = @CodProveedor";

                            int ultimo = connection.Query(query, parameters4).FirstOrDefault();

                            ultimo = ultimo + 1;

                            if (curMonto > 0)
                            {
                                query = "insert cxp_cargosper(id, cod_proveedor, cod_cargo, tipo, valor, vence, saldo, concepto, detalle, recaudado)";
                                query += "values(@IdUltimo, @CodProveedor, @CodCargo, @Tipo, @Valor, @Vence, @Saldo, @Concepto, @Detalle, @Recaudado)";

                                var parameters5 = new
                                {
                                    IdUltimo = ultimo,
                                    CodProveedor = compraDto.Cod_Proveedor,
                                    CodCargo = "",
                                    Tipo = 'M',
                                    Valor = curMonto,
                                    Vence = new DateTime(),
                                    Saldo = curMonto,
                                    Concepto = "ANULACION DE FACTURA DE COMPRA",
                                    Detalle = "FACTURA : " + compraDto.Cod_Factura,
                                    Recaudado = 0
                                };
                                connection.Execute(query, parameters5);
                            }
                        }

                        //Elimina Programacion Pendiente de Pago de la Factura
                        query = "delete cxp_pagoProv where cod_proveedor = @CodProveedor and cod_factura = @CodFactura and tesoreria is null";
                        var parameters6 = new
                        {
                            CodProveedor = compraDto.Cod_Proveedor,
                            CodFactura = compraDto.Cod_Factura
                        };

                        connection.Execute(query, parameters6);

                    }

                    //Reversa inventario
                    query = "select * from cpr_compras_detalle where cod_factura = @codFactura and cod_proveedor = @codProveedor";
                    var parameters7 = new
                    {
                        codProveedor = compraDto.Cod_Proveedor,
                        codFactura = compraDto.Cod_Factura
                    };
                    List<CompraDetalleDto> compDetalleList = connection.Query<CompraDetalleDto>(query, parameters7).ToList();


                    foreach (var item in compDetalleList)
                    {
                        var comp = new CompraInventarioDTO();
                        comp.CodProducto = item.Cod_Producto;
                        comp.Cantidad = item.Cantidad;
                        comp.CodBodega = item.Cod_Bodega;
                        comp.CodTipo = compraDto.Cod_Compra;
                        comp.Origen = "Compra.Anu";
                        comp.Fecha = compraDto.Fecha.ToString();
                        comp.Precio = item.Precio;
                        comp.ImpConsumo = item.Imp_Consumo;
                        comp.ImpVentas = item.Imp_Ventas;
                        comp.Usuario = "S";

                        result = new mProGrX_AuxiliarDB(_config).sbInvInventario(CodEmpresa, comp);
                        if (result.Code == -1)
                        {
                            ErrorDTO.Code = -1;
                            ErrorDTO.Description = result.Description;
                            break;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                ErrorDTO.Code = -1;
                ErrorDTO.Description = ex.Message;
            }

            return ErrorDTO;
        }

        public ErrorDTO<List<CompraDetalleDto>> CompraDetalles_Obtener(int CodEmpresa, string Cod_Factura)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<CompraDetalleDto>>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //var query = "select * from cpr_compras_detalle where cod_factura = @cod_factura";
                    var query = "select D.*, B.DESCRIPCION AS BODEGA, P.DESCRIPCION AS PRODUCTO from cpr_compras_detalle D  INNER JOIN PV_BODEGAS B ON B.COD_BODEGA = D.COD_BODEGA " +
                        "INNER JOIN PV_PRODUCTOS P ON P.COD_PRODUCTO = D.COD_PRODUCTO where D.cod_factura =@cod_factura";

                    var parameters = new DynamicParameters();
                    parameters.Add("cod_factura", Cod_Factura, DbType.String);

                    response.Result = connection.Query<CompraDetalleDto>(query, parameters).ToList();

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

        public ErrorDTO<CompraAnulacionDto> Compra_Obtener(int CodEmpresa, string codCompra)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<CompraAnulacionDto>();
            response.Result = new CompraAnulacionDto();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = string.Empty;
                    query = "select Cod_Factura, Cod_Proveedor, Cod_Orden, Cod_Compra, Estado, Forma_Pago, Fecha, Sub_Total, Notas, Descuento," +
                        " Imp_Ventas, Imp_Consumo, Total, Cxp_Estado, Asiento_Estado, Asiento_Fecha from cpr_compras where cod_compra = @pCodCompra";

                    var parameters = new DynamicParameters();
                    parameters.Add("pCodCompra", codCompra, DbType.String);

                    response.Result = connection.Query<CompraAnulacionDto>(query, parameters).FirstOrDefault();
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

        public ErrorDTO Compra_Anular1(int CodEmpresa, CompraAnulacionDto compraDto)
        {
            ErrorDTO ErrorDTO = new ErrorDTO();

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                var query = string.Empty;

                using var connection = new SqlConnection(stringConn);
                {
                    if (compraDto.Forma_Pago == "CR")
                    {
                        var parameters = new
                        {
                            CodProv = compraDto.Cod_Proveedor
                        };

                        query = "update cxp_proveedores set saldo = isnull(saldo, 0) - @Total where cod_proveedor = @CodProv";

                        connection.Execute(query, parameters);

                        var parameters2 = new
                        {
                            Fecha = compraDto.Fecha,
                            Usuario = "",
                            CodCompra = compraDto.Cod_Compra
                        };

                        query = "update cpr_compras set estado = 'A', anula_fecha = Getdate(), anula_fec_afecta = @Fecha, anula_user = @Usuario where cod_compra = @CodCompra";

                        connection.Execute(query, parameters2);

                        if (compraDto.Cxp_Estado == "G")
                        {

                            var parameters3 = new
                            {
                                CodProveedor = compraDto.Cod_Proveedor,
                                CodFactura = compraDto.Cod_Factura
                            };

                            query = "select isnull(sum(monto), 0) as MontoX from cxp_pagoProv where cod_proveedor = @CodProveedor and cod_factura = @CodFactura and tesoreria is not null";

                            int curMonto = connection.Query(query, parameters3).FirstOrDefault();

                            if (compraDto.Forma_Pago == "CR")
                            {
                                var parameters4 = new
                                {
                                    CodProveedor = compraDto.Cod_Proveedor
                                };

                                query = "select isnull(max(ID),0) as ultimo from cxp_cargosper where cod_proveedor = @CodProveedor";

                                int ultimo = connection.Query(query, parameters4).FirstOrDefault();

                                ultimo = ultimo + 1;

                                if (curMonto > 0)
                                {
                                    query = "insert cxp_cargosper(id, cod_proveedor, cod_cargo, tipo, valor, vence, saldo, concepto, detalle, recaudado)";
                                    query += "values(@IdUltimo, @CodProveedor, @CodCargo, @Tipo, @Valor, @Vence, @Saldo, @Concepto, @Detalle, @Recaudado)";

                                    var parameters5 = new
                                    {
                                        IdUltimo = ultimo,
                                        CodProveedor = compraDto.Cod_Proveedor,
                                        CodCargo = "",
                                        Tipo = 'M',
                                        Valor = curMonto,
                                        Vence = new DateTime(),
                                        Saldo = curMonto,
                                        Concepto = "ANULACION DE FACTURA DE COMPRA",
                                        Detalle = "FACTURA : " + compraDto.Cod_Factura,
                                        Recaudado = 0
                                    };
                                    connection.Execute(query, parameters5);
                                }
                            }
                            //Elimina Programacion Pendiente de Pago de la Factura
                            query = "delete cxp_pagoProv where cod_proveedor = @CodProveedor and cod_factura = @CodFactura and tesoreria is null";
                            var parameters6 = new
                            {

                                CodProveedor = compraDto.Cod_Proveedor,
                                CodFactura = compraDto.Cod_Factura
                            };

                            connection.Execute(query, parameters6);

                            //Reversa inventario
                            query = "select * from cpr_compras_detalle where cod_factura = @codFactura and cod_proveedor = @codProveedor";
                            List<CompraDetalleDto> compDetalleList = connection.Query<CompraDetalleDto>(query).ToList();

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorDTO.Code = -1;
                ErrorDTO.Description = ex.Message;
            }

            return ErrorDTO;
        }

        public ErrorDTO<CompraAnulacionDatosDto> Compra_Anulacion_Datos_Obtener(int CodEmpresa, CompraAnulacionDatosRequestDto compraAnulacionDatosRequestDto)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDTO<CompraAnulacionDatosDto>();
            response.Result = new CompraAnulacionDatosDto();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = string.Empty;
                    query = "select E.Cod_Factura, E.Cod_Proveedor, E.Cod_Orden, E.Cod_Compra, E.Estado, E.Forma_Pago, E.Fecha,"
                        + "E.Sub_Total, E.Notas, E.Descuento, E.Imp_Ventas, E.Imp_Consumo, E.Total, E.Cxp_Estado, E.Asiento_Estado,"
                        + "E.Asiento_Fecha, (rtrim(C.Tipo_Orden) + ' - ' + C.descripcion) as Causa "
                        + ",P.descripcion as Proveedor,O.nota"
                        + " from cpr_ordenes O inner join cpr_Tipo_Orden C on O.Tipo_Orden = C.Tipo_Orden"
                        + " inner join cpr_compras E on O.cod_orden = E.cod_orden"
                        + " inner join cxp_proveedores P on E.cod_proveedor = P.cod_proveedor"
                        + " where E.cod_compra = @codigoCompra";

                    var parameters = new DynamicParameters();
                    parameters.Add("codigoCompra", compraAnulacionDatosRequestDto.codigoCompra, DbType.String);


                    if (!String.IsNullOrEmpty(compraAnulacionDatosRequestDto.codigoOrden))
                    {
                        query += "and E.cod_orden = @codigoOrden";
                        parameters.Add("codigoOrden", compraAnulacionDatosRequestDto.codigoOrden, DbType.String);
                    }

                    else if (!String.IsNullOrEmpty(compraAnulacionDatosRequestDto.codigoProveedor))
                    {
                        query += "and E.cod_proveedor = @codigoProveedor";
                        parameters.Add("codigoProveedor", compraAnulacionDatosRequestDto.codigoProveedor, DbType.String);
                    }

                    response.Result = connection.Query<CompraAnulacionDatosDto>(query, parameters).FirstOrDefault();
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




    }
}
