using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.AH;
using PgxAPI.Models.ERROR;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmAH_ConfiguracionDB
    {
        private readonly IConfiguration _config;

        public frmAH_ConfiguracionDB(IConfiguration config)
        {
            _config = config;
        }


        public ParametrosPatrimonioDto ParametrosPatrimonio_Obtener(int CodEmpresa, string Divisa)
        {
            ParametrosPatrimonioDto resp = new ParametrosPatrimonioDto();

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);


                using var connection = new SqlConnection(stringConn);
                {
                    var procedure = "[spPAT_Parametros]";
                    var values = new
                    {
                        Divisa = Divisa,
                    };


                    resp = connection.Query<ParametrosPatrimonioDto>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public ErrorDto Actualiza_ConfiguracionPatrimonio(int CodEmpresa, ParametrosPatrimonioDto data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"UPDATE PAR_AFAH
                                            cta_obrero = '{data.cta_obrero}',
                                            cta_patronal = '{data.cta_patronal}',
                                            cta_custodia = '{data.cta_custodia}',
                                            cta_capitaliza = '{data.cta_capitaliza}',
                                            cta_devoluciones = '{data.cta_devoluciones}',
                                            cta_liqpas = '{data.cta_liqpas}',
                                            cta_devoluciones = '{data.cta_devoluciones}',
                                            cta_rentacap = '{data.cta_rentacap}',
                                            cta_edst_mask = '{data.cta_edst_mask}',
                                            cta_rentacap = '{data.cta_rentacap}',
                                            cta_ecxc_mask = '{data.cta_ecxc_mask}',
                                            cta_ecxp_mask = '{data.cta_ecxc_mask}',
                                            cta_enc_mask = '{data.cta_enc_mask}',
                                            cta_edon_mask = '{data.cta_edon_mask}',
                                            cta_ereserva_mask = '{data.cta_ereserva_mask}',
                                            cta_epg_mask = '{data.cta_epg_mask}'";

                    resp.Code = connection.Query<int>(query).FirstOrDefault();
                    resp.Description = "Ok";

                    //if (resp.Code == 0)
                    //{
                    //    Bitacora(new BitacoraInsertarDto
                    //    {
                    //        EmpresaId = CodEmpresa,
                    //        Usuario = data.Creacion_User,
                    //        DetalleMovimiento = "CxP Factura: " + data.Cod_Factura + " Prov: " + data.Cod_Proveedor,
                    //        Movimiento = "REGISTRA - WEB",
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


    }
}