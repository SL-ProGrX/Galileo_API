using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.Security;
using System.Data;

namespace PgxAPI.DataBaseTier
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

        public List<Aplicacion> Aplicacion_ObtenerTodos()
        {
            List<Aplicacion> data = new List<Aplicacion>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_W_Aplicacion_Obtener]";

                    data = connection.Query<Aplicacion>(procedure, commandType: CommandType.StoredProcedure).ToList();
                    foreach (Aplicacion dt in data)
                    {
                        dt.Estado = dt.Activa ? "ACTIVO" : "INACTIVO";

                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return data;
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

        public List<Bloqueo> Bloqueo_ObtenerTodos(string Cod_App)
        {
            List<Bloqueo> data = new List<Bloqueo>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_W_Bloqueo_Obtener]";

                    var values = new
                    {
                        Cod_App = Cod_App,
                    };
                    data = connection.Query<Bloqueo>(procedure, values, commandType: CommandType.StoredProcedure).ToList();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return data;
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

        public List<Actualizacion> Actualizacion_ObtenerTodos(string Cod_App)
        {
            List<Actualizacion> data = new List<Actualizacion>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_W_Actualizacion_Obtener]";

                    var values = new
                    {
                        Cod_App = Cod_App,

                    };

                    data = connection.Query<Actualizacion>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return data;
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