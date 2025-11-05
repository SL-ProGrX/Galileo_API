using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Bancos;

namespace PgxAPI.DataBaseTier.ProGrX.Bancos
{
    public class frmTES_TipoCambioDB
    {
        private readonly IConfiguration? _config;
        private readonly mProGrX_AuxiliarDB _utils;
        private readonly mProGrx_Main _grx_Main;

        public frmTES_TipoCambioDB(IConfiguration config)
        {
            _config = config;
            _utils = new mProGrX_AuxiliarDB(_config);
            _grx_Main = new mProGrx_Main(_config);
        }

        /// <summary>
        /// Obtiene el tipo de cambio de divisas para una empresa y divisa específica.
        /// </summary>
        /// <param name="tipoCambio"></param>
        /// <returns></returns>
        public ErrorDTO<TesTipoCambioDivisasTipoCambio> Tes_TipoCambio_Obtener(TesTipoCambioConsulta tipoCambio)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(tipoCambio.CodEmpresa);
            var response = new ErrorDTO<TesTipoCambioDivisasTipoCambio>
            {
                Code = 0,
                Description = "OK",
                Result = new TesTipoCambioDivisasTipoCambio()
            };
            try
            {
                var query = @$"";
                using var connection = new SqlConnection(stringConn);
                {

                    /*
                     TCPermitido = "Tipo de Cambio Permitido" (tc_Compra)
                     TCVariacion = "Variación del Tipo de Cambio" (variacion)
                     txtTC = "Tipo de Cambio Actual" (tc_Compra)
                     */
                    string fecha = _utils.validaFechaGlobal(DateTime.Now);
                    query = @$"select tc_venta,tc_Compra,variacion from CntX_Divisas_Tipo_Cambio 
                                    where cod_contabilidad = @cod_contabilidad and cod_divisa = @cod_divisa 
                                       and @fecha between inicio and corte  ";

                    response.Result = connection.QueryFirstOrDefault<TesTipoCambioDivisasTipoCambio>(query, new
                    {
                        cod_contabilidad = tipoCambio.contabilidad,
                        cod_divisa = tipoCambio.divisa, 
                        fecha = fecha
                    });

                    if(response.Result != null)
                    {
                        response.Result.tc_actual = response.Result.tc_Compra;
                    }
                    else
                    {
                        query = $@"SELECT D.*,X.Descripcion  from CNTX_DIVISAS_TIPO_CAMBIO D inner join  
                                        CNTX_DIVISAS X on D.COD_DIVISA = X.COD_DIVISA where  D.COD_CONTABILIDAD = @cod_contabilidad
                                        and D.cod_divisa = @cod_divisa  order by corte desc";

                        response.Result = connection.QueryFirstOrDefault<TesTipoCambioDivisasTipoCambio>(query, new
                        {
                            cod_contabilidad = tipoCambio.contabilidad,
                            cod_divisa = tipoCambio.divisa
                        });

                        if (response.Result != null)
                        {
                            response.Result.tc_actual = response.Result.tc_Compra;
                        }
                        else
                        {
                            response.Code = 1;
                            response.Description = "No se encontró el tipo de cambio para la divisa especificada.";

                            response.Result = new TesTipoCambioDivisasTipoCambio();


                            response.Result.tc_Compra = tipoCambio.tcPermitido;
                            response.Result.variacion = tipoCambio.tcVariacion;
                            response.Result.tc_venta = tipoCambio.tc_actual;
                            response.Result.tc_actual = tipoCambio.tc_actual;


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
        /// Valida el tipo de cambio que tenga el formato correcto.
        /// </summary>
        /// <param name="pTipoCambio"></param>
        /// <returns></returns>
        public ErrorDTO<double> Tes_TipoCambio_MontoCambiar(decimal pTipoCambio)
        {
            var response = new ErrorDTO<double>
            {
                Code = 0,
                Description = "OK",
                Result = 0
            };
            try
            {
                response.Result = _grx_Main.fxSys_Tipo_Cambio_Apl(pTipoCambio);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = 0;
            }
            return response;
        }

        /// <summary>
        /// Obtiene la descripción de una divisa específica.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_divisa"></param>
        /// <returns></returns>
        public ErrorDTO<string> Tes_tipoCambioDivisa_Obterner(int CodEmpresa, string cod_divisa)
        {
            var response = new ErrorDTO<string>
            {
                Code = 0,
                Description = "OK",
                Result = string.Empty
            };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                {
                    string query = @"select Descripcion from CntX_Divisas where cod_divisa = @cod_divisa";
                    response.Result = connection.QueryFirstOrDefault<string>(query, new { cod_divisa });
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }
    
    }
}
