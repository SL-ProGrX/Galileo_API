using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.CxP;
using PgxAPI.Models.ERROR;

namespace PgxAPI.DataBaseTier
{
    public class frmCxPControlProgramacionDB
    {
        private readonly IConfiguration _config;

        public frmCxPControlProgramacionDB(IConfiguration config)
        {
            _config = config;
        }


        public ErrorDTO<ProgramacionPagoLista> PagosFactura_Obtener(int CodEmpresa, int? pagina, int? paginacion, string? filtro, ConsultaPagosParam param)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDTO<ProgramacionPagoLista>
            {
                Code = 0,
                Result = new ProgramacionPagoLista()
            };
            response.Result.Total = 0;
            try
            {
                var query = "";
                var filtroQ = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);
                {

                    //Busco Total
                    switch (param.Estado)
                    {
                        case "P":
                            query = $@"SELECT COUNT (cod_factura) 
                                 FROM vCxP_ProgramacionPago  where cxp_estado = 'P' ";
                            response.Result.Total = connection.Query<int>(query).FirstOrDefault();
                            break;
                        case "G":
                            query = $@"SELECT COUNT (cod_factura)  
                                 FROM vCxP_ProgramacionPago  where cxp_estado = 'G' ";
                            response.Result.Total = connection.Query<int>(query).FirstOrDefault();
                            break;
                        default:
                            query = $@"SELECT COUNT (cod_factura) 
                                 FROM vCxP_ProgramacionPago";
                            response.Result.Total = connection.Query<int>(query).FirstOrDefault();
                            break;
                    }


                    // Check if param.Estado is not empty
                    if (param.Estado != "")
                    {
                        filtroQ = " WHERE cxp_estado = '" + param.Estado + "'";
                    }

                    // Append filtro conditions if filtro is not null
                    if (filtro != null)
                    {
                        // If filtroQ is already set (due to param.Estado), append with AND, otherwise use WHERE
                        string filtroCondition = " (cod_factura LIKE '%" + filtro + "%' " +
                                                 "OR cod_proveedor LIKE '%" + filtro + "%' " +
                                                 "OR proveedor LIKE '%" + filtro + "%' " +
                                                 "OR cod_divisa LIKE '%" + filtro + "%')";

                        if (filtroQ != "")
                        {
                            filtroQ += " AND " + filtroCondition;
                        }
                        else
                        {
                            filtroQ = " WHERE " + filtroCondition;
                        }
                    }

                    // Check if param.Forma_Pago is not empty and append accordingly
                    if (param.Forma_Pago != "" && param.Forma_Pago != "T")
                    {
                        // If filtroQ is already set, append with AND, otherwise use WHERE
                        if (filtroQ != "")
                        {
                            filtroQ += " AND forma_pago = '" + param.Forma_Pago + "'";
                        }
                        else
                        {
                            filtroQ = " WHERE forma_pago = '" + param.Forma_Pago + "'";
                        }
                    }

                    if (pagina != null)
                    {
                        paginaActual = " OFFSET " + pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
                    }

                    query = $@"SELECT cod_proveedor,cod_Factura,total,CxP_Estado,fecha,tipo,fecha_ingreso,Proveedor,forma_pago,cod_divisa,tipo_cambio,Vence,IMPORTE_DIVISA_REAL
                             FROM vCxP_ProgramacionPago 
                            {filtroQ} 
                            ORDER BY FECHA desc
                            {paginaActual}
                            {paginacionActual} ";


                    response.Result.FacturasPago = connection.Query<ProgramacionPagoDTO>(query).ToList();

                    foreach (ProgramacionPagoDTO ft in response.Result.FacturasPago)
                    {
                        ft.DataKey = ft.Cod_Factura + '-' + ft.Cod_Proveedor;
                    }

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

        public ErrorDTO<List<CargoAdicional>> CargosAdicionales_Obtener(int CodEmpresa)
        {
            var response = new ErrorDTO<List<CargoAdicional>>();

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            mCntLinkDB obj = new mCntLinkDB(_config);

            string query = "SELECT cod_Cargo,descripcion,0 AS Monto FROM cxp_cargos WHERE Activo = 1";

            try
            {
                using var connection = new SqlConnection(stringConn);
                response.Result = connection.Query<CargoAdicional>(query).ToList();

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }

            return response;
        }

        public ErrorDTO<SaldosInformacion> DetalleSaldos_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDTO<SaldosInformacion>();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT 
                                C.CREDITO_PLAZO,
                                dbo.fxCxPSaldoCorte(C.cod_proveedor, Getdate()) AS 'SALDO',
                                ISNULL(SUM(P.monto), 0) AS SaldoFactura
                            FROM 
                                CXP_PROVEEDORES C
                            LEFT JOIN 
                                cxp_pagoprov P ON C.cod_proveedor = P.cod_proveedor AND P.cod_factura = '{Cod_Factura}' AND P.tesoreria IS NULL
                            WHERE 
                                C.cod_proveedor = {Cod_Proveedor}
                            GROUP BY 
                                C.CREDITO_PLAZO, 
                                C.cod_proveedor";

                    response.Result = connection.Query<SaldosInformacion>(query).FirstOrDefault();

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

        public ErrorDTO<FacturaDatos> CompraDatos_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDTO<FacturaDatos>();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT CxP_Estado,Total,Imp_ventas FROM CPR_COMPRAS WHERE cod_factura = '{Cod_Factura}'
                                    AND cod_proveedor = {Cod_Proveedor}";

                    response.Result = connection.Query<FacturaDatos>(query).FirstOrDefault();

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

        public ErrorDTO<FacturaDatos> FacturaDatos_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDTO<FacturaDatos>();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT CxP_Estado,Total, ISNULL(impuesto_Ventas,0) AS 'Imp_ventas' 
                                FROM cxp_facturas 
                                WHERE cod_factura = '{Cod_Factura}' AND cod_proveedor = {Cod_Proveedor}";

                    response.Result = connection.Query<FacturaDatos>(query).FirstOrDefault();

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

        public ErrorDTO<List<DetallePago>> DetallePagos_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            var response = new ErrorDTO<List<DetallePago>>();

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            string query = $@"SELECT P.NPago, P.Cod_Factura, P.Cod_Proveedor, ISNULL(SUM(C.monto), 0) AS Cargo,
			                P.Monto, (P.monto - ISNULL(SUM(C.monto), 0)) AS Neto, ISNULL(P.Tesoreria, 0) AS Tesoreria,
			                P.fecha_Vencimiento, P.importe_divisa_real, P.cod_divisa, P.tipo_Cambio, P.forma_pago 
                            FROM cxp_pagoprov P
                            LEFT JOIN cxp_pagoProvCargos C ON P.npago = C.npago
								                            AND P.cod_factura = C.cod_factura 
								                            AND P.cod_proveedor = C.cod_proveedor                            

                            WHERE P.cod_factura = '{Cod_Factura}' AND P.cod_proveedor = {Cod_Proveedor}
                            GROUP BY P.NPago, P.Cod_Factura, P.Cod_Proveedor, P.Monto, P.Tesoreria, 
			                            P.fecha_Vencimiento, P.importe_divisa_real, P.cod_divisa, P.tipo_Cambio, P.forma_pago
			                            ORDER BY P.NPago";

            try
            {
                using var connection = new SqlConnection(stringConn);
                response.Result = connection.Query<DetallePago>(query).ToList();

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }

            return response;

        }

        public ErrorDTO<TesoreriaDetalle> TesoreriaDetalle_Obtener(int CodEmpresa, int Tesoreria)
        {
            var response = new ErrorDTO<TesoreriaDetalle>();

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            string query = $@"SELECT C.estado, C.tipo, B.descripcion, C.beneficiario, C.monto
                            FROM Tes_Transacciones AS C INNER JOIN Tes_Bancos AS B ON C.id_banco = B.id_banco
                            WHERE C.Nsolicitud = {Tesoreria}";

            try
            {
                using var connection = new SqlConnection(stringConn);
                response.Result = connection.Query<TesoreriaDetalle>(query).FirstOrDefault();

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }

            return response;
        }

        public ErrorDTO SaldosProveedor_Actualizar(int CodEmpresa, decimal Saldo, decimal Tipo_Cambio, int Cod_Proveedor)
        {
            ErrorDTO resp = new ErrorDTO();

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"update cxp_proveedores set saldo = isnull(saldo,0) - {Saldo}
                                ,SALDO_DIVISA_REAL = isnull(SALDO_DIVISA_REAL,0) - {Saldo} / {Tipo_Cambio}
                                where cod_proveedor = {Cod_Proveedor}";

                    resp.Code = connection.Query<int>(query).FirstOrDefault();
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

        public ErrorDTO FacturaEstado_Actualizar(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            ErrorDTO resp = new ErrorDTO();

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"update cxp_facturas set cxp_estado = 'G' where cod_factura = '{Cod_Factura}' and cod_proveedor = {Cod_Proveedor}";

                    resp.Code = connection.Query<int>(query).FirstOrDefault();
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

        public ErrorDTO CompraEstado_Actualizar(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            ErrorDTO resp = new ErrorDTO();

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"update CPR_COMPRAS set cxp_estado = 'G' where cod_factura = '{Cod_Factura}' and cod_proveedor = {Cod_Proveedor}";

                    resp.Code = connection.Query<int>(query).FirstOrDefault();
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

        public ErrorDTO Pago_Insertar(int CodEmpresa, DetallePago data)
        {
            ErrorDTO resp = new ErrorDTO();

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"INSERT cxp_pagoProv(npago,cod_proveedor,cod_factura,fecha_vencimiento,monto,frecuencia,tipo_transac,
                                apl_cargo_flotante,pago_anticipado,forma_pago,importe_divisa_real,tipo_cambio,cod_divisa) 
                                VALUES({data.NPago},{data.Cod_Proveedor},'{data.Cod_Factura}','{data.Fecha_Vencimiento}',{data.Monto},{data.Frecuencia},{data.Tipo},{data.Apl_Cargo_Flotante},
                                {data.Pago_Anticipado},'{data.Forma_Pago}',{data.Importe_Divisa_Real},{data.Tipo_Cambio},'{data.Cod_Divisa}')";

                    resp.Code = connection.Query<int>(query).FirstOrDefault();
                    resp.Description = "Registro guardado correctamente";

                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDTO PagoProvCargo_Insertar(int CodEmpresa, PagoProvCargo data)
        {
            ErrorDTO resp = new ErrorDTO();

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"INSERT cxp_PagoProvCargos(Npago,Cod_factura,cod_proveedor,cod_cargo,monto,registro_fecha,
                                        registro_usuario,cod_divisa,tipo_cambio,tipo_cargo,tipo_proceso) 
                                VALUES({data.NPago},'{data.Cod_Factura}',{data.Cod_Proveedor},'{data.Cod_Cargo}',{data.Monto},
                                        '{DateTime.Now}','{data.Registro_Usuario}','{data.Cod_Divisa}',
                                        {data.Tipo_Cambio},'{data.Tipo_Cargo}','{data.Tipo_Proceso}')";

                    resp.Code = connection.Query<int>(query).FirstOrDefault();
                    resp.Description = "Registro guardado correctamente";

                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDTO<Disponible> Disponible_Obtener(int CodEmpresa, int NPago, string Cod_Factura, int Cod_Proveedor)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDTO<Disponible>();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT P.Npago,(P.Monto - (isnull(sum(C.monto),0))) AS Neto
                                FROM cxp_pagoProv P LEFT JOIN cxp_pagoProvCargos C ON P.npago = C.npago
                                AND P.cod_factura = C.cod_factura AND P.cod_proveedor = C.cod_proveedor
                                WHERE P.npago = {NPago} AND P.cod_factura = '{Cod_Factura}' AND P.cod_proveedor = {Cod_Proveedor} group by P.NPago,P.Monto ";

                    response.Result = connection.Query<Disponible>(query).FirstOrDefault();

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

        public ErrorDTO PagoProv_Actualizar(int CodEmpresa, string Usuario, string Cod_Factura, int Cod_Proveedor)
        {
            ErrorDTO resp = new ErrorDTO();

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"UPDATE cxp_pagoProv 
                                SET 
                                Tesoreria = 0,
                                fecha_traslada = '{DateTime.Now}',
                                user_traslada = '{Usuario}'
                                WHERE cod_proveedor = {Cod_Proveedor} AND cod_factura = '{Cod_Factura}'
                                AND Npago IN(SELECT P.npago FROM cxp_pagoProv P INNER JOIN cxp_PagoprovCargos C
                                ON P.cod_proveedor = C.cod_proveedor AND P.cod_factura = C.cod_factura AND P.npago = C.npago
                                WHERE P.cod_proveedor = {Cod_Proveedor} AND P.cod_factura = '{Cod_Factura}'
                                GROUP BY P.npago,P.cod_proveedor,P.cod_factura,P.monto 
                                HAVING P.Monto = isnull(Sum(C.Monto), 0))";

                    resp.Code = connection.Query<int>(query).FirstOrDefault();
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

        public ErrorDTO FechaVencimiento_Actualizar(int CodEmpresa, DetallePago data)
        {
            ErrorDTO resp = new ErrorDTO();

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"UPDATE cxp_pagoprov SET fecha_vencimiento = '{data.Fecha_Vencimiento}' WHERE npago = {data.NPago} AND cod_factura = '{data.Cod_Factura}' and cod_proveedor = {data.Cod_Proveedor}";

                    resp.Code = connection.Query<int>(query).FirstOrDefault();
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
    }
}