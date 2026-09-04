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
        private const int moduloBitacora = 13;

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
            var response = new ErrorDto { Code = 0, Description = "Ok" };
            try
            {
                using var connection = CreateConnection();
                connection.Open();
                using var transaction = connection.BeginTransaction();
                connection.Execute("[spPGX_Formulario_Insertar]", BuildFormularioParams(request), transaction, commandType: CommandType.StoredProcedure);

                var codOpcion = connection.QuerySingle<int>("SELECT ISNULL(MAX(Cod_Opcion), 0) + 1 FROM US_OPCIONES", transaction: transaction);
                connection.Execute(@"INSERT INTO US_OPCIONES
                    (Modulo, Formulario, Cod_Opcion, Opcion, Opcion_Descripcion, Registro_Fecha, Registro_Usuario)
                    VALUES (@Modulo, @Formulario, @CodOpcion, 'MenuAccess', 'Acceso al Formulario', GETDATE(), @Usuario)",
                    new { Modulo = request.ModuloId, Formulario = request.Nombre, CodOpcion = codOpcion, Usuario = request.Usuario }, transaction);
                transaction.Commit();
                RegistrarBitacora(request, "REGISTRA", $"Formulario: {request.Nombre}");
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        public ErrorDto Formulario_Eliminar(int modulo, string formulario, int codEmpresa, string usuario)
        {
            var response = new ErrorDto { Code = 0, Description = "Ok" };
            try
            {
                using var connection = CreateConnection();
                connection.Open();
                using var transaction = connection.BeginTransaction();
                var values = new { ModuloId = modulo, Formulario = formulario };
                connection.Execute(@"DELETE FROM US_ROL_PERMISOS
                    WHERE Cod_Opcion IN (SELECT Cod_Opcion FROM US_OPCIONES WHERE Formulario = @Formulario AND Modulo = @ModuloId)", values, transaction);
                connection.Execute("DELETE FROM US_OPCIONES WHERE Formulario = @Formulario AND Modulo = @ModuloId", values, transaction);
                connection.Execute("[spPGX_Formulario_Eliminar]", values, transaction, commandType: CommandType.StoredProcedure);
                transaction.Commit();
                RegistrarBitacora(new FormularioDto { ModuloId = modulo, Nombre = formulario, CodEmpresa = codEmpresa, Usuario = usuario }, "ELIMINA", $"Formulario: {formulario}");
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        private ErrorDto Formulario_Actualizar(FormularioDto request)
        {
            const string procedure = "[spPGX_Formulario_Editar]";
            var response = EjecutarFormularioSp(procedure, BuildFormularioParams(request));
            if (response.Code == 0) RegistrarBitacora(request, "MODIFICA", $"Formulario: {request.Nombre}");
            return response;
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

        private void RegistrarBitacora(FormularioDto request, string movimiento, string detalle)
        {
            if (request.CodEmpresa <= 0 || string.IsNullOrWhiteSpace(request.Usuario)) return;

            try
            {
                using var connection = CreateConnection();
                connection.Execute("spSEG_Bitacora_Add", new
                {
                    Cliente = request.CodEmpresa,
                    Usuario = request.Usuario,
                    Modulo = moduloBitacora,
                    Movimiento = $"{movimiento} - WEB",
                    Detalle = detalle,
                    AppName = "ProGrX_WEB",
                    AppVersion = "",
                    LogEquipo = "",
                    LogIP = "",
                    LogEquipoMac = ""
                }, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
        }
    }
}
