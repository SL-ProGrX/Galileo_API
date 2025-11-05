using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.INV;
using System.Data;


namespace PgxAPI.DataBaseTier
{
    public class frmInvDepartamentosDB
    {
        private readonly IConfiguration _config;

        public frmInvDepartamentosDB(IConfiguration config)
        {
            _config = config;
        }

        #region Departamentos


        /// <summary>
        /// Obtiene la lista lazy de departamentos 
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="pagina"></param>
        /// <param name="paginacion"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDTO<DepartamentosDataLista> Departamentos_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            var response = new ErrorDTO<DepartamentosDataLista>
            {
                Result = new DepartamentosDataLista()
            };
            response.Result.Total = 0;
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);
                {

                    //Busco Total
                    query = "SELECT COUNT(*) FROM pv_Departamentos";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    if (filtro != null)
                    {
                        filtro = " WHERE COD_departamento LIKE '%" + filtro + "%' OR DESCRIPCION LIKE '%" + filtro + "%' ";
                    }

                    if (pagina != null)
                    {
                        paginaActual = " OFFSET " + pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
                    }

                    query = $@"SELECT COD_departamento,descripcion, activo
                                       FROM pv_Departamentos
                                         {filtro} 
                                        ORDER BY COD_departamento
                                        {paginaActual}
                                        {paginacionActual} ";


                    response.Result.Departamentos = connection.Query<DepartamentosDTO>(query).ToList();

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
        /// Actualiza el departamento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDTO Departamentos_Actualizar(int CodEmpresa, DepartamentosDTO request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDTO resp = new();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "Update pv_Departamentos set descripcion = @Descripcion, activo = @Activo where cod_departamento = @Cod_Departamento";

                    var parameters = new DynamicParameters();
                    parameters.Add("Cod_Departamento", request.Cod_Departamento, DbType.String);
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
        /// Inserta un nuevo departamento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDTO Departamentos_Insertar(int CodEmpresa, DepartamentosDTO request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO resp = new();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "insert into pv_Departamentos(cod_departamento, descripcion, activo)values(@Cod_Departamento, @Descripcion, @Activo)";

                    var parameters = new DynamicParameters();
                    parameters.Add("cod_departamento", request.Cod_Departamento, DbType.String);
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
        /// Elimina un departamento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="departamento"></param>
        /// <returns></returns>
        public ErrorDTO Departamentos_Eliminar(int CodEmpresa, string departamento)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDTO resp = new();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "DELETE pv_Departamentos where cod_departamento = @cod_departamento";

                    var parameters = new DynamicParameters();
                    parameters.Add("cod_departamento", departamento, DbType.String);

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

        #endregion


        #region Asignaciones

        public ErrorDTO<List<AsignacionesDTO>> Asignaciones_ObtenerTodos(int CodEmpresa, string departamento)
        {

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDTO<List<AsignacionesDTO>>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "SELECT c.cod_prodclas, c.descripcion, c.cod_alter, c.costeo, c.cod_cuenta, c.valuacion, l.cod_departamento  " +
                        "FROM pv_prod_clasifica c left join pv_lineasdep l " +
                        "on C.cod_prodclas = L.cod_prodclas " +
                        "and L.cod_departamento = @cod_departamento " +
                        "order by l.cod_departamento desc";


                    var parameters = new DynamicParameters();
                    parameters.Add("cod_departamento", departamento, DbType.String);

                    response.Result = connection.Query<AsignacionesDTO>(query, parameters).ToList();
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

        public ErrorDTO Asignaciones_Insertar(int CodEmpresa, AsignacionesDTO request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO resp = new();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "insert PV_LINEASDEP(cod_departamento,cod_prodclas)values(@Cod_Departamento, @Cod_Prodclas)";

                    var parameters = new DynamicParameters();
                    parameters.Add("Cod_Departamento", request.Cod_Departamento, DbType.String);
                    parameters.Add("Cod_Prodclas", request.Cod_Prodclas, DbType.String);

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

        public ErrorDTO Asignaciones_Eliminar(int CodEmpresa, string Cod_Departamento, string Cod_Prodclas)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDTO resp = new();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "delete PV_LINEASDEP where cod_departamento = @cod_departamento and cod_prodclas = @cod_prodclas";

                    var parameters = new DynamicParameters();
                    parameters.Add("cod_departamento", Cod_Departamento, DbType.String);
                    parameters.Add("cod_prodclas", Cod_Prodclas, DbType.String);

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

        #endregion
    }
}
