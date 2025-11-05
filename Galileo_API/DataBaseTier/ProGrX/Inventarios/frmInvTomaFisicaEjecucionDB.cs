using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.INV;

namespace PgxAPI.DataBaseTier
{
    public class frmInvTomaFisicaEjecucionDB
    {
        private readonly IConfiguration _config;

        public frmInvTomaFisicaEjecucionDB(IConfiguration config)
        {
            _config = config;
        }


        public ErrorDTO<List<EntradasTomaFisicaDTO>> Obtener_Entradas(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDTO<List<EntradasTomaFisicaDTO>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = "SELECT RTRIM(cod_entsal) AS Codigo, RTRIM(descripcion) AS Descripcion\r\nFROM pv_entrada_salida\r\nWHERE tipo = 'E'";

                    response.Result = connection.Query<EntradasTomaFisicaDTO>(query).ToList();

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
        public ErrorDTO<List<SalidasTomaFisicaDTO>> Obtener_Salidas(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDTO<List<SalidasTomaFisicaDTO>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = "SELECT RTRIM(cod_entsal) AS Codigo, RTRIM(descripcion) AS Descripcion\r\nFROM pv_entrada_salida\r\nWHERE tipo = 'S';";

                    response.Result = connection.Query<SalidasTomaFisicaDTO>(query).ToList();

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
        public ErrorDTO ProcesarTomaFisica(int CodEmpresa, int consecutivo, string usuario, string cod_entrada, string cod_salida)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO info = new ErrorDTO();
            info.Code = 0;

            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"UPDATE PV_INVTOMAFISICA
                                                SET 
                                                    ESTADO = 'P', 
                                                    FECHA_APLICA = GETDATE(), 
                                                    USER_APLICA = '{usuario}', 
                                                    CAUSA_ENTRADA = '{cod_entrada}', 
                                                    CAUSA_SALIDA = '{cod_salida}',
                                                    COD_ENTRADAG = 0, 
                                                    COD_SALIDAG = 0

                                                WHERE 
                                                    CONSECUTIVO = {consecutivo};";
                    info.Code = connection.Execute(query);


                    var query2 = $@"SELECT COD_PRODUCTO, EXISTENCIA_FISICA FROM pv_InvTomaFisica AS tomaFisica
                                            JOIN pv_invTF_Detalle AS tomaFisicaDetalle
                                              ON tomaFisicaDetalle.consecutivo = tomaFisica.consecutivo
                                            WHERE tomaFisica.consecutivo = {consecutivo}";

                    var productos = connection.Query<ProductosTomaFisica>(query2).ToList();

                    foreach (var producto in productos)
                    {
                        var updateQuery = $@"UPDATE PV_PRODUCTOS
                         SET Existencia = Existencia - {producto.existencia_fisica}
                         WHERE cod_producto = '{producto.cod_producto}'";

                        info.Code = connection.Execute(updateQuery);
                    }

                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message.ToString();
            }
            return info;
        }
    }



}