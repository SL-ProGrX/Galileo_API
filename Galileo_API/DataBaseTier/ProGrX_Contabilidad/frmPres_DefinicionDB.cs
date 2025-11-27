using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.PRES;

namespace Galileo.DataBaseTier
{
    public class FrmPresDefinicionDb
    {
        private readonly IConfiguration _config;
        const string _consolidado = "CONSOLIDADO";

        public FrmPresDefinicionDb(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<List<ModeloGenericList>> Pres_Modelos_Obtener(int CodEmpresa, string usuario, int codContab)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<ModeloGenericList>>
            {
                Code = 0,
                Result = new List<ModeloGenericList>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"
                                SELECT P.cod_modelo AS IdX, P.DESCRIPCION AS ItmX, Cc.Inicio_Anio
                                FROM PRES_MODELOS P 
                                INNER JOIN PRES_MODELOS_USUARIOS Pmu 
                                    ON P.cod_Contabilidad = Pmu.cod_contabilidad 
                                    AND P.cod_Modelo = Pmu.cod_Modelo 
                                    AND Pmu.Usuario = '{usuario}' 
                                INNER JOIN CNTX_CIERRES Cc 
                                    ON P.cod_Contabilidad = Cc.cod_Contabilidad 
                                    AND P.ID_CIERRE = Cc.ID_CIERRE 
                                WHERE P.COD_CONTABILIDAD = {codContab} 
                                GROUP BY P.cod_Modelo, P.Descripcion, Cc.Inicio_Anio 
                                ORDER BY Cc.INICIO_ANIO DESC, P.Cod_Modelo";
                    resp.Result = connection.Query<ModeloGenericList>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }
            return resp;
        }

        public ErrorDto<List<ModeloGenericList>> Pres_Modelo_Unidades_Obtener(int CodEmpresa, string codModelo, int codContab, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<ModeloGenericList>>
            {
                Code = 0,
                Result = new List<ModeloGenericList>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"exec spPres_Modelo_Unidades {codContab},'{codModelo}','{usuario}'";
                    resp.Result = connection.Query<ModeloGenericList>(query).ToList();

                    resp.Result.RemoveAll(X => string.IsNullOrWhiteSpace(X.IdX));
                    resp.Result.Add(new ModeloGenericList { IdX = _consolidado, ItmX = _consolidado });
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }
            return resp;
        }

        public ErrorDto<List<ModeloGenericList>> Pres_Modelo_Unidades_CC_Obtener(int CodEmpresa, string codModelo, int codContab, string codUnidad)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<ModeloGenericList>>
            {
                Code = 0,
                Result = new List<ModeloGenericList>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    if(codUnidad == _consolidado)
                    {
                        codUnidad = "CONS";
                    }

                    var query = $@"exec spPres_Modelo_Unidades_CC {codContab},'{codModelo}','{codUnidad}'";
                    resp.Result = connection.Query<ModeloGenericList>(query).ToList();

                    resp.Result.RemoveAll(X => string.IsNullOrWhiteSpace(X.IdX));
                    resp.Result.Add(new ModeloGenericList { IdX = "TODOS", ItmX = "TODOS" });
                    resp.Result.Add(new ModeloGenericList { IdX = _consolidado, ItmX = _consolidado });
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }
            return resp;
        }

        public ErrorDto<CntxCuentasData> Pres_Definicion_scroll(int CodEmpresa, int scrollValue, string? CodCtaMask, int CodContab)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<CntxCuentasData>
            {
                Code = 0,
                Result = new CntxCuentasData()
            };
            try
            {
                string filtro = $"where COD_CONTABILIDAD = '{CodContab}' and Acepta_Movimientos = 1 ";

                if (scrollValue == 1)
                {
                    filtro += $"and Cod_Cuenta_Mask > '{CodCtaMask}' order by Cod_Cuenta_Mask asc";
                }
                else
                {
                    filtro += $"and Cod_Cuenta_Mask < '{CodCtaMask}' order by Cod_Cuenta_Mask desc";
                }

                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select Top 1 Cod_Cuenta_Mask,Descripcion from CntX_cuentas {filtro}";
                    resp.Result = connection.Query<CntxCuentasData>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }
            return resp;
        }

        public ErrorDto<List<VistaPresCuentaData>> Pres_VistaPresupuesto_Cuenta_SP(int CodEmpresa, PresCuenta request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<VistaPresCuentaData>>
            {
                Code = 0,
                Result = new List<VistaPresCuentaData>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spPres_VistaPresupuesto_Cuenta {request.Cod_Contabilidad},'{request.Cod_Modelo}',
                        '{request.Cod_Unidad}','{request.Cod_Centro_Costo}','{request.Cod_Cuenta}','{request.Vista}'";
                    resp.Result = connection.Query<VistaPresCuentaData>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }
            return resp;
        }

        public ErrorDto<CuentasLista> Pres_Cuentas_Obtener(int CodEmpresa, string cod_contabilidad, int? pagina, int? paginacion, string? filtro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<CuentasLista>
            {
                Code = 0,
                Result = new CuentasLista()
            };
            resp.Result.total = 0;
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";

                using var connection = new SqlConnection(clienteConnString);
                {
                    //Busco Total
                    query = $@"Select COUNT(cod_cuenta) from CntX_cuentas where cod_contabilidad = '{cod_contabilidad}' and acepta_movimientos = 1 ";
                    resp.Result.total = connection.Query<int>(query).FirstOrDefault();

                    if (filtro != null)
                    {
                        filtro = " AND Cod_Cuenta_Mask LIKE '%" + filtro + "%' OR descripcion LIKE '%" + filtro + "%' ";
                    }

                    if (pagina != null)
                    {
                        paginaActual = " OFFSET " + pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
                    }

                    query = $@"Select Cod_Cuenta_Mask,descripcion
                                from CntX_cuentas where cod_contabilidad = '{cod_contabilidad}' and acepta_movimientos = 1 {filtro}  
                                ORDER BY Cod_Cuenta_Mask
                                {paginaActual} {paginacionActual}";
                    resp.Result.lista = connection.Query<CntxCuentasData>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

    }
}