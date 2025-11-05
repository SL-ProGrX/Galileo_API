using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmUS_OpcionesDB
    {
        private readonly IConfiguration _config;

        public frmUS_OpcionesDB(IConfiguration config)
        {
            _config = config;
        }

        public List<ModuloDTO> Modulo_ObtenerTodos()
        {
            List<ModuloDTO> data = new List<ModuloDTO>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var procedure = "[spPGX_W_Opciones_Modulos_Obtener]";

                    data = connection.Query<ModuloDTO>(procedure, commandType: CommandType.StoredProcedure).ToList();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return data;
        }

        public List<FormularioDTO> Formulario_ObtenerTodos(int modulo)
        {
            List<FormularioDTO> data = new List<FormularioDTO>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var procedure = "[spPGX_W_Opciones_Modulo_Forms_Obtener]";

                    var values = new
                    {
                        modulo = modulo,
                    };

                    data = connection.Query<FormularioDTO>(procedure, values, commandType: CommandType.StoredProcedure).ToList();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return data;
        }

        public List<OpcionDTO> Opcion_ObtenerTodos(int modulo, string formulario)
        {
            List<OpcionDTO> data = new List<OpcionDTO>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var procedure = "[spPGX_W_Opciones_Obtener]";

                    var values = new
                    {
                        modulo = modulo,
                        formulario = formulario,
                    };

                    data = connection.Query<OpcionDTO>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return data;
        }

        private ErrorDto Opcion_Insertar(OpcionDTO request)
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var procedure = "[spPGX_W_Opciones_Insertar]";
                    var values = new
                    {
                        //Cod_Opcion = request.Cod_Opcion,
                        Opcion = request.Opcion,
                        Opcion_Descripcion = request.Opcion_Descripcion,
                        Modulo = request.Modulo,
                        Formulario = request.Formulario,
                        Registro_Usuario = request.Registro_Usuario,
                        // Registro_Fecha = request.Registro_Fecha,

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

        public ErrorDto Opcion_Eliminar(string codigo, string formulario, int modulo)
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var procedure = "[spPGX_W_Opciones_Eliminar]";
                    var values = new
                    {
                        Cod_Opcion = codigo,
                        Formulario = formulario,
                        Modulo = modulo

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

        private ErrorDto Opcion_Actualizar(OpcionDTO request)
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var procedure = "[spPGX_W_Opciones_Editar]";
                    var values = new
                    {
                        Cod_Opcion = request.Cod_Opcion,
                        Opcion = request.Opcion,
                        Opcion_Descripcion = request.Opcion_Descripcion,
                        Modulo = request.Modulo,
                        Formulario = request.Formulario,
                        //Registro_Usuario = request.Registro_Usuario,
                        //Registro_Fecha = request.Registro_Fecha,

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

        public ErrorDto Opcion_Guardar(OpcionDTO request)
        {
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;

            if (request.Cod_Opcion == 0)
            {
                resp = Opcion_Insertar(request);
            }
            else
            {
                resp = Opcion_Actualizar(request);
            }

            return resp;
        }
    }
}
