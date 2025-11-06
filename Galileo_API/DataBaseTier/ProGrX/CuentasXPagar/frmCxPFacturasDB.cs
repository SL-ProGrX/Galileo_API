using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.CxP;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.Security;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmCxPFacturasDB
    {
        private readonly IConfiguration _config;
        MSecurityMainDb DBBitacora;
        public frmCxPFacturasDB(IConfiguration config)
        {
            _config = config;
            DBBitacora = new MSecurityMainDb(_config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return DBBitacora.Bitacora(data);
        }

        public ErrorDto<List<ParametrosIva>> ParamIVA_Obtener(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<ParametrosIva>>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT P.COD_PARAMETRO, P.VALOR , P.DESCRIPCION , C.COD_CUENTA_MASK, C.DESCRIPCION AS DESC_CUENTA
                                FROM SYS_IVA_PARAMETROS P
                                LEFT JOIN CntX_Cuentas C ON C.cod_cuenta = P.VALOR AND C.cod_contabilidad = 1
                                WHERE COD_PARAMETRO in('02','03','08')";

                    response.Result = connection.Query<ParametrosIva>(query).ToList();
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

        public ErrorDto<DivisaLocal> DivisaLocal_Obtener(int CodEmpresa)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<DivisaLocal>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@" select rtrim(cod_divisa) as 'cod_divisa',rtrim(descripcion) as 'descripcion'
                                from CntX_Divisas where cod_contabilidad = 1
                                and Divisa_Local = 1";

                    response.Result = connection.Query<DivisaLocal>(query).FirstOrDefault();
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

        public ErrorDto<List<Divisa>> Divisas_Obtener(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<Divisa>>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select rtrim(cod_divisa) as 'Cod_Divisa', rtrim(descripcion) as 'Descripcion' 
                                    from CntX_Divisas where cod_contabilidad = 1 order by divisa_local desc,cod_divisa";
                    response.Result = connection.Query<Divisa>(query).ToList();
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

        public ErrorDto<List<Unidad>> Unidades_Obtener(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<List<Unidad>>
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT cod_unidad,descripcion FROM CntX_unidades WHERE Activa = 1 and cod_contabilidad = 1";

                    response.Result = connection.Query<Unidad>(query).ToList();
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

        public ErrorDto<List<CentroCosto>> CentrosCosto_Obtener(int CodEmpresa, string Cod_Unidad)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<List<CentroCosto>>
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT RTRIM(COD_CENTRO_COSTO) AS 'cod_centro_costo', RTRIM(descripcion) AS 'descripcion'
                                FROM CNTX_CENTRO_COSTOS 
                                WHERE COD_CONTABILIDAD = 1 AND ACTIVO = 1
                                AND COD_CENTRO_COSTO IN (
                                    SELECT COD_CENTRO_COSTO FROM CNTX_UNIDADES_CC
                                    WHERE COD_CONTABILIDAD = 1 AND COD_UNIDAD = '{Cod_Unidad}')";

                    response.Result = connection.Query<CentroCosto>(query).ToList();

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

        public ErrorDto<FacturaLista> Facturas_Obtener(int CodEmpresa, int Cod_Proveedor, int? pagina, int? paginacion, string? filtro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<FacturaLista>
            {
                Code = 0,
                Result = new FacturaLista()
            };
            response.Result.Total = 0;
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);
                {

                    //Busco Total
                    query = "SELECT COUNT(*) from cxp_facturas ";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();


                    if (filtro != null && Cod_Proveedor > 0)
                    {
                        filtro = " and COD_FACTURA LIKE '%" + filtro + "%' OR NOTAS LIKE '%" + filtro + "%' OR COD_PROVEEDOR LIKE '%" + filtro + "%' ";
                    }


                    if (filtro != null && Cod_Proveedor == 0)
                    {
                        filtro = " where COD_FACTURA LIKE '%" + filtro + "%' OR NOTAS LIKE '%" + filtro + "%' OR COD_PROVEEDOR LIKE '%" + filtro + "%' ";
                    }


                    paginaActual = " OFFSET " + pagina + " ROWS ";
                    paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";


                    if (Cod_Proveedor > 0)
                    {
                        query = $@"select cod_factura,cod_proveedor,TOTAL AS TOTAL_FACTURA ,notas  From cxp_facturas WHERE cod_proveedor = {Cod_Proveedor}
                                         {filtro} 
                                        order by cod_factura
                                        {paginaActual}
                                        {paginacionActual} ";
                    }
                    else
                    {
                        query = $@"select cod_factura,cod_proveedor,TOTAL AS TOTAL_FACTURA ,notas  From cxp_facturas
                                         {filtro} 
                                        order by cod_factura
                                        {paginaActual}
                                        {paginacionActual} ";
                    }

                    response.Result.Facturas = connection.Query<Factura>(query).ToList();

                    foreach (Factura ft in response.Result.Facturas)
                    {
                        ft.DataKey = ft.Cod_Factura + '-' + ft.Cod_Proveedor;
                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.Facturas = null;
                response.Result.Total = 0;
            }
            return response;
        }

        public ErrorDto<FacturaDto> FacturaDetalle_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<FacturaDto>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT F.*, P.descripcion AS Proveedor, P.cod_Divisa AS 'DivisaProv',
                                dbo.fxCxPSaldoFacturaCorte(F.cod_Proveedor, F.cod_Factura, Getdate()) AS 'Saldo',
                                RTRIM(D.descripcion) AS 'DivisaFactura'
                                FROM cxp_facturas F 
                                INNER JOIN cxp_proveedores P ON F.cod_proveedor = P.cod_proveedor
                                INNER JOIN CntX_Divisas D ON D.cod_contabilidad = 1 AND D.cod_divisa = F.cod_divisa
                                WHERE F.cod_factura = '{Cod_Factura}'";
                    if (Cod_Proveedor > 0)
                    {
                        query += $" AND F.cod_proveedor = {Cod_Proveedor}";
                    }

                    response.Result = connection.Query<FacturaDto>(query).FirstOrDefault();
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

        public ErrorDto<List<AsientoFactura>> FacturaAsientos_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<List<AsientoFactura>>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT C.cod_Cuenta_Mask, C.cod_cuenta,C.descripcion as 'Cuenta',D.debehaber,D.monto,D.cod_unidad,U.descripcion as 'Unidad',
                                D.cod_centro_costo,X.descripcion as 'CentroCosto',D.cod_proveedor,D.cod_factura,Div.Cod_Divisa,Div.Descripcion as 'Divisa',D.Tipo_Cambio
                                FROM CXP_FACTURAS_DETALLE D INNER JOIN CXP_FACTURAS Ch ON D.cod_factura = Ch.cod_factura AND D.cod_proveedor = Ch.Cod_Proveedor
                                INNER JOIN CntX_Cuentas C ON D.cod_cuenta = C.cod_cuenta AND D.cod_contabilidad = C.cod_Contabilidad
                                INNER JOIN CntX_Divisas Div ON D.cod_divisa = Div.cod_Divisa AND D.cod_contabilidad = Div.cod_Contabilidad
                                LEFT JOIN CntX_unidades U ON D.cod_unidad = U.cod_unidad AND U.cod_contabilidad = D.cod_Contabilidad
                                LEFT JOIN CNTX_CENTRO_COSTOS X ON D.cod_centro_costo = X.COD_CENTRO_COSTO AND X.cod_contabilidad = 1
                                WHERE D.cod_factura = '{Cod_Factura}' AND D.cod_proveedor = {Cod_Proveedor}
                                ORDER BY D.linea;";

                    response.Result = connection.Query<AsientoFactura>(query).ToList();

                    foreach (AsientoFactura AF in response.Result)
                    {
                        AF.DataKey = AF.Cod_Factura + '-' + AF.Cod_Proveedor + '-' + AF.Cod_Cuenta;
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

        public ErrorDto FacturaNumero_Cambiar(int CodEmpresa, FacturaCambioNo data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var procedure = "[spCxP_Factura_Cambio_No]";
                    var values = new
                    {
                        Proveedor = data.Cod_Proveedor,
                        Factura = data.Cod_Factura,
                        FactNew = data.Cod_FacturaNew,
                        Usuario = data.Usuario,

                    };

                    resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    resp.Description = "Ok";

                    if (resp.Code == 0)
                    {
                        Bitacora(new BitacoraInsertarDto
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = data.Usuario,
                            DetalleMovimiento = "Cambio Factura: " + data.Cod_Factura + " --> " + data.Cod_FacturaNew + " Prov.Id: " + data.Cod_Proveedor,
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

        public ErrorDto FacturaImpuesto_Actualizar(int CodEmpresa, FacturaImpuesto data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"UPDATE cxp_facturas SET 
                                IMPUESTO_VENTAS = '{data.Impuesto_Ventas}'
                                WHERE cod_proveedor = {data.Cod_Proveedor}
                                AND cod_factura = '{data.Cod_Factura}'";

                    resp.Code = connection.Query<int>(query).FirstOrDefault();
                    resp.Description = "Ok";

                    if (resp.Code == 0)
                    {
                        Bitacora(new BitacoraInsertarDto
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = data.Usuario,
                            DetalleMovimiento = "CxP-Factura: " + data.Cod_Factura + " ...Prov: " + data.Cod_Proveedor + " IV: " + data.Impuesto_Ventas,
                            Movimiento = "MODIFICA - WEB",
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

        public ErrorDto<ProveedorFactura> ProveedorFactura_Obtener(int CodEmpresa, int Cod_Proveedor)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<ProveedorFactura>
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select distinct P.cod_proveedor,P.descripcion,P.cod_divisa,c.cod_cuenta, C.COD_CUENTA_MASK, c.DESCRIPCION as Desc_Cuenta
                                ,rtrim(D.descripcion) as 'Divisa_Local'
                                from  Cxp_Proveedores P 
                                inner join CntX_Divisas D on P.cod_divisa = D.cod_divisa
                                inner join CNTX_CUENTAS C on p.COD_CUENTA = c.COD_CUENTA
                                and D.cod_contabilidad = 1
                                where P.cod_proveedor = {Cod_Proveedor}";

                    response.Result = connection.Query<ProveedorFactura>(query).FirstOrDefault();

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

        public ErrorDto<FacturaAntSig> ConsultaAscDesc(int CodEmpresa, string Cod_Factura, string tipo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<FacturaAntSig>
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
                        if (Cod_Factura == "0")
                        {
                            query = $@"select Top 1 cod_factura from cxp_facturas
                                    order by cod_factura desc";
                        }
                        else
                        {
                            query = $@"select Top 1 cod_factura from cxp_facturas
                                    where cod_factura < '{Cod_Factura}' order by cod_factura desc";
                        }

                    }
                    else
                    {
                        query = $@"select Top 1 cod_factura from cxp_facturas
                                    where cod_factura > '{Cod_Factura}' order by cod_factura asc";
                    }


                    response.Result = connection.Query<FacturaAntSig>(query).FirstOrDefault();

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

        public ErrorDto Factura_Anular(int CodEmpresa, FacturaAnular data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var procedure = "[spCxPFacturaAnula]";
                    var values = new
                    {
                        Proveedor = data.Cod_Proveedor,
                        Factura = data.Cod_Factura,
                        Usuario = data.Usuario,

                    };

                    resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    resp.Description = "Ok";

                    //if (resp.Code == 0)
                    //{
                    //    Bitacora(new BitacoraInsertarDto
                    //    {
                    //        EmpresaId = CodEmpresa,
                    //        Usuario = data.Usuario,
                    //        DetalleMovimiento = "Cambio Factura: " + data.Cod_Factura + " --> " + data.Cod_FacturaNew + " Prov.Id: " + data.Cod_Proveedor,
                    //        Movimiento = "APLICA - WEB",
                    //        Modulo = 30
                    //    });
                    //}
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto<FacturaPlantillaLista> Plantillas_Obtener(int CodEmpresa, int? pagina, int? paginacion, string? filtro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<FacturaPlantillaLista>
            {
                Code = 0,
                Result = new FacturaPlantillaLista()
            };
            response.Result.Total = 0;
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);
                {

                    //Busco Total
                    query = "SELECT COUNT(*) from CXP_PLANTILLAS ";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    if (filtro != null)
                    {
                        filtro = " and COD_PLANTILLA LIKE '%" + filtro + "%' OR DESCRIPCION LIKE '%" + filtro + "%' ";
                    }

                    if (filtro != null)
                    {
                        paginaActual = " OFFSET " + pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
                    }

                    query = $@"select COD_PLANTILLA, DESCRIPCION  From CXP_PLANTILLAS  WHERE ACTIVO = 1
                                         {filtro} 
                                        order by COD_PLANTILLA
                                        {paginaActual}
                                        {paginacionActual} ";


                    response.Result.Plantillas = connection.Query<FacturaPlantilla>(query).ToList();

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

        public ErrorDto<List<AsientoFactura>> PlantillaAsientos_Obtener(int CodEmpresa, int Cod_Plantilla, string fecha, decimal total)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<List<AsientoFactura>>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select Cta.COD_CUENTA_MASK, Cta.DESCRIPCION, P.COD_UNIDAD, P.COD_CENTRO_COSTO, Cta.COD_DIVISA
                                , dbo.fxCntXTipoCambio(P.COD_CONTABILIDAD, Cta.COD_DIVISA, '{fecha}', 'V') as 'Tipo_Cambio'
                                , {total} * P.PORCENTAJE / 100 as 'Debito', 0 as 'Credito'
                                , isnull(D.DESCRIPCION,'') as 'Divisa_Desc'
                                , isnull(U.DESCRIPCION,'') as 'Unidad_Desc', isnull(C.DESCRIPCION,'') as 'Centro_Desc'
                                from CXP_PLANTILLAS_ASIENTO P inner join CNTX_CUENTAS Cta on P.COD_CONTABILIDAD = Cta.COD_CONTABILIDAD
                                and P.COD_CUENTA = Cta.COD_CUENTA
                                left join CNTX_DIVISAS D on Cta.COD_CONTABILIDAD = D.COD_CONTABILIDAD and   Cta.COD_DIVISA = D.COD_DIVISA
                                left join CNTX_UNIDADES U on P.COD_CONTABILIDAD = U.COD_CONTABILIDAD and P.COD_UNIDAD = U.COD_UNIDAD
                                left join CNTX_CENTRO_COSTOS  C on P.COD_CONTABILIDAD = C.COD_CONTABILIDAD and P.COD_CENTRO_COSTO = C.COD_CENTRO_COSTO
                                Where COD_PLANTILLA = {Cod_Plantilla}
                                order by LINEA;";

                    response.Result = connection.Query<AsientoFactura>(query).ToList();

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

        public ErrorDto<List<Factura>> PlantillaFactura_Obtener(int CodEmpresa)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<List<Factura>>
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT F.cod_factura,F.cod_proveedor, P.descripcion AS Proveedor,F.total as total_factura,F.notas
                                FROM cxp_facturas F INNER JOIN cxp_proveedores P ON F.cod_proveedor = P.cod_proveedor
                                AND plantilla = 1;";

                    response.Result = connection.Query<Factura>(query).ToList();

                    foreach (Factura ft in response.Result)
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

        public ErrorDto<CuentaProveedor> CuentaProveedor_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<CuentaProveedor>
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT C.cod_cuenta,C.descripcion,C.Cod_Cuenta_Mask,P.cod_Divisa AS 'DivisaProv'
                                FROM cxp_proveedores P INNER JOIN Cntx_Cuentas C ON P.cod_cuenta = C.cod_cuenta and C.cod_Contabilidad = 1
                                WHERE P.cod_proveedor = {Cod_Proveedor}";

                    response.Result = connection.Query<CuentaProveedor>(query).FirstOrDefault();

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

        public int TipoCambio_Obtener(int CodEmpresa, string cod_Divisa, string Fecha)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            int result = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select dbo.fxCntXTipoCambio(1,'{cod_Divisa}','{Fecha}','V')";

                    result = connection.Query<int>(query).FirstOrDefault();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return result;
        }



        public ErrorDto FacturaAsientos_Borrar(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"DELETE cxp_facturas_detalle
                                WHERE cod_factura = '{Cod_Factura}' AND cod_proveedor = {Cod_Proveedor}";


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

        public ErrorDto SaldoPagarProv_Actualizar(int CodEmpresa, decimal Saldo, decimal Saldo_Divisa, int Cod_Proveedor)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"UPDATE cxp_proveedores SET saldo = ISNULL(saldo,0) + {Saldo}
                                ,SALDO_DIVISA_REAL =  ISNULL(SALDO_DIVISA_REAL ,0) + {Saldo_Divisa}
                                WHERE cod_proveedor = {Cod_Proveedor}";

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

        public ErrorDto FacturaAsiento_Insertar(int CodEmpresa, AsientoFactura data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"INSERT INTO cxp_facturas_detalle(linea,cod_factura,cod_proveedor,cod_contabilidad,cod_cuenta,cod_unidad,cod_centro_costo,cod_divisa
                                ,debeHaber,tipo_cambio,Monto) 
                                values({data.Linea},'{data.Cod_Factura}',{data.Cod_Proveedor},{data.Cod_Contabilidad}
                                ,'{data.Cod_Cuenta}','{data.Cod_Unidad}','{data.Cod_Centro_Costo}','{data.Cod_Divisa}', '{data.Debehaber}', {data.Tipo_Cambio},{data.Monto})";

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

        public ErrorDto PagoContado_Insertar(int CodEmpresa, PagoContado data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"INSERT cxp_pagoProv(NPago,Cod_Proveedor,Cod_Factura,Fecha_Vencimiento,Monto,Frecuencia
                                ,Tipo_Transac,User_TrasLada,Fecha_Traslada,Tesoreria,Pago_Tercero,Apl_Cargo_Flotante
                                ,Pago_Anticipado,forma_pago,IMPORTE_DIVISA_REAL,TIPO_CAMBIO,COD_DIVISA) 
                                values({data.NPago},{data.Cod_Proveedor},'{data.Cod_Factura}','{data.Fecha_Vencimiento}'
                                ,{data.Monto},{data.Frecuencia},{data.Tipo_Transac},'{data.User_Traslada}', '{data.Fecha_Traslada}', 
                                {data.Tesoreria},'{data.Pago_Tercero}',{data.Apl_Cargo_Flotante},{data.Pago_Anticipado},'{data.Forma_Pago}',{data.Importe_Divisa_Real}
                                ,{data.Tipo_Cambio},'{data.Cod_Divisa}')";

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

        public ErrorDto Factura_Insertar(int CodEmpresa, FacturaDto data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"INSERT cxp_facturas(estado,cod_factura,cod_proveedor,fecha,total,cxp_estado
                                ,asiento_generado,plantilla,vence,creacion_fecha,creacion_user,notas,cod_forma_Pago
                                ,cod_divisa,tipo_cambio,importe_divisa_real,IMPUESTO_VENTAS)
                                values('{data.Estado}','{data.Cod_Factura}',{data.Cod_Proveedor},'{data.Fecha}'
                                ,{data.Total},'{data.Cxp_Estado}','{data.Asiento_Generado}','{data.Plantilla}', '{data.Vence}', 
                                '{DateTime.Now}','{data.Creacion_User}','{data.Notas}','{data.Cod_Forma_Pago}','{data.Cod_Divisa}',{data.Tipo_Cambio},
                                {data.Importe_Divisa_Real}
                                ,{data.Impuesto_Ventas})";

                    resp.Code = connection.Query<int>(query).FirstOrDefault();
                    resp.Description = "Ok";

                    if (resp.Code == 0)
                    {
                        Bitacora(new BitacoraInsertarDto
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = data.Creacion_User,
                            DetalleMovimiento = "CxP Factura: " + data.Cod_Factura + " Prov: " + data.Cod_Proveedor,
                            Movimiento = "REGISTRA - WEB",
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

    }
}