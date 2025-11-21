using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmPgxClientesTiposIDsDb
    {
        private readonly IConfiguration _config;
        private const string connectionStringName = "DefaultConnString";

        public FrmPgxClientesTiposIDsDb(IConfiguration config)
        {
            _config = config;
        }

        public List<TipoId> TipoId_ObtenerTodos()
        {
            List<TipoId> types = new List<TipoId>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_W_TiposId_Obtener]";

                    types = connection.Query<TipoId>(procedure, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return types;
        }

        private ErrorDto TipoId_Insertar(TipoId request)
        {
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {
                int activa = request.Activa == true ? 1 : 0;

                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    //Pregunto si existe
                    var query = $"SELECT COUNT(*) FROM PGX_TIPOS_ID WHERE TIPO_ID = '{request.tipo}'";
                    var existe = connection.Query<int>(query).FirstOrDefault();
                    if (existe > 0)
                    {
                        //Actualizo activa a 1
                        var queryUpdate = $"UPDATE PGX_TIPOS_ID SET ACTIVA = 1 WHERE TIPO_ID = '{request.tipo}'";
                        connection.Query<int>(queryUpdate);
                        resp.Description = "Ok";
                        return resp;
                    }

                    var procedure = "[spPGX_W_TiposId_Insertar]";
                    var values = new
                    {
                        Tipo_Id = request.tipo,
                        Descripcion = request.Descripcion,
                        Activa = activa,
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

        public ErrorDto TipoId_Eliminar(string tipo_id)
        {
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_W_TiposId_Eliminar]";
                    var values = new
                    {
                        Tipo_Id = tipo_id,
                        //ModificaUsuario = request.ModificaUsuario,

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

        private ErrorDto TipoId_Actualizar(TipoId request)
        {
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_W_TiposId_Editar]";
                    var values = new
                    {
                        Tipo_Id = request.Tipo_Id,
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

        public ErrorDto TipoId_Guardar(TipoId request)
        {
            ErrorDto resp;
            if (request.Tipo_Id == "0")
            {
                resp = TipoId_Insertar(request);
            }
            else
            {
                resp = TipoId_Actualizar(request);
            }
            return resp;
        }
    }
}