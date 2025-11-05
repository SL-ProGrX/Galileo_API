using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.CxP;
using PgxAPI.Models.ERROR;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmCxPControlEjecucionDB
    {
        private readonly IConfiguration _config;
        mSecurityMainDb DBBitacora;

        public frmCxPControlEjecucionDB(IConfiguration config)
        {
            _config = config;
            DBBitacora = new mSecurityMainDb(_config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDTO data)
        {
            return DBBitacora.Bitacora(data);
        }

        public ErrorDto SincronizaTesoreriaCxPReportes(int CodEmpresa)
        {
            ErrorDto resp = new ErrorDto();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var procedure = "spCxP_SincronizaTesoreria";

                    resp.Code = connection.Query<int>(procedure, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    resp.Description = "Sincronizaci�n finalizada correctamente";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto<ProveedoresPagosLista> Proveedores_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro, string? filtroQ)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            var response = new ErrorDto<ProveedoresPagosLista>
            {
                Code = 0,
                Result = new ProveedoresPagosLista()
            };
            response.Result.Total = 0;
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);
                {

                    //Busco Total

                    if (filtroQ != null)
                    {
                        query = "SELECT COUNT(*) FROM CXP_PROVEEDORES P inner join CntX_Divisas D ON P.cod_divisa = D.cod_divisa and D.cod_contabilidad = 1" + filtroQ;
                        response.Result.Total = connection.Query<int>(query).FirstOrDefault();
                    }
                    else
                    {
                        query = "SELECT COUNT(*) FROM CXP_PROVEEDORES";
                        response.Result.Total = connection.Query<int>(query).FirstOrDefault();
                    }


                    if (filtro != null)
                    {
                        filtro = " AND P.COD_PROVEEDOR LIKE '%" + filtro + "%' OR P.DESCRIPCION LIKE '%" + filtro + "%' ";
                    }

                    if (pagina != null)
                    {
                        paginaActual = " OFFSET " + pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
                    }

                    query = $@"SELECT P.cod_proveedor,P.descripcion,rtrim(D.cod_divisa) + ' - ' + rtrim(D.descripcion) AS  'Divisa',P.CedJur as 'Cedjuridica',
                                P.cod_banco, dbo.fxSys_Cuenta_Bancos_Desc(P.cod_Banco) as 'Cuenta_Default'
                               FROM cxp_proveedores P inner join CntX_Divisas D ON P.cod_divisa = D.cod_divisa and D.cod_contabilidad = 1 
                                         {filtroQ}
                                         {filtro} 
                                        ORDER BY COD_PROVEEDOR
                                        {paginaActual}
                                        {paginacionActual} ";


                    response.Result.Proveedores = connection.Query<ProveedorPagos>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.Proveedores = null;
                response.Result.Total = 0;
            }
            return response;
        }

        public ErrorDto<Divisa> DivisaFuncional_Obtener(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<Divisa>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select rtrim(cod_divisa) as 'Cod_Divisa', rtrim(descripcion) as 'Descripcion' 
                                    from CntX_Divisas where cod_contabilidad = 1 order by divisa_local desc,cod_divisa";
                    response.Result = connection.Query<Divisa>(query).FirstOrDefault();
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

        public ErrorDto<List<Cargo>> CargosAdicionales_Obtener(int CodEmpresa)
        {
            var response = new ErrorDto<List<Cargo>>
            {
                Code = 0
            };

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            string query = $@"SELECT RTRIM(COD_CARGO) AS 'cod_cargo', RTRIM(descripcion) AS 'descripcion'
                            FROM cxp_cargos WHERE activo = 1
                            ";
            try
            {
                using var connection = new SqlConnection(stringConn);
                response.Result = connection.Query<Cargo>(query).ToList();

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }

            return response;

        }

        public ErrorDto<List<FacturaPendiente_Pago>> FacturasPendientePago_Obtener(int CodEmpresa, FactPen_Req request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<List<FacturaPendiente_Pago>>
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var procedure = "spCxP_FacturasPendientesPago";

                    var values = new
                    {
                        Proveedor = request.Proveedor,
                        Divisa = request.Divisa,
                        Corte = request.Corte,
                        Usuario = request.Usuario == "" ? null : request.Usuario,

                    };

                    response.Result = connection.Query<FacturaPendiente_Pago>(procedure, values, commandType: CommandType.StoredProcedure).ToList();

                    foreach (var item in response.Result)
                    {
                        //item.Apl_Cargo_Flotante = item.Apl_Cargo_Flotante == "S" ? true : false;
                        item.Datakey = item.Npago.ToString() + "-" + item.Cod_Proveedor.ToString() + "-" + item.Cod_Factura;
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

        public ErrorDto<Detalle> DetalleProveedor_Obtener(int CodEmpresa, int Cod_Proveedor, string Vence)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<Detalle>
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"SELECT 
                                    p.credito_plazo as Credito, 
                                    p.ultimo_pago, 
                                    p.saldo, 
                                    dbo.fxCxP_CargoFlotanteSaldo({Cod_Proveedor}, '{Vence}') AS Car_Per_Saldo, 
                                    ISNULL((SELECT SUM(valor) 
                                            FROM cxp_cargosPer 
                                            WHERE cod_proveedor = {Cod_Proveedor} AND tipo = 'P' AND vence >= '{Vence}'), 0) AS Car_Per_Porc
                                FROM 
                                    cxp_proveedores p
                                WHERE 
                                    p.cod_proveedor = {Cod_Proveedor}";
                    response.Result = connection.Query<Detalle>(query).FirstOrDefault();
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

        public ErrorDto RevisionPagos_Reactivar(int CodEmpresa, string User)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"UPDATE P SET P.TESORERIA = Null, P.FECHA_TRASLADA = Null, P.USER_TRASLADA = Null
                                FROM CXP_PAGOPROV P left join TES_TRANSACCIONES T ON P.TESORERIA = T.NSOLICITUD
                                WHERE ISNULL(P.tesoreria, 0) > 0
                                AND T.NSOLICITUD IS NULL";
                    resp.Code = connection.Query<int>(query).FirstOrDefault();

                    if (resp.Code == 0)
                    {
                        Bitacora(new BitacoraInsertarDTO
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = User,
                            DetalleMovimiento = "Revisi�n de Pagos de Facturas en Bancos con Solicitud eliminada",
                            Movimiento = "APLICA - WEB",
                            Modulo = 30
                        });
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

        public ErrorDto<List<Autorizado>> Autorizaciones_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            var response = new ErrorDto<List<Autorizado>>
            {
                Code = 0
            };

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            string query = $@"select * from cxp_autorizaciones where cod_proveedor = {Cod_Proveedor}
                            ";
            try
            {
                using var connection = new SqlConnection(stringConn);
                response.Result = connection.Query<Autorizado>(query).ToList();

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }

            return response;

        }

        public ErrorDto<Fusion> Fusion_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<Fusion>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select F.cod_proveedor, F.cod_proveedor_fus, rtrim(P.descripcion) as Proveedor
                                 from cxp_fusiones F inner join cxp_proveedores P on F.cod_proveedor = P.cod_proveedor
                                 where F.cod_proveedor_fus = {Cod_Proveedor}";
                    response.Result = connection.Query<Fusion>(query).FirstOrDefault();
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

        public ErrorDto<List<CuentaDesembolso>> CuentasDesembolso_Obtener(int CodEmpresa)
        {
            var response = new ErrorDto<List<CuentaDesembolso>>
            {
                Code = 0
            };
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var procedure = "[spCxP_Bancos_Autorizados]";
                    response.Result = connection.Query<CuentaDesembolso>(procedure, commandType: CommandType.StoredProcedure).ToList();
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

        public ErrorDto<InfoCuenta> InfoCuenta_Obtener(int CodEmpresa, int Cod_Banco)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<InfoCuenta>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select id_banco,descripcion,cod_divisa
                                ,dbo.fxCntXTipoCambio(1,COD_DIVISA,Getdate(),'V') as 'Tipo_Cambio',CTACONTA
                                from Tes_Bancos 
                                where id_banco = {Cod_Banco}";
                    response.Result = connection.Query<InfoCuenta>(query).FirstOrDefault();
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

        public ErrorDto<List<CuentaBancaria>> CuentasBancarias_Obtener(int CodEmpresa, string Identificacion, int BancoId, int DivisaCheck)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<CuentaBancaria>>
            {
                Code = 0
            };
            try
            {

                Identificacion = Identificacion.Replace("undefined", "").Replace(" ", "").Trim();

                using var connection = new SqlConnection(clienteConnString);
                {

                    var procedure = "[spSys_Cuentas_Bancarias]";
                    var values = new
                    {
                        Identificacion = Identificacion.Trim(),
                        BancoId = BancoId,
                        DivisaCheck = DivisaCheck

                    };

                    response.Result = connection.Query<CuentaBancaria>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
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

        public ErrorDto<List<CargoPorcentual>> CargoPorcentual_Obtener(int CodEmpresa, int Cod_Proveedor, string Vence)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new  ErrorDto<List<CargoPorcentual>>
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"SELECT cod_proveedor, (valor / 100) AS Porcentaje 
                                FROM cxp_cargosPer WHERE cod_proveedor = {Cod_Proveedor}
                                AND tipo = 'P' AND vence >= '{Vence}'";
                    response.Result = connection.Query<CargoPorcentual>(query).ToList();
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

        public ErrorDto<ProveedorPagos> ConsultaAscDesc(int CodEmpresa, int Cod_Proveedor, string Vence, string tipo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<ProveedorPagos>
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = "";

                    if (tipo == "desc")
                    {
                        if (Cod_Proveedor == 0)
                        {
                            query = $@"select TOP 1  
                                    P.cod_proveedor,P.descripcion,rtrim(D.cod_divisa) as 'Cod_Divisa',P.CedJur,  rtrim(D.cod_divisa) + ' - ' + rtrim(D.descripcion) AS  'Divisa',P.cod_divisa,P.CedJur as 'Cedjuridica',
                                    P.cod_banco, dbo.fxSys_Cuenta_Bancos_Desc(P.cod_Banco) as 'Cuenta_Default'
                                    from cxp_proveedores P 
                                    inner join CntX_Divisas D on P.cod_divisa = D.cod_divisa and D.cod_contabilidad = 1
                                    AND P.cod_proveedor in(select cod_proveedor From cxp_PagoProv Where tesoreria Is Null and fecha_vencimiento <= '{Vence} 23:59:59') 
                                    order by cod_proveedor desc";
                        }
                        else
                        {
                            query = $@"select TOP 1  
                                    P.cod_proveedor,P.descripcion,rtrim(D.cod_divisa) as 'Cod_Divisa',P.CedJur,  rtrim(D.cod_divisa) + ' - ' + rtrim(D.descripcion) AS  'Divisa',P.cod_divisa,P.CedJur as 'Cedjuridica',
                                    P.cod_banco, dbo.fxSys_Cuenta_Bancos_Desc(P.cod_Banco) as 'Cuenta_Default'
                                    from cxp_proveedores P 
                                    inner join CntX_Divisas D on P.cod_divisa = D.cod_divisa and D.cod_contabilidad = 1
                                    AND P.cod_proveedor in(select cod_proveedor From cxp_PagoProv Where tesoreria Is Null and fecha_vencimiento <= '{Vence} 23:59:59' and cod_proveedor < {Cod_Proveedor} group by cod_proveedor) 
                                    order by cod_proveedor desc";
                        }

                    }
                    else
                    {
                        query = $@"select TOP 1  
                                P.cod_proveedor,P.descripcion,rtrim(D.cod_divisa) as 'Cod_Divisa',P.CedJur,  rtrim(D.cod_divisa) + ' - ' + rtrim(D.descripcion) AS  'Divisa',P.cod_divisa,P.CedJur as 'Cedjuridica',
                                P.cod_banco, dbo.fxSys_Cuenta_Bancos_Desc(P.cod_Banco) as 'Cuenta_Default'
                                from cxp_proveedores P 
                                inner join CntX_Divisas D on P.cod_divisa = D.cod_divisa and D.cod_contabilidad = 1
                                AND P.cod_proveedor in(select cod_proveedor From cxp_PagoProv Where tesoreria Is Null and fecha_vencimiento <= '{Vence} 23:59:59' and cod_proveedor > {Cod_Proveedor} group by cod_proveedor) 
                                order by cod_proveedor asc";
                    }


                    response.Result = connection.Query<ProveedorPagos>(query).FirstOrDefault();

                    // result = result == 0 || result == Cod_Proveedor ? Cod_Proveedor : result;

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

        public ErrorDto Detalle_Insertar(int CodEmpresa, TesTransAsientoDTO data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"INSERT Tes_Trans_Asiento(nsolicitud,cuenta_contable,monto,debehaber,linea,cod_unidad,cod_cc,cod_divisa,tipo_cambio) 
                        VALUES({data.NSolicitud},'{data.Cuenta_Contable}',{data.Monto}
                       ,'{data.DebeHaber}',{data.Linea},'{data.Cod_Unidad}','{data.Cod_Cc}','{data.Cod_Divisa}',{data.Tipo_Cambio})";

                    resp.Code = connection.Query<int>(query).FirstOrDefault();
                    resp.Description = "Registro agregado correctamente";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto<Anticipo> MontoAnticipos_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<Anticipo>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT isnull(Sum(Pc.monto),0) AS 'Cargos'
                                FROM CXP_CARGOSPER Cp INNER JOIN CXP_ANTICIPOS Ca ON Cp.COD_PROVEEDOR = Ca.COD_PROVEEDOR AND Cp.COD_CARGO = Ca.COD_CARGO AND Cp.ID = Ca.ID_CARGO
                                INNER JOIN cxp_pagoProv Pf ON Pf.COD_PROVEEDOR = Cp.COD_PROVEEDOR
                                INNER JOIN CXP_PAGOPROVCARGOS Pc ON Pf.COD_PROVEEDOR = Pc.COD_PROVEEDOR  AND  Pf.COD_FACTURA = Pc.COD_FACTURA AND Pc.NPAGO = Pf.NPAGO AND Pc.ID = Cp.ID
                                Where Cp.COD_PROVEEDOR = {Cod_Proveedor}
                                AND Pf.user_traslada = 'xBITxTesx' ";
                    response.Result = connection.Query<Anticipo>(query).FirstOrDefault();
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

        public ErrorDto Tesoreria_Insertar(int CodEmpresa, Tes_TransaccionesDTO data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"INSERT Tes_Transacciones(id_banco,tipo,codigo,beneficiario,monto,fecha_solicitud,estado,estadoi
                                ,modulo,submodulo,cta_ahorros,detalle1,detalle2,referencia,op,genera,actualiza,cod_unidad
                                ,cod_concepto,user_solicita,autoriza,fecha_autorizacion,user_autoriza,TIPO_BENEFICIARIO,tipo_cambio,cod_divisa)  
                                VALUES({data.Id_Banco},'{data.Tipo}','{data.Codigo}','{data.Beneficiario}',{data.Monto},'{data.Fecha_Solicitud}','{data.Estado}',
                                '{data.Estadoi}','{data.Modulo}','{data.Submodulo}','{data.Cta_Ahorros}'),'{data.Detalle1}','{data.Detalle2}',{data.Referencia},
                                {data.Op},'{data.Genera}','{data.Actualiza}','{data.Cod_Unidad}','{data.Cod_Concepto}','{data.User_Solicita}','{data.Autoriza}',
                                '{data.Fecha_Autorizacion}','{data.User_Autoriza}',{data.Tipo_Beneficiario},{data.Tipo_Cambio}";

                    resp.Code = connection.Query<int>(query).FirstOrDefault();
                    resp.Description = "Registro agregado correctamente";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto<Tes_TransaccionesDTO> Tesoreria_Obtener(int CodEmpresa, int nSolicitud)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<Tes_TransaccionesDTO>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT * FROM Tes_Transacciones WHERE nsolicitud = {nSolicitud}";
                    response.Result = connection.Query<Tes_TransaccionesDTO>(query).FirstOrDefault();
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

        public ErrorDto EjecucionPagosCargos_Registra(int CodEmpresa, FacturaPendiente_Pago data)
        {
            ErrorDto resp = new ErrorDto();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var procedure = "spCxP_EjecucionPagos_RegistroCargos";

                    var values = new
                    {
                        Proveedor = data.Proveedor,
                        Factura = data.Cod_Factura,
                        NPago = data.Npago,
                        CodCargo = data.Cod_Cargo,
                        Divisa = data.Cod_Divisa,
                        Monto = data.Monto,
                        TipoCambio = data.Tipo_Cambio,
                        Usuario = data.Usuario,

                    };

                    resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    resp.Description = "Registro agregado correctamente";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }





        public ErrorDto EjecucionPagos_CargosFlotantes_Aplicar(int CodEmpresa, FacturaPendiente_Pago data)
        {
            ErrorDto resp = new ErrorDto();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var procedure = "spCxP_EjecucionPagos_AplicaCargosFlotantes";

                    var values = new
                    {
                        Proveedor = data.Cod_Proveedor,
                        Factura = data.Cod_Factura,
                        NPago = data.Npago,
                        Disponible = data.Neto,
                        Corte = $"{data.Fecha_Vencimiento.ToString("yyyy/MM/dd")} 23:59:59",
                        AplicaCargos = data.Apl_Cargo_Flotante,
                        Usuario = data.Usuario,

                    };

                    resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    resp.Description = "Cargos aplicados correctamente";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto EjecucionPagos_SaldosCargoPorc_Actualizar(int CodEmpresa)
        {
            ErrorDto resp = new ErrorDto();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var procedure = "spCxP_EjecucionPagos_ActualizaSaldosConCargosPorc";

                    resp.Code = connection.Query<int>(procedure, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    resp.Description = "Cargos actualizados correctamente";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto<DesembolsoNetos> DesembolsoNetos_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<DesembolsoNetos>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT CEDJUR, cod_proveedor, SUM(monto - cargos) AS Neto, SUM(Divisa_Real_Neto) AS 'Divisa_Real_Neto'
                                FROM vCXP_Pagos 
                                WHERE cod_Proveedor = {Cod_Proveedor}
                                GROUP BY cod_proveedor, CEDJUR;
                                ";
                    response.Result = connection.Query<DesembolsoNetos>(query).FirstOrDefault();
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

        public ErrorDto Indicadores_Actualizar(int CodEmpresa, PagoProvUpdate data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"UPDATE cxp_pagoprov 
                                    SET tesoreria = {data.Tesoreria},
                                        fecha_traslada = Getdate(),
                                        user_traslada = '{data.User_Traslada}',
                                        pago_tercero = '{(data.IsPagoTerceroChecked ? data.Pago_Tercero : string.Empty)}'
                                    WHERE user_traslada = 'xBITxTesx'
                                    AND cod_proveedor = {data.Cod_Proveedor}";

                    resp.Code = connection.Query<int>(query).FirstOrDefault();
                    resp.Description = "Registro actualizado correctamente";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto CancelacionCargos_Actualizar(int CodEmpresa, int Cod_Proveedor, string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"UPDATE cxp_pagoprov 
                                    SET tesoreria = 0,
                                        Tipo_Cancelacion = 'C',
                                        Tesoreria_Estado = 'E',
                                        fecha_traslada = Getdate(),
                                        user_traslada = '{Usuario}',
                                        pago_tercero = '',
                                        Tesoreria_Emision = Getdate()
                                    WHERE user_traslada = 'xBITxTesx'
                                    AND cod_proveedor = {Cod_Proveedor}";

                    resp.Code = connection.Query<int>(query).FirstOrDefault();
                    resp.Description = "Cargos actualizado correctamente";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto EjecucionPagos_TesoreriaDetalle_Actualizar(int CodEmpresa)
        {
            ErrorDto resp = new ErrorDto();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var procedure = "spCxP_Tesoreria_Detalle_Update";

                    resp.Code = connection.Query<int>(procedure, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    resp.Description = "Registro actualizado correctamente";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto<List<CargoPer>> CargosPer_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<CargoPer>>
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"SELECT Cr.COD_CARGO, Cr.DESCRIPCION, Cr.COD_CUENTA, Pc.monto, Pc.COD_DIVISA
                                    FROM CXP_CARGOSPER Cp
                                    INNER JOIN CXP_ANTICIPOS Ca ON Cp.COD_PROVEEDOR = Ca.COD_PROVEEDOR AND Cp.COD_CARGO = Ca.COD_CARGO AND Cp.ID = Ca.ID_CARGO
                                    INNER JOIN cxp_pagoProv Pf ON Pf.COD_PROVEEDOR = Cp.COD_PROVEEDOR
                                    INNER JOIN CXP_PAGOPROVCARGOS Pc ON Pf.COD_PROVEEDOR = Pc.COD_PROVEEDOR AND Pf.COD_FACTURA = Pc.COD_FACTURA AND Pc.NPAGO = Pf.NPAGO AND Pc.ID = Cp.ID
                                    INNER JOIN CXP_CARGOS Cr ON Cp.COD_CARGO = Cr.COD_CARGO
                                    WHERE Cp.COD_PROVEEDOR = {Cod_Proveedor} AND Pf.user_traslada = 'xBITxTesx'";
                    response.Result = connection.Query<CargoPer>(query).ToList();
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

        public ErrorDto<ProveedorInfoEjecucion> ProveedorTesoreria_Obtener(int CodEmpresa, int Cod_Proveedor, int cod_contabilidad)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<ProveedorInfoEjecucion>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select P.CEDJUR, P.cod_proveedor, P.descripcion, P.cod_cuenta, P.cod_divisa,
                                       D.cod_cuenta as 'CtaDivDifIng', D.cod_cuenta_Gasto as 'CtaDivDifGst',
                                       dbo.fxCntXTipoCambio(1, P.COD_DIVISA, Getdate(), 'V') as 'TipoCambio',
                                       Getdate() as Fecha
                                from  Cxp_Proveedores P
                                inner join CntX_Divisas D on P.cod_divisa = D.cod_divisa
                                and D.cod_contabilidad = {cod_contabilidad}
                                where P.cod_proveedor = {Cod_Proveedor}
                                ";
                    response.Result = connection.Query<ProveedorInfoEjecucion>(query).FirstOrDefault();
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

        public ErrorDto<List<Anticipo>> Anticipos_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<Anticipo>>
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"SELECT Cr.COD_CARGO, Cr.DESCRIPCION, Cr.COD_CUENTA, Pc.monto, Pc.COD_DIVISA
                                    FROM CXP_CARGOSPER Cp
                                    INNER JOIN CXP_ANTICIPOS Ca ON Cp.COD_PROVEEDOR = Ca.COD_PROVEEDOR AND Cp.COD_CARGO = Ca.COD_CARGO AND Cp.ID = Ca.ID_CARGO
                                    INNER JOIN cxp_pagoProv Pf ON Pf.COD_PROVEEDOR = Cp.COD_PROVEEDOR
                                    INNER JOIN CXP_PAGOPROVCARGOS Pc ON Pf.COD_PROVEEDOR = Pc.COD_PROVEEDOR AND Pf.COD_FACTURA = Pc.COD_FACTURA AND Pc.NPAGO = Pf.NPAGO AND Pc.ID = Cp.ID
                                    INNER JOIN CXP_CARGOS Cr ON Cp.COD_CARGO = Cr.COD_CARGO
                                    WHERE Cp.COD_PROVEEDOR = {Cod_Proveedor} AND Pf.user_traslada = 'xBITxTesx'";
                    response.Result = connection.Query<Anticipo>(query).ToList();
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