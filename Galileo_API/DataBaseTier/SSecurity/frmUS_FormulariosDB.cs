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

        // ========== Helpers comunes ==========

        private SqlConnection CreateConnection()
        {
            var connString = _config.GetConnectionString(connectionStringName);
            if (string.IsNullOrEmpty(connString))
                throw new InvalidOperationException($"Connection string '{connectionStringName}' is not configured.");

            return new SqlConnection(connString);
        }

        private List<T> QuerySpList<T>(string storedProcedure, object? parameters = null)
        {
            try
            {
                using var connection = CreateConnection();
                return connection.Query<T>(storedProcedure, parameters, commandType: CommandType.StoredProcedure).ToList();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                return new List<T>();
            }
        }

        private ErrorDto EjecutarFormularioSp(string storedProcedure, object parameters)
        {
            var resp = new ErrorDto();

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

        private object BuildFormularioParams(FormularioDto request) => new
        {
            ModuloId = request.ModuloId,
            Formulario = request.Nombre,
            Descripcion = request.Descripcion,
            Usuario = request.Usuario
        };

        // ========== Métodos públicos/privados ==========

        public List<FormularioModel> ObtenerFormulariosPorModulo(int moduloId)
        {
            const string procedure = "[spPGX_Formularios_PorModulo_Obtener]";
            var values = new { ModuloId = moduloId };

            return QuerySpList<FormularioModel>(procedure, values);
        }

        private ErrorDto Formulario_Insertar(FormularioDto request)
        {
            const string procedure = "[spPGX_Formulario_Insertar]";
            return EjecutarFormularioSp(procedure, BuildFormularioParams(request));
        }

        public ErrorDto Formulario_Eliminar(int modulo, string formulario)
        {
            const string procedure = "[spPGX_Formulario_Eliminar]";
            var values = new
            {
                ModuloId = modulo,
                Formulario = formulario
            };

            return EjecutarFormularioSp(procedure, values);
        }

        private ErrorDto Formulario_Actualizar(FormularioDto request)
        {
            const string procedure = "[spPGX_Formulario_Editar]";
            return EjecutarFormularioSp(procedure, BuildFormularioParams(request));
        }

        public ErrorDto Formulario_Guardar(FormularioDto request)
        {
            var resp = new ErrorDto { Code = 0 };

            using (var connection = CreateConnection())
            {
                const string query = @"
                    SELECT COUNT(*) 
                    FROM [US_FORMULARIOS] 
                    WHERE Modulo = @ModuloId 
                      AND UPPER(Formulario) = @Formulario";

                var values = new
                {
                    ModuloId = request.ModuloId,
                    Formulario = request.Nombre.ToUpper()
                };

                var count = connection.Query<int>(query, values).FirstOrDefault();

                resp = count == 0
                    ? Formulario_Insertar(request)
                    : Formulario_Actualizar(request);
            }

            return resp;
        }
    }
}