using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmCntX_ConsultaCuentasDB
    {
        private readonly IConfiguration _config;

        public frmCntX_ConsultaCuentasDB(IConfiguration config)
        {
            _config = config;
        }

        public List<CtnxCuentasDto> ObtenerCuentas(int CodEmpresa, CuentaVarModel cuenta)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            string Query = string.Empty;
            List<CtnxCuentasDto> info = new List<CtnxCuentasDto>();
            switch (cuenta.Cuenta)
            {
                case "T": //Tipo de Cuenta
                    Query = $@" select cod_cuenta,cod_cuenta_Mask, descripcion,acepta_movimientos, COD_DIVISA  
                                            from CntX_Cuentas where cuenta_madre = ''
                                      and cod_contabilidad = '{cuenta.Contabilidad}'  
                                      and TIPO_CUENTA = '{cuenta.Cuenta}' order by cod_cuenta";
                    break;
                default:
                    string procedure = "spCntX_Consulta_Cuentas";
                    var values = new
                    {
                        Contabilidad = cuenta.Contabilidad,
                        Cuenta = cuenta.Cuenta,
                        Descripcion = cuenta.Descripcion,
                        Divisa = cuenta.Divisa,
                        Nivel = cuenta.Nivel
                    };
                    try
                    {
                        using var connection = new SqlConnection(clienteConnString);
                        info = connection.Query<CtnxCuentasDto>(procedure, values, commandType: CommandType.StoredProcedure).ToList();

                    }
                    catch (Exception ex)
                    {
                        _ = ex.Message;
                    }
                    return info;


                    //Query = $@"select cod_cuenta,cod_cuenta_Mask,descripcion,acepta_movimientos from CntX_Cuentas 
                    //                 where cuenta_madre = '{cuenta.cuentaMadre}'
                    //                  and cod_contabilidad = '{cuenta.Contabilidad}' order by cod_cuenta";

            }

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    info = connection.Query<CtnxCuentasDto>(Query).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;

            //return resp;
        }

        public List<CtnxCuentasArbolModel> ObtenerCuentasArbol(int CodEmpresa, CuentaVarModel cuenta)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<CtnxCuentasDto> info = new List<CtnxCuentasDto>();
            List<CtnxCuentasArbolModel> resp = new List<CtnxCuentasArbolModel>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select cod_cuenta,cuenta_madre,cod_cuenta_Mask,descripcion,acepta_movimientos 
                                    from CntX_Cuentas where
                                    cod_contabilidad = '{cuenta.Contabilidad}' order by cod_cuenta";
                    info = connection.Query<CtnxCuentasDto>(query).ToList();
                }

                foreach (CtnxCuentasDto item in info)
                {
                    if (item.cuenta_madre == "")
                    {
                        resp.Add(new CtnxCuentasArbolModel
                        {
                            Key = item.cod_cuenta,
                            Label = item.cod_cuenta_Mask + "-" + item.descripcion,
                            Data = item.cod_cuenta_Mask,
                            Icon = "pi pi-fw pi-folder",
                            ExpandedIcon = "pi pi-fw pi-folder-open",
                            CollapsedIcon = "pi pi-fw pi-folder",
                            Children = AddCuentasArbol(info, item),
                            leaf = (AddCuentasArbol(info, item).Count == 0) ? true : false
                        });
                        // A;
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public List<DropDownListaGenericaModel> ObtenerDivisas(int CodEmpresa, int Contavilidad)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<DropDownListaGenericaModel> info = new List<DropDownListaGenericaModel>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select rtrim(cod_divisa) as 'item', rtrim(descripcion) as 'descripcion' 
                                    from CntX_Divisas where cod_contabilidad = '{Contavilidad}' order by divisa_local desc";
                    info = connection.Query<DropDownListaGenericaModel>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }
        public List<DropDownListaGenericaModel> ObtenerTiposCuentas(int CodEmpresa, int Contavilidad)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<DropDownListaGenericaModel> info = new List<DropDownListaGenericaModel>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select TIPO_CUENTA as 'item',Descripcion from CntX_Tipos_Cuentas 
                                      where cod_contabilidad = '{Contavilidad}' order by Prioridad,Tipo_cuenta";
                    info = connection.Query<DropDownListaGenericaModel>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }

        public List<CtnxCuentasArbolModel> AddCuentasArbol(List<CtnxCuentasDto> cuentas, CtnxCuentasDto cuenta)
        {
            List<CtnxCuentasArbolModel> resp = new List<CtnxCuentasArbolModel>();
            foreach (var item in cuentas)
            {
                if (item.cuenta_madre == cuenta.cod_cuenta)
                {
                    resp.Add(new CtnxCuentasArbolModel
                    {
                        Key = item.cod_cuenta,
                        Label = item.cod_cuenta_Mask + "-" + item.descripcion,
                        Data = item.cod_cuenta_Mask,
                        Icon = "pi pi-fw pi-folder",
                        ExpandedIcon = "pi pi-fw pi-folder-open",
                        CollapsedIcon = "pi pi-fw pi-folder",
                        Children = AddCuentasArbol(cuentas, item),
                        leaf = (AddCuentasArbol(cuentas, item).Count == 0) ? true : false
                    });
                }
            }
            return resp;
        }

    }
}
