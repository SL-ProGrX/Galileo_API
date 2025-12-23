using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmUsModulosDb
    {
        private readonly IConfiguration _config;
        private const string connectionStringName = "DefaultConnString";

        public FrmUsModulosDb(IConfiguration config)
        {
            _config = config;
        }

        // Helper para crear conexión
        private SqlConnection CreateConnection()
        {
            return new SqlConnection(_config.GetConnectionString(connectionStringName));
        }

        // Helper genérico para ejecutar SP que devuelven un int (código de error)
        private ErrorDto EjecutarSpModulo(string storedProcedure, object parameters)
        {
            var resp = new ErrorDto { Code = 0 };

            try
            {
                using var connection = CreateConnection();
                resp.Code = connection
                    .Query<int>(storedProcedure, parameters, commandType: CommandType.StoredProcedure)
                    .FirstOrDefault();

                resp.Description = "Ok";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public List<ModuloDto> Modulo_ObtenerTodos()
        {
            try
            {
                using var connection = CreateConnection();
                const string procedure = "[spPGX_W_Opciones_Modulos_Obtener]";

                return connection
                    .Query<ModuloDto>(procedure, commandType: CommandType.StoredProcedure)
                    .ToList();
            }
            catch (Exception)
            {
                // Manejo mínimo, puedes loguear el error si quieres
                return new List<ModuloDto>();
            }
        }

        private object BuildModuloParams(ModuloDto request) => new
        {
            Modulo = request.Modulo,
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            Activo = request.Activo
        };

        private ErrorDto Modulo_Insertar(ModuloDto request)
        {
            const string procedure = "[spPGX_W_Modulo_Insertar]";
            return EjecutarSpModulo(procedure, BuildModuloParams(request));
        }

        private ErrorDto Modulo_Actualizar(ModuloDto request)
        {
            const string procedure = "[spPGX_W_Modulo_Editar]";
            return EjecutarSpModulo(procedure, BuildModuloParams(request));
        }

        public ErrorDto Modulo_Eliminar(int moduloId)
        {
            const string procedure = "[spPGX_W_Modulo_Eliminar]";
            var parameters = new { Modulo = moduloId };

            return EjecutarSpModulo(procedure, parameters);
        }

        public ErrorDto Modulo_Guardar(ModuloDto request)
        {
            var resp = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                using var connection = CreateConnection();

                const string query = "SELECT COUNT(*) FROM US_MODULOS WHERE Modulo = @Modulo";
                var exists = connection
                    .Query<int>(query, new { Modulo = request.Modulo })
                    .FirstOrDefault() > 0;

                resp = exists
                    ? Modulo_Actualizar(request)
                    : Modulo_Insertar(request);
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