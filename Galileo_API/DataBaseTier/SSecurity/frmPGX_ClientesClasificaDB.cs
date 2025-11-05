using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
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

        public List<Cliente_Clasifica> Cliente_Clasifica_ObtenerTodos()
        {
            List<Cliente_Clasifica> data = new List<Cliente_Clasifica>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var procedure = "[spPGX_W_Clientes_Clasifica_Obtener]";

                    data = connection.Query<Cliente_Clasifica>(procedure, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return data;
        }

        private ErrorDTO Cliente_Clasifica_Insertar(Cliente_Clasifica request)
        {
            ErrorDTO resp = new ErrorDTO();
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

        public ErrorDTO Cliente_Clasifica_Eliminar(string request)
        {
            ErrorDTO resp = new ErrorDTO();
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

        private ErrorDTO Cliente_Clasifica_Actualizar(Cliente_Clasifica request)
        {
            ErrorDTO resp = new ErrorDTO();
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

        public List<Cliente_Selecciona> Cliente_Selecciona_ObtenerTodos(string usuario)
        {
            List<Cliente_Selecciona> data = new List<Cliente_Selecciona>();
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

                    data = connection.Query<Cliente_Selecciona>(procedure, values, commandType: CommandType.StoredProcedure).ToList();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return data;
        }

        public ErrorDTO Cliente_Clasifica_Guardar(Cliente_Clasifica request)
        {
            ErrorDTO resp = new ErrorDTO();
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
