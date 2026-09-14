using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class AplicacionDB
    {
        private readonly IConfiguration _config;
        private const string connectionStringName = "DefaultConnString";
        public AplicacionDB(IConfiguration config)
        {
            _config = config;
        }


        #region MÉTODOS APP_BANK

        public ErrorDto<List<Aplicacion>> Aplicacion_ObtenerTodos()
        {
            var response = new ErrorDto<List<Aplicacion>> { Code = 0, Result = [] };
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_W_Aplicacion_Obtener]";

                    response.Result = connection.Query<Aplicacion>(procedure, commandType: CommandType.StoredProcedure).ToList();
                    foreach (Aplicacion dt in response.Result)
                    {
                        dt.Estado = (dt.Activa ?? false) ? "ACTIVO" : "INACTIVO";

                    }
                    response.Description = "Ok";
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        public ErrorDto Aplicacion_Insertar(Aplicacion request)
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_W_Aplicacion_Insertar]";
                    var values = new
                    {
                        Cod_App = request.Cod_App,
                        Descripcion = request.Descripcion,
                        Activa = request.Activa,
                        Registro_Usuario = request.Registro_Usuario,

                    };

                    resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
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

        public ErrorDto Aplicacion_Eliminar(Aplicacion request)
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_W_Aplicacion_Eliminar]";
                    var values = new
                    {
                        Cod_App = request.Cod_App,
                    };

                    resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
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

        public ErrorDto Aplicacion_Actualizar(Aplicacion request)
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_W_Aplicacion_Editar]";
                    var values = new
                    {
                        Cod_App = request.Cod_App,
                        Descripcion = request.Descripcion,
                        Activa = request.Activa,
                    };

                    resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
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

        #region MÉTODOS APP_BLOCK

        public ErrorDto<List<Bloqueo>> Bloqueo_ObtenerTodos(string Cod_App)
        {
            var response = new ErrorDto<List<Bloqueo>> { Code = 0, Result = [] };
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_W_Bloqueo_Obtener]";

                    var values = new
                    {
                        Cod_App = Cod_App,
                    };
                    response.Result = connection.Query<Bloqueo>(procedure, values, commandType: CommandType.StoredProcedure).Take(100).ToList();
                    response.Description = "Ok";

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        public ErrorDto Bloqueo_Insertar(Bloqueo request)
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_W_Bloqueo_Insertar]";
                    var values = new
                    {
                        //Cod_Linea = request.Cod_Linea,
                        Cod_App = request.Cod_App,
                        Fecha_Bloqueo = request.Fecha_Bloqueo,
                        Version_Bloqueada = request.Version_Bloqueada,
                        Registro_Usuario = request.Registro_Usuario,

                    };

                    resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
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

        public ErrorDto Bloqueo_Eliminar(Bloqueo request)
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_W_Bloqueo_Eliminar]";
                    var values = new
                    {
                        Cod_Linea = request.Cod_Linea,
                        Cod_App = request.Cod_App,
                    };

                    resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
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

        #region MÉTODOS APP_UPDATE

        public ErrorDto<List<Actualizacion>> Actualizacion_ObtenerTodos(string Cod_App)
        {
            var response = new ErrorDto<List<Actualizacion>> { Code = 0, Result = [] };
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_W_Actualizacion_Obtener]";

                    var values = new
                    {
                        Cod_App = Cod_App,

                    };

                    response.Result = connection.Query<Actualizacion>(procedure, values, commandType: CommandType.StoredProcedure).Take(100).ToList();
                    response.Description = "Ok";
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        public ErrorDto Actualizacion_Insertar(Actualizacion request)
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_W_Actualizacion_Insertar]";
                    var values = new
                    {
                        Cod_App = request.Cod_App,
                        Version = request.Version,
                        Notas_Descarga = request.Notas_Descarga,
                        Fecha_Libera = request.Fecha_Libera,
                        Registro_Usuario = request.Registro_Usuario,

                    };

                    resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
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

        public ErrorDto Actualizacion_Eliminar(Actualizacion request)
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_W_Actualizacion_Eliminar]";
                    var values = new
                    {
                        Cod_App = request.Cod_App,
                        Version = request.Version,
                    };

                    resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
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
