using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.INV;
using System.Data;


namespace PgxAPI.DataBaseTier
{
    public class frmInvUnidadesConvDB
    {
        private readonly IConfiguration _config;

        public frmInvUnidadesConvDB(IConfiguration config)
        {
            _config = config;
        }


        /// <summary>
        /// Obtiene la lista lazy de unidades 
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDto<List<UnidadMedicionConv>>  UnidadMedicion_Obtener(int CodCliente)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<UnidadMedicionConv>>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "SELECT COD_UNIDAD AS ITEM, DESCRIPCION FROM PV_UNIDADES WHERE ACTIVO = 1 ";
                    response.Result = connection.Query<UnidadMedicionConv>(query).ToList();
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

        public ErrorDto<UnidadesConvLista> UnidadConvLista_Obtener(int CodCliente, string cod_unidad)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<UnidadesConvLista>
            {
                Code = 0,
                Description = "",
                Result = new UnidadesConvLista()
            };
            
            response.Result.total = 0;
            response.Result.lista = new List<UnidadMedicionConvData>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"SELECT * FROM PV_UNIDADES_CONV WHERE COD_UNIDAD = '{cod_unidad}' ";
                    response.Result.lista = connection.Query<UnidadMedicionConvData>(query).ToList();

                    response.Result.total = response.Result.lista.Count;
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


        public ErrorDto UnidadConv_Guardar(int CodCliente, UnidadMedicionConvData equivalencia)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto resp = new();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //valido si la equivalencia ya existe

                    var query = $@"SELECT COUNT(*) FROM PV_UNIDADES_CONV WHERE 
                            COD_UNIDAD = '{equivalencia.cod_unidad}' 
                            AND COD_UNIDAD_D = '{equivalencia.cod_unidad_d}' ";
                    var count = connection.ExecuteScalar<int>(query);

                    if (count > 0)
                    {
                        //actualizo
                        query = $@"UPDATE PV_UNIDADES_CONV SET FACTOR = {equivalencia.factor} 
                                WHERE COD_UNIDAD = '{equivalencia.cod_unidad}' 
                                AND COD_UNIDAD_D = '{equivalencia.cod_unidad_d}' ";
                        resp.Code = connection.Execute(query);
                    }
                    else
                    {
                        query = $@"INSERT INTO PV_UNIDADES_CONV (COD_UNIDAD, COD_UNIDAD_D, FACTOR) 
                                VALUES ('{equivalencia.cod_unidad}', '{equivalencia.cod_unidad_d}', {equivalencia.factor}) ";
                        resp.Code = connection.Execute(query);
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

        public ErrorDto UnidadConv_Eliminar(int CodCliente, string cod_unidad, string cod_unidad_d)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto resp = new();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"DELETE FROM PV_UNIDADES_CONV WHERE 
                            COD_UNIDAD = '{cod_unidad}' 
                            AND COD_UNIDAD_D = '{cod_unidad_d}' ";
                    resp.Code = connection.Execute(query);
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
