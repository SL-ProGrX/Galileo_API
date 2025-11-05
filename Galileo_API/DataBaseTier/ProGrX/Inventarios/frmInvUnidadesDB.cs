using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.INV;
using System.Data;


namespace PgxAPI.DataBaseTier
{
    public class frmInvUnidadesDB
    {
        private readonly IConfiguration _config;

        public frmInvUnidadesDB(IConfiguration config)
        {
            _config = config;
        }



        /// <summary>
        /// Obtiene la lista lazy de unidades 
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="pagina"></param>
        /// <param name="paginacion"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDTO<UnidadesDataLista>  UnidadMedicion_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<UnidadesDataLista>();
            response.Result = new UnidadesDataLista();
            response.Result.Total = 0;
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);
                {

                    //Busco Total
                    query = "SELECT COUNT(*) FROM pv_unidades";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    if (filtro != null)
                    {
                        filtro = " WHERE COD_UNIDAD LIKE '%" + filtro + "%' OR DESCRIPCION LIKE '%" + filtro + "%' ";
                    }

                    if (pagina != null)
                    {
                        paginaActual = " OFFSET " + pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
                    }

                    query = $@"SELECT cod_unidad,descripcion, ISNULL(Unidad_Hacienda_Id, 'Unid') as hacienda , activo
                                       FROM pv_unidades
                                         {filtro} 
                                        ORDER BY COD_unidad
                                        {paginaActual}
                                        {paginacionActual} ";


                    response.Result.Unidades = connection.Query<UnidadMedicionDTO>(query).ToList();

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
        /// Obtiene la lista de unidades de medición con detalle (OLD version)
        /// </summary>
        /// <returns></returns>
        public ErrorDTO<List<UnidadMedicionDTO>> UnidadMedicion_ObtenerTodosDetalle(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDTO<List<UnidadMedicionDTO>>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "SELECT cod_unidad,descripcion, ISNULL(Unidad_Hacienda_Id, 'Unid') as hacienda , activo  FROM pv_unidades order by cod_unidad";

                    response.Result = connection.Query<UnidadMedicionDTO>(query).ToList();
                    foreach (UnidadMedicionDTO dt in response.Result)
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
        /// Obtiene lista de unidades de medición para select
        /// </summary>
        /// <returns></returns>
        public ErrorDTO<List<UnidadMedicion>> UnidadMedicion_ObtenerTodos(int CodEmpresa)
        {

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDTO<List<UnidadMedicion>>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "SELECT cod_unidad, descripcion  FROM pv_unidades order by cod_unidad";

                    response.Result = connection.Query<UnidadMedicion>(query).ToList();

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
        /// Actualiza la unidad de medición
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDTO UnidadMedicion_Actualizar(int CodEmpresa, UnidadMedicionDTO request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDTO resp = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "Update pv_unidades set descripcion = @Descripcion, activo = @Activo, Unidad_Hacienda_Id = @Hacienda where Cod_Unidad = @Cod_Unidad";

                    var parameters = new DynamicParameters();
                    parameters.Add("Cod_Unidad", request.Cod_Unidad, DbType.String);
                    parameters.Add("Descripcion", request.Descripcion, DbType.String);
                    parameters.Add("Activo", request.Activo, DbType.Int32);
                    parameters.Add("Hacienda", request.Hacienda, DbType.String);


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
        /// Agrega una unidad de medición
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDTO UnidadMedicion_Agregar(int CodEmpresa, UnidadMedicionDTO request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO resp = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "insert into pv_unidades(cod_unidad, descripcion, Unidad_Hacienda_Id, activo, registro_fecha, registro_usuario)values(@Cod_Unidad, @Descripcion, @Hacienda, @Activo, GETDATE(),@Registro_Usuario)";

                    var parameters = new DynamicParameters();
                    parameters.Add("Cod_Unidad", request.Cod_Unidad, DbType.String);
                    parameters.Add("Descripcion", request.Descripcion, DbType.String);
                    parameters.Add("Activo", request.Activo, DbType.Int32);
                    parameters.Add("Hacienda", request.Hacienda, DbType.String);
                    parameters.Add("Registro_Usuario", request.Registro_Usuario, DbType.String);

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
        /// Elimina una unidad de medición
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="unidad"></param>
        /// <returns></returns>
        public ErrorDTO UnidadMedicion_Eliminar(int CodEmpresa, string unidad)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDTO resp = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "DELETE pv_unidades where Cod_Unidad = @Cod_Unidad";

                    var parameters = new DynamicParameters();
                    parameters.Add("Cod_Unidad", unidad, DbType.String);

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
