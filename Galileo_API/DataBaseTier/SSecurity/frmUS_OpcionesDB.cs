using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmUsOpcionesDb
    {
        private readonly IConfiguration _config;
        private const string connectionStringName = "DefaultConnString";

        public FrmUsOpcionesDb(IConfiguration config)
        {
            _config = config;
        }

        public List<ModuloDto> Modulo_ObtenerTodos()
        {
            List<ModuloDto> data = new List<ModuloDto>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_W_Opciones_Modulos_Obtener]";

                    data = connection.Query<ModuloDto>(procedure, commandType: CommandType.StoredProcedure).ToList();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return data;
        }

        public List<FormularioDto> Formulario_ObtenerTodos(int modulo)
        {
            List<FormularioDto> data = new List<FormularioDto>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_W_Opciones_Modulo_Forms_Obtener]";

                    var values = new
                    {
                        modulo = modulo,
                    };

                    data = connection.Query<FormularioDto>(procedure, values, commandType: CommandType.StoredProcedure).ToList();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return data;
        }

        public List<OpcionDto> Opcion_ObtenerTodos(int modulo, string formulario)
        {
            List<OpcionDto> data = new List<OpcionDto>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_W_Opciones_Obtener]";

                    var values = new
                    {
                        modulo = modulo,
                        formulario = formulario,
                    };

                    data = connection.Query<OpcionDto>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return data;
        }

        private ErrorDto Opcion_Insertar(OpcionDto request)
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
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
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
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

        private ErrorDto Opcion_Actualizar(OpcionDto request)
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
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

        public ErrorDto Opcion_Guardar(OpcionDto request)
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
