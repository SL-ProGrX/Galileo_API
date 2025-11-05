using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.INV;
using System.Data;


namespace PgxAPI.DataBaseTier
{
    public class frmInvTiposPreciosDB
    {
        private readonly IConfiguration _config;

        public frmInvTiposPreciosDB(IConfiguration config)
        {
            _config = config;
        }


        /// <summary>
        /// Obtiene la lista lazy de precios 
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="pagina"></param>
        /// <param name="paginacion"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDTO<PreciosDataLista> Precios_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<PreciosDataLista>();
            response.Result = new PreciosDataLista();
            response.Result.Total = 0;
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);
                {

                    //Busco Total
                    query = "SELECT COUNT(*) FROM pv_tipos_precios";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    if (filtro != null)
                    {
                        filtro = " WHERE cod_marca LIKE '%" + filtro + "%' OR DESCRIPCION LIKE '%" + filtro + "%' ";
                    }

                    if (pagina != null)
                    {
                        paginaActual = " OFFSET " + pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
                    }

                    query = $@"SELECT cod_precio,descripcion,defecto as activo
                                       FROM pv_tipos_precios
                                         {filtro} 
                                        ORDER BY cod_precio
                                        {paginaActual}
                                        {paginacionActual} ";


                    response.Result.Precios = connection.Query<Precio>(query).ToList();

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


        /// <summary>
        /// Obtiene la lista de precios 
        /// </summary>
        /// <returns></returns>
        public ErrorDTO<List<Precio>> Precios_ObtenerTodos(int CodEmpresa)
        {

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<Precio>>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "SELECT cod_precio,descripcion,defecto as activo  FROM pv_tipos_precios order by cod_precio";

                    response.Result = connection.Query<Precio>(query).ToList();
                    foreach (Precio dt in response.Result)
                    {
                        dt.omision = dt.activo ? "APLICA" : "NO_APLICA";
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


        /// <summary>
        /// Actualiza el tipo precio
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDTO Precios_Actualizar(int CodEmpresa, Precio request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDTO resp = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "Update pv_tipos_precios set descripcion = @Descripcion, defecto = @Defecto where cod_precio = @Cod_Precio";

                    var parameters = new DynamicParameters();
                    parameters.Add("Cod_Precio", request.Cod_Precio, DbType.String);
                    parameters.Add("Descripcion", request.Descripcion, DbType.String);
                    parameters.Add("Defecto", request.activo, DbType.Int32);

                    resp.Code = connection.ExecuteAsync(query, parameters).Result;
                    resp.Description = "Ok";
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
        /// Inserta el tipo precio
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDTO Precios_Insertar(int CodEmpresa, Precio request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO resp = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "insert into pv_tipos_precios(cod_precio, descripcion, defecto)values(@Cod_Precio, @Descripcion, @Defecto)";

                    var parameters = new DynamicParameters();
                    parameters.Add("Cod_Precio", request.Cod_Precio, DbType.String);
                    parameters.Add("Descripcion", request.Descripcion, DbType.String);
                    parameters.Add("Defecto", request.activo, DbType.Int32);


                    resp.Code = connection.ExecuteAsync(query, parameters).Result;
                    resp.Description = "Ok";
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
        /// Elimina el tipo precio
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="precio"></param>
        /// <returns></returns>
        public ErrorDTO Precios_Eliminar(int CodEmpresa, string precio)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDTO resp = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "DELETE pv_tipos_precios where Cod_Precio = @Cod_Precio";

                    var parameters = new DynamicParameters();
                    parameters.Add("Cod_Precio", precio, DbType.String);

                    resp.Code = connection.ExecuteAsync(query, parameters).Result;
                    resp.Description = "Ok";
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
