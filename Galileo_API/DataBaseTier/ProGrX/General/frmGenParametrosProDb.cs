using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.GEN;
using Microsoft.Data.SqlClient;


namespace Galileo.DataBaseTier
{
    public class FrmGenParametrosProDb
    {
        private readonly IConfiguration _config;

        public FrmGenParametrosProDb(IConfiguration config)
        {
            _config = config;
        }


        /// <summary>
        /// Obtiene los parametros generales
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<PvParametrosModDto> Obtener_ParamaterosPro(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<PvParametrosModDto>();
            resp.Result = new PvParametrosModDto
            {
                COD_PAR = 0,
                CHK_FACTURA_MIN = 0,
                CHK_DESCUENTO_BIFIV = 0,
                CHK_COSTO_ULTCOMP = 0,
                CHK_COSTO_CERO = 0,
                CHK_MODO_ASIENTO = 0
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "select * from pv_parametros_mod";
                    resp.Result = connection.Query<PvParametrosModDto>(query).FirstOrDefault();
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


        /// <summary>
        /// Actualiza los parametros generales
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="pvParametrosMod"></param>
        /// <returns></returns>
        public ErrorDto ParamaterosPro_ActualizaGen(int CodEmpresa, PvParametrosModDto pvParametrosMod)
        {
            ErrorDto resp = new ErrorDto();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"update pv_parametros_mod set
                                    chk_factura_min = {pvParametrosMod.CHK_FACTURA_MIN} 
                                    ,chk_descuento_bifIV = {pvParametrosMod.CHK_DESCUENTO_BIFIV} 
                                    ,chk_costo_UltComp = {pvParametrosMod.CHK_COSTO_ULTCOMP}
                                    ,chk_costo_cero = {pvParametrosMod.CHK_COSTO_CERO}
                                    ,chk_modo_asiento = {pvParametrosMod.CHK_MODO_ASIENTO}
                                    ,aplica_iv_sobre = '{pvParametrosMod.APLICA_IV_SOBRE}' ";
                    resp.Code = connection.Query<int>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;

        }


        /// <summary>
        /// Actualiza los parametros de CxP
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="pvParametrosMod"></param>
        /// <returns></returns>
        public ErrorDto ParamaterosPro_ActualizaCxP(int CodEmpresa, PvParametrosModDto pvParametrosMod)
        {
            ErrorDto resp = new ErrorDto();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"update pv_parametros_mod set
                                    cxp_tc_nc = '{pvParametrosMod.CXP_TC_NC}' 
                                    ,cxp_tc_nd = '{pvParametrosMod.CXP_TC_ND}' 
                                    ,cxp_tc_pago = '{pvParametrosMod.CXP_TC_PAGO}' ";
                    resp.Code = connection.Query<int>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;

        }


        /// <summary>
        /// Actualiza los parametros de Inventario
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="pvParametrosMod"></param>
        /// <returns></returns>
        public ErrorDto ParamaterosPro_ActualizaInv(int CodEmpresa, PvParametrosModDto pvParametrosMod)
        {
            ErrorDto resp = new ErrorDto();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"update pv_parametros_mod set
                                    inv_tc_entrada = '{pvParametrosMod.INV_TC_ENTRADA}' 
                                    ,inv_tc_salida = '{pvParametrosMod.INV_TC_SALIDA}' 
                                    ,inv_tc_traslado = '{pvParametrosMod.INV_TC_TRASLADO}'
                                    ,inv_tc_compra = '{pvParametrosMod.INV_TC_TRASLADO}' ";
                    resp.Code = connection.Query<int>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;

        }


        /// <summary>
        /// Actualiza los parametros de POS
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="pvParametrosMod"></param>
        /// <returns></returns>
        public ErrorDto ParamaterosPro_ActualizaPos(int CodEmpresa, PvParametrosModDto pvParametrosMod)
        {
            ErrorDto resp = new ErrorDto();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"update pv_parametros_mod set
                                    pos_tc_factura = '{pvParametrosMod.POS_TC_FACTURA}' 
                                    ,pos_tc_recibo = '{pvParametrosMod.POS_TC_RECIBO}' 
                                    ";
                    resp.Code = connection.Query<int>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;

        }


        /// <summary>
        /// Inserta los parametros generales
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto ParametrosGen_Insertar(int CodEmpresa)
        {
            ErrorDto resp = new ErrorDto();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"insert pv_parametros_mod(chk_factura_min,chk_Descuento_Bifiv,chk_costo_ultComp,Chk_Costo_cero
                                  ,chk_modo_asiento,aplica_iv_sobre,cxp_tc_nc,cxp_tc_nd,cxp_tc_pago,inv_tc_entrada,inv_tc_salida
                                  ,inv_tc_traslado,inv_tc_compra,pos_tc_factura,pos_tc_recibo,pos_rei_user,pos_rei_clave,pos_cp_user,pos_cp_clave
                                  ,tc_compra,tc_venta,tc_fecha,tc_usuario) values(0,1,0,1,1,'SB','C','V','C','C','C','C','C','V'
                                  ,'C','','','','',0,0,Getdate(),'InI')";
                    resp.Code = connection.Query<int>(query).FirstOrDefault();
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