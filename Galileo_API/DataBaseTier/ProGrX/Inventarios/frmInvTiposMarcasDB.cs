using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.INV;
using System.Data;


namespace PgxAPI.DataBaseTier
{
    public class frmInvTiposMarcasDB
    {
        private readonly IConfiguration _config;

        public frmInvTiposMarcasDB(IConfiguration config)
        {
            _config = config;
        }


        /// <summary>
        /// Obtiene la lista lazy de marcas 
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="pagina"></param>
        /// <param name="paginacion"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<MarcasDataLista> Marcas_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<MarcasDataLista>();
            response.Result = new MarcasDataLista();
            response.Result.Total = 0;

            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);

                {

                    //Busco Total
                    query = "SELECT COUNT(*) FROM pv_marcas";
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

                    query = $@"SELECT cod_marca,descripcion, activo 
                                       FROM pv_marcas
                                         {filtro} 
                                        ORDER BY cod_marca
                                        {paginaActual}
                                        {paginacionActual} ";


                    response.Result.Marcas = connection.Query<MarcasDto>(query).ToList();

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
        /// Obtiene la lista de marcas 
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<MarcasDto>> Marcas_ObtenerTodos(int CodEmpresa)
        {

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<MarcasDto>>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "SELECT cod_marca,descripcion, activo  FROM pv_marcas order by cod_marca";

                    response.Result = connection.Query<MarcasDto>(query).ToList();
                    foreach (MarcasDto dt in response.Result)
                    {
                        dt.Estado = dt.Activo ? "ACTIVO" : "INACTIVO";
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
        /// Actualiza la marca
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Marcas_Actualizar(int CodEmpresa, MarcasDto request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "Update pv_marcas set descripcion = @Descripcion, activo = @Activo where Cod_Marca = @Cod_Marca";

                    var parameters = new DynamicParameters();
                    parameters.Add("Cod_Marca", request.Cod_Marca, DbType.String);
                    parameters.Add("Descripcion", request.Descripcion, DbType.String);
                    parameters.Add("Activo", request.Activo, DbType.Int32);

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
        /// Inserta la marca
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Marcas_Insertar(int CodEmpresa, MarcasDto request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "insert into pv_marcas(cod_marca, descripcion, activo)values(@Cod_Marca, @Descripcion, @Activo)";

                    var parameters = new DynamicParameters();
                    parameters.Add("Cod_Marca", request.Cod_Marca, DbType.String);
                    parameters.Add("Descripcion", request.Descripcion, DbType.String);
                    parameters.Add("Activo", request.Activo, DbType.Int32);


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
        /// Elimina la marca
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="marca"></param>
        /// <returns></returns>
        public ErrorDto Marcas_Eliminar(int CodEmpresa, string marca)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "DELETE pv_marcas where Cod_Marca = @Cod_Marca";

                    var parameters = new DynamicParameters();
                    parameters.Add("Cod_Marca", marca, DbType.String);

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
