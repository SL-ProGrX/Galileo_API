using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.INV;

namespace PgxAPI.DataBaseTier
{
    public class frmInvMargenUtilidadDB
    {
        private readonly IConfiguration _config;

        public frmInvMargenUtilidadDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDTO<List<LineaDTO>> Linea_Obtener(int CodEmpresa)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<LineaDTO>>();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = "select * From PV_PROD_CLASIFICA";

                    response.Result = connection.Query<LineaDTO>(query).ToList();
                    foreach (LineaDTO dt in response.Result)
                    {
                        dt.Descripcion = dt.Descripcion;


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

        public ErrorDTO<List<SubLineaDTO>> SubLinea_Obtener(int CodEmpresa)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<SubLineaDTO>>();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = "Select * From PV_PROD_CLASIFICA_SUB";

                    response.Result = connection.Query<SubLineaDTO>(query).ToList();
                    foreach (SubLineaDTO dt in response.Result)
                    {
                        dt.Descripcion = dt.Descripcion;


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

        public ErrorDTO<List<PrecioDTO>> ListadoPrecios_Obtener(int CodEmpresa)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<PrecioDTO>>();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = "select * From pv_tipos_precios";

                    response.Result = connection.Query<PrecioDTO>(query).ToList();


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

        public ErrorDTO cambio_margen(int CodEmpresa, int monto, int cod_linea, int cod_sublinea, string cambio_margen)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDTO resp = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    if (cambio_margen == "MU")
                    {
                        var query = $@"UPDATE pv_productos
                                        SET 
                                            precio_regular = costo_regular + (costo_regular * {monto} / 100),
                                            porc_utilidad = {monto}
                                        WHERE 
                                            estado = 'A' 
                                            AND cod_prodclas = {cod_linea}
                                            AND COD_LINEA_SUB = {cod_sublinea}";


                        resp.Code = connection.Query<int>(query).FirstOrDefault();
                        resp.Description = "Ok";

                    } else
                    {
                        var query = $@"UPDATE pv_productos 
                                        SET 
                                            porc_utilidad = {monto} * 100,
                                            PRECIO_REGULAR = P.costo_Regular + (P.costo_Regular * {monto})
                                        FROM 
                                            pv_producto_precios X
                                        INNER JOIN 
                                            pv_productos P ON P.cod_producto = X.cod_producto
                                        WHERE 
                                            P.estado = 'A' 
                                            AND P.cod_prodclas = {cod_linea}
                                            AND P.COD_LINEA_SUB = {cod_sublinea}";


                        resp.Code = connection.Query<int>(query).FirstOrDefault();
                        resp.Description = "Ok";

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