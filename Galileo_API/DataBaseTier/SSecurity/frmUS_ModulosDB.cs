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
        private const int moduloBitacora = 13;
        private readonly MSecurityMainDb _securityMainDb;

        public FrmUsModulosDb(IConfiguration config)
        {
            _config = config;
            _securityMainDb = new MSecurityMainDb(config);
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

        public ErrorDto<List<ModuloDto>> Modulo_ObtenerTodos()
        {
            var response = new ErrorDto<List<ModuloDto>> { Result = new List<ModuloDto>(), Code = 0 };
            try
            {
                using var connection = CreateConnection();
                const string procedure = "[spPGX_W_Opciones_Modulos_Obtener]";

                response.Result = connection
                    .Query<ModuloDto>(procedure, commandType: CommandType.StoredProcedure)
                    .ToList();
                response.Description = "Ok";
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
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
            var response = EjecutarSpModulo(procedure, BuildModuloParams(request));
            if (response.Code == 0) RegistrarBitacora(request, "REGISTRA", $"Módulo del Sistema: {request.Modulo}");
            return response;
        }

        private ErrorDto Modulo_Actualizar(ModuloDto request)
        {
            const string procedure = "[spPGX_W_Modulo_Editar]";
            var response = EjecutarSpModulo(procedure, BuildModuloParams(request));
            if (response.Code == 0) RegistrarBitacora(request, "MODIFICA", $"Módulo del Sistema: {request.Modulo}");
            return response;
        }

        public ErrorDto Modulo_Eliminar(int moduloId, int codEmpresa, string usuario)
        {
            const string procedure = "[spPGX_W_Modulo_Eliminar]";
            var parameters = new { Modulo = moduloId };

            var response = EjecutarSpModulo(procedure, parameters);
            if (response.Code == 0)
            {
                RegistrarBitacora(new ModuloDto
                {
                    Modulo = moduloId,
                    CodEmpresa = codEmpresa,
                    Registro_Usuario = usuario
                }, "ELIMINA", $"Módulo del Sistema: {moduloId}");
            }
            return response;
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

        private void RegistrarBitacora(ModuloDto request, string movimiento, string detalle)
        {
            if (request.CodEmpresa <= 0 || string.IsNullOrWhiteSpace(request.Registro_Usuario)) return;

            _ = _securityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = request.CodEmpresa,
                Usuario = request.Registro_Usuario,
                Modulo = moduloBitacora,
                Movimiento = $"{movimiento} - WEB",
                DetalleMovimiento = detalle,
                AppNombre = "ProGrX_WEB"
            });
        }
    }
}
