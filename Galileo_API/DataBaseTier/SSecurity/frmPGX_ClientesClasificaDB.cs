using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.Security;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmPGX_ClientesClasificaDB
    {
        private readonly IConfiguration _config;

        public frmPGX_ClientesClasificaDB(IConfiguration config)
        {
            _config = config;
        }

        public List<ClienteClasifica> Cliente_Clasifica_ObtenerTodos()
        {
            List<ClienteClasifica> data = new List<ClienteClasifica>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var procedure = "[spPGX_W_Clientes_Clasifica_Obtener]";

                    data = connection.Query<ClienteClasifica>(procedure, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return data;
        }

        private ErrorDto Cliente_Clasifica_Insertar(ClienteClasifica request)
        {
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var procedure = "[spPGX_W_Clientes_Clasifica_Insertar]";
                    var values = new
                    {
                        Cod_Clasificacion = request.Cod_Clasificacion,
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

        public ErrorDto Cliente_Clasifica_Eliminar(string request)
        {
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var procedure = "[spPGX_W_Clientes_Clasifica_Eliminar]";
                    var values = new
                    {
                        Cod_Clasificacion = request,

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

        private ErrorDto Cliente_Clasifica_Actualizar(ClienteClasifica request)
        {
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var procedure = "[spPGX_W_Clientes_Clasifica_Editar]";
                    var values = new
                    {
                        Cod_Clasificacion = request.Cod_Clasificacion,
                        Descripcion = request.Descripcion,
                        Activa = request.Activa,
                        //   Registro_Usuario = request.Registro_Usuario,

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

        public List<ClienteSelecciona> Cliente_Selecciona_ObtenerTodos(string usuario)
        {
            List<ClienteSelecciona> data = new List<ClienteSelecciona>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var procedure = "[spSEG_Admin_Client_Access_List]";
                    var values = new
                    {
                        Usuario = usuario,
                        Filtro = "",
                        Top = 30,
                    };

                    data = connection.Query<ClienteSelecciona>(procedure, values, commandType: CommandType.StoredProcedure).ToList();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return data;
        }

        public ErrorDto Cliente_Clasifica_Guardar(ClienteClasifica request)
        {
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;

            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    //valido si existe codigo
                    var query = "SELECT COUNT(*) FROM PGX_CLIENTES_CLASIFICACION WHERE Cod_Clasificacion = @Cod_Clasificacion";
                    var count = connection.Query<int>(query, new { Cod_Clasificacion = request.Cod_Clasificacion }).FirstOrDefault();
                    if (count > 0)
                    {
                        resp = Cliente_Clasifica_Actualizar(request);
                    }
                    else
                    {
                        resp = Cliente_Clasifica_Insertar(request);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }

            return resp;
        }
    }
}
