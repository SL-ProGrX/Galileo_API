using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmUsFormulariosDb
    {
        private readonly IConfiguration _config;
        private const string connectionStringName = "DefaultConnString";

        public FrmUsFormulariosDb(IConfiguration config)
        {
            _config = config;
        }

        public List<FormularioModel> ObtenerFormulariosPorModulo(int moduloId)
        {
            List<FormularioModel> result = new List<FormularioModel>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_Formularios_PorModulo_Obtener]";
                    var values = new
                    {
                        ModuloId = moduloId,
                    };
                    result = connection.Query<FormularioModel>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return result!;
        }

        private ErrorDto Formulario_Insertar(FormularioDto request)
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_Formulario_Insertar]";
                    var values = new
                    {
                        ModuloId = request.ModuloId,
                        Formulario = request.Nombre,
                        Descripcion = request.Descripcion,
                        Usuario = request.Usuario
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

        public ErrorDto Formulario_Eliminar(int modulo, string formulario)
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_Formulario_Eliminar]";
                    var values = new
                    {
                        ModuloId = modulo,
                        Formulario = formulario
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

        private ErrorDto Formulario_Actualizar(FormularioDto request)
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_Formulario_Editar]";
                    var values = new
                    {
                        ModuloId = request.ModuloId,
                        Formulario = request.Nombre,
                        Descripcion = request.Descripcion,
                        Usuario = request.Usuario
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

        public ErrorDto Formulario_Guardar(FormularioDto request)
        {
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;

            using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
            {
                //Valido si el formulario ya existe
                var query = "SELECT COUNT(*) FROM [US_FORMULARIOS] WHERE Modulo = @ModuloId AND UPPER(Formulario) = @Formulario";
                var values = new
                {
                    ModuloId = request.ModuloId,
                    Formulario = request.Nombre.ToUpper()
                };
                var count = connection.Query<int>(query, values).FirstOrDefault();
                if (count == 0)
                {
                    resp = Formulario_Insertar(request);
                }
                else
                {
                    resp = Formulario_Actualizar(request);
                }
            }
            return resp;
        }
    }
}
