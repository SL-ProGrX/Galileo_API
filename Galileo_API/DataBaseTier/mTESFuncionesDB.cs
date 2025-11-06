using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;

namespace PgxAPI.DataBaseTier
{
    public class MTesFuncionesDb
    {
        private readonly IConfiguration _config;
        public MTesFuncionesDb(IConfiguration config)
        {
            _config = config;
        }

        public long fxgTesoreriaMaestro(int CodEmpresa, string usuario, TesoreriaMaestroModel tesoreria)
        {

            long resp1 = 0;
            long lngSol = 0;
            string query = "", detalle1 = "", detalle2 = "";
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    detalle1 = (tesoreria.vDetalle1.Length > 26) ? tesoreria.vDetalle1.Substring(0, 26) : tesoreria.vDetalle1;
                    detalle2 = (tesoreria.vDetalle2.Length > 26) ? tesoreria.vDetalle2.Substring(0, 26) : tesoreria.vDetalle2;

                    if (tesoreria.vTipoDocumento.ToUpper() == "CK")
                    {
                        query = $@"INSERT INTO Tes_Transacciones (
                                        id_banco,
                                        tipo,
                                        codigo,
                                        beneficiario,
                                        monto,
                                        fecha_solicitud,
                                        estado,
                                        estadoi,
                                        modulo,
                                        submodulo,
                                        cta_ahorros,
                                        detalle1,
                                        detalle2,
                                        referencia,
                                        op,
                                        genera,
                                        actualiza,
                                        cod_unidad,
                                        cod_concepto,
                                        user_solicita,
                                        autoriza,
                                        fecha_autorizacion,
                                        user_autoriza,
                                        ref_01,
                                        ref_02,
                                        ref_03,
                                        cod_app,
                                        ID_TOKEN,
                                        REMESA_TIPO,
                                        REMESA_ID
                                    ) VALUES (
                                        '{tesoreria.vBanco}',
                                        '{tesoreria.vTipoDocumento}',
                                        '{tesoreria.vCodigo}',
                                        '{tesoreria.vBeneficiario}',
                                        {tesoreria.vMonto},
                                        '{tesoreria.vFecha}',
                                        'P',
                                        'P',
                                        'CC',
                                        'C',
                                        '{tesoreria.vCuenta}',
                                        '{detalle1}',
                                        '{detalle2}',
                                        {tesoreria.vReferencia},
                                        {tesoreria.vOP},
                                        'S',
                                        'S',
                                        '{tesoreria.vUnidad}',
                                        '{tesoreria.vConcepto}',
                                        '{usuario}',
                                        'S',
                                        Getdate(),
                                        '{usuario}',
                                        '{tesoreria.vRef_01}',
                                        '{tesoreria.vRef_02}',
                                        '{tesoreria.vRef_03}',
                                        '{tesoreria.vCodApp}',
                                        '{tesoreria.vToken}',
                                        '{tesoreria.vRemesaTipo}',
                                        {tesoreria.vRemesa}
                                    )";
                        connection.Execute(query);

                    }
                    else
                    {
                        query = $@"INSERT INTO Tes_Transacciones (
                                        id_banco,
                                        tipo,
                                        codigo,
                                        beneficiario,
                                        monto,
                                        fecha_solicitud,
                                        estado,
                                        estadoi,
                                        modulo,
                                        submodulo,
                                        cta_ahorros,
                                        detalle1,
                                        detalle2,
                                        referencia,
                                        op,
                                        genera,
                                        actualiza,
                                        cod_unidad,
                                        cod_concepto,
                                        ref_01,
                                        ref_02,
                                        ref_03,
                                        cod_app,
                                        ID_TOKEN,
                                        REMESA_TIPO,
                                        REMESA_ID,
                                        user_solicita
                                    ) VALUES (
                                        '{tesoreria.vBanco}',
                                        '{tesoreria.vTipoDocumento}',
                                        '{tesoreria.vCodigo}',
                                        '{tesoreria.vBeneficiario}',
                                        {tesoreria.vMonto},
                                        '{tesoreria.vFecha}',
                                        'P',
                                        'P',
                                        'CC',
                                        'C',
                                        '{tesoreria.vCuenta}',
                                        '{detalle1}',
                                        '{detalle2}',
                                        {tesoreria.vReferencia},
                                        {tesoreria.vOP},
                                        'S',
                                        'S',
                                        '{tesoreria.vUnidad}',
                                        '{tesoreria.vConcepto}',
                                        '{tesoreria.vRef_01}',
                                        '{tesoreria.vRef_02}',
                                        '{tesoreria.vRef_03}',
                                        '{tesoreria.vCodApp}',
                                        '{tesoreria.vToken}',
                                        '{tesoreria.vRemesaTipo}',
                                        {tesoreria.vRemesa},
                                        '{usuario}'
                                    )";
                        connection.Execute(query);
                    }

                    query = $@"select max(nsolicitud) as Solicitud from Tes_Transacciones";
                    resp1 = connection.Query<long>(query).FirstOrDefault();

                    query = $@"select * from Tes_Transacciones where nsolicitud = {resp1} ";
                    var resp2 = connection.Query<TesTransaccionesDto>(query).FirstOrDefault();

                    if (resp2 != null && resp2.CODIGO.Trim() == tesoreria.vCodigo.Trim())
                    {
                        lngSol = resp1;
                    }

                    if (lngSol == 0)
                    {
                        query = $@"select max(nsolicitud) as Solicitud from Tes_Transacciones where codigo = '{tesoreria.vCodigo}' 
                                    and op = {tesoreria.vOP} ";
                        lngSol = connection.Query<long>(query).FirstOrDefault();
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                lngSol = 0;
            }
            return lngSol;
        }

        public void sbgTesoreriaDetalle(int CodEmpresa, TesoreriaDetalleModel detalle)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    string query = $@"INSERT INTO Tes_Trans_Asiento  (
                                         nsolicitud,
                                        cuenta_contable,
                                        monto,
                                        debehaber,
                                        linea,
                                        cod_unidad,
                                        cod_cc
                                    ) VALUES (
                                        {detalle.vSolicitud},
                                        '{detalle.vCtaConta}',
                                        {detalle.vMonto},
                                        '{detalle.vDH}',
                                        {detalle.vLinea},
                                        '{detalle.vUnidad}',
                                        '{detalle.vCC}'
                                    )";
                    connection.Execute(query);
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
        }

        public static string fxTipoDocumento(string tipo)
        {
            switch (tipo)
            {
                case "CK":
                    return "Cheque";
                case "TE":
                    return "Transferencia";
                case "EF":
                case "RE":
                    return "Efectivo";
                case "ND":
                    return "Nota Debito";
                case "NC":
                    return "Nota Credito";
                case "OT":
                    return "Otro...";
                case "CD":
                    return "Ctrl Desembolsos";
                case "CP":
                    return "Proveedor";
                case "RC":
                    return "Retiro en Caja";
                case "FD":
                    return "Fondo Transitorio";
                case "TS":
                    return "Transferencia SINPE";
                //-------------------------------------------------

                case "Cheque":
                    return "CK";
                case "Transferencia":
                    return "TE";
                case "Efectivo":
                    return "EF";
                case "Nota Debito":
                    return "ND";
                case "Nota Credito":
                    return "NC";
                case "Otro...":
                    return "OT";
                case "Ctrl Desembolsos":
                    return "CD";
                case "Proveedor":
                    return "CP";
                case "Retiro en Caja":
                    return "RC";
                case "Fondo Transitorio":
                    return "FD";
                case "Transferencia SINPE":
                    return "TS";
                default:
                    return "";
            }
        }

        public string fxTesToken(int CodEmpresa, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            string strToken = "";
            try
            {
                string query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    DateTime fxFechaServidor = DateTime.Now;
                    strToken = fxFechaServidor.ToString("yyyy.MM.dd");

                    query = $@"select  isnull(COUNT(id_token),0)+ 1 as 'consec'  from tes_tokens where id_token like('{strToken}')";
                    var resp = connection.Query<int>(query).FirstOrDefault();

                    strToken = strToken + resp.ToString();

                    query = $@"insert tes_tokens(id_token,registro_fecha,registro_usuario,estado)
                                    values('{strToken}',Getdate(),'{usuario}','A') ";
                    connection.Execute(query);
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
            return strToken;
        }

        public bool fxgTESValidaDatos(int CodEmpresa, int Contabilidad, string vTipo, string vCodigo, string vFiltro = "")
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            bool result = false;
            try
            {
                string query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    switch (vTipo.ToUpper())
                    {
                        case "CONCEPTO":
                            query = @"select isnull(count(*),0) as Existe from tes_conceptos 
                                where cod_concepto = @codigo and Estado = 'A'";
                            break;
                        case "UNIDAD":
                            query = @"select isnull(count(*),0) as Existe from CntX_unidades 
                                where cod_unidad = @codigo and Activa = 1 and cod_Contabilidad = @contabilidad";
                            break;
                        case "CC":
                            query = @"select isnull(count(*),0) as Existe from CNTX_CENTRO_COSTOS 
                                where COD_CENTRO_COSTO = @codigo and Activo = 1 and cod_contabilidad = @contabilidad";
                            if (vFiltro != "")
                            {
                                query += @" and COD_CENTRO_COSTO in(select COD_CENTRO_COSTO from CNTX_UNIDADES_CC 
                                    where cod_unidad = @filtro and cod_contabilidad = @contabilidad)";
                            }
                            break;
                    }
                    int existe = connection.QueryFirstOrDefault<int>(query, new 
                        {
                            codigo = vCodigo,
                            contabilidad = Contabilidad,
                            filtro = vFiltro
                        });

                    if (existe == 0)
                    {
                        result = false;
                    }
                    else 
                    {
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return result;
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbgTESBusqueda(int CodEmpresa, int Contabilidad, string vTipo, string vFiltro = "")
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                string query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    switch (vTipo.ToUpper())
                    {
                        case "CONCEPTO":
                            query = @"select cod_concepto as item,descripcion from tes_conceptos 
                                where Estado = 'A' order by cod_concepto";
                            break;
                        case "UNIDAD":
                            query = @"select cod_unidad as item,descripcion from CntX_unidades
                                where Activa = 1 and cod_Contabilidad = @contabilidad order by cod_unidad";
                            break;
                        case "CC":
                            query = @"select COD_CENTRO_COSTO as item,descripcion from CNTX_CENTRO_COSTOS
                                where Activo = 1 and cod_contabilidad = @contabilidad";
                            if (vFiltro != "")
                            {
                                query += @" and COD_CENTRO_COSTO in(select COD_CENTRO_COSTO from CNTX_UNIDADES_CC 
                                    where cod_unidad = @filtro and cod_contabilidad = @contabilidad)";
                            }
                            query += " order by COD_CENTRO_COSTO";
                            break;
                    }
                    response.Result = connection.Query<DropDownListaGenericaModel>(query, new
                    {
                        contabilidad = Contabilidad,
                        filtro = vFiltro
                    }).ToList();
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
