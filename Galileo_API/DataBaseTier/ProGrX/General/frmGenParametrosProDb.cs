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
        public ErrorDto<PvParametrosModDto> Obtener_ParamaterosPro(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<PvParametrosModDto>
            {
                Result = new PvParametrosModDto
                {
                    COD_PAR = 0,
                    CHK_FACTURA_MIN = 0,
                    CHK_DESCUENTO_BIFIV = 0,
                    CHK_COSTO_ULTCOMP = 0,
                    CHK_COSTO_CERO = 0,
                    CHK_MODO_ASIENTO = 0
                }
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                const string query = "select * from pv_parametros_mod";
                resp.Result = connection.Query<PvParametrosModDto>(query).FirstOrDefault();
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
        public ErrorDto ParamaterosPro_ActualizaGen(int CodEmpresa, PvParametrosModDto pvParametrosMod)
        {
            var resp = new ErrorDto();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                const string query = @"
                    UPDATE pv_parametros_mod SET
                          chk_factura_min     = @CHK_FACTURA_MIN
                        , chk_descuento_bifIV = @CHK_DESCUENTO_BIFIV
                        , chk_costo_UltComp   = @CHK_COSTO_ULTCOMP
                        , chk_costo_cero      = @CHK_COSTO_CERO
                        , chk_modo_asiento    = @CHK_MODO_ASIENTO
                        , aplica_iv_sobre     = @APLICA_IV_SOBRE";

                resp.Code = connection.Execute(query, new
                {
                    pvParametrosMod.CHK_FACTURA_MIN,
                    pvParametrosMod.CHK_DESCUENTO_BIFIV,
                    pvParametrosMod.CHK_COSTO_ULTCOMP,
                    pvParametrosMod.CHK_COSTO_CERO,
                    pvParametrosMod.CHK_MODO_ASIENTO,
                    pvParametrosMod.APLICA_IV_SOBRE
                });
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
        public ErrorDto ParamaterosPro_ActualizaCxP(int CodEmpresa, PvParametrosModDto pvParametrosMod)
        {
            var resp = new ErrorDto();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                const string query = @"
                    UPDATE pv_parametros_mod SET
                          cxp_tc_nc   = @CXP_TC_NC
                        , cxp_tc_nd   = @CXP_TC_ND
                        , cxp_tc_pago = @CXP_TC_PAGO";

                resp.Code = connection.Execute(query, new
                {
                    pvParametrosMod.CXP_TC_NC,
                    pvParametrosMod.CXP_TC_ND,
                    pvParametrosMod.CXP_TC_PAGO
                });
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
        public ErrorDto ParamaterosPro_ActualizaInv(int CodEmpresa, PvParametrosModDto pvParametrosMod)
        {
            var resp = new ErrorDto();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                const string query = @"
                    UPDATE pv_parametros_mod SET
                          inv_tc_entrada  = @INV_TC_ENTRADA
                        , inv_tc_salida   = @INV_TC_SALIDA
                        , inv_tc_traslado = @INV_TC_TRASLADO
                        , inv_tc_compra   = @INV_TC_TRASLADO"; // ojo: se mantiene la lógica original

                resp.Code = connection.Execute(query, new
                {
                    pvParametrosMod.INV_TC_ENTRADA,
                    pvParametrosMod.INV_TC_SALIDA,
                    pvParametrosMod.INV_TC_TRASLADO
                });
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
        public ErrorDto ParamaterosPro_ActualizaPos(int CodEmpresa, PvParametrosModDto pvParametrosMod)
        {
            var resp = new ErrorDto();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                const string query = @"
                    UPDATE pv_parametros_mod SET
                          pos_tc_factura = @POS_TC_FACTURA
                        , pos_tc_recibo  = @POS_TC_RECIBO";

                resp.Code = connection.Execute(query, new
                {
                    pvParametrosMod.POS_TC_FACTURA,
                    pvParametrosMod.POS_TC_RECIBO
                });
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
        public ErrorDto ParametrosGen_Insertar(int CodEmpresa)
        {
            var resp = new ErrorDto();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                const string query = @"
                    INSERT pv_parametros_mod
                        (chk_factura_min, chk_Descuento_Bifiv, chk_costo_ultComp, Chk_Costo_cero,
                         chk_modo_asiento, aplica_iv_sobre,
                         cxp_tc_nc, cxp_tc_nd, cxp_tc_pago,
                         inv_tc_entrada, inv_tc_salida, inv_tc_traslado, inv_tc_compra,
                         pos_tc_factura, pos_tc_recibo,
                         pos_rei_user, pos_rei_clave, pos_cp_user, pos_cp_clave,
                         tc_compra, tc_venta, tc_fecha, tc_usuario)
                    VALUES
                        (0, 1, 0, 1,
                         1, 'SB',
                         'C', 'V', 'C',
                         'C', 'C', 'C', 'C',
                         'V', 'C',
                         '', '', '', '',
                         0, 0, GETDATE(), 'InI')";

                resp.Code = connection.Execute(query);
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