using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo.Models.Security;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmUsOpcionesDb
    {
        private readonly IConfiguration _config;
        private const string connectionStringName = "DefaultConnString";
        private const int moduloBitacora = 13;
        private readonly MProGrXSecurityMainDb DBBitacora;

        public FrmUsOpcionesDb(IConfiguration config)
        {
            _config = config;
            DBBitacora = new MProGrXSecurityMainDb(config);
        }

        public List<ModuloDto> Modulo_ObtenerTodos()
        {
            using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));
            return connection.Query<ModuloDto>("[spPGX_W_Opciones_Modulos_Obtener]", commandType: CommandType.StoredProcedure).ToList();
        }

        public List<FormularioDto> Formulario_ObtenerTodos(int modulo)
        {
            using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));
            return connection.Query<FormularioDto>("[spPGX_W_Opciones_Modulo_Forms_Obtener]", new { modulo }, commandType: CommandType.StoredProcedure).ToList();
        }

        public List<OpcionDto> Opcion_ObtenerTodos(int modulo, string formulario)
        {
            using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));
            return connection.Query<OpcionDto>("[spPGX_W_Opciones_Obtener]", new { modulo, formulario }, commandType: CommandType.StoredProcedure).ToList();
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

        public ErrorDto Opcion_Eliminar(string codigo, string formulario, int modulo, string usuario, int codEmpresa)
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
                    if (resp.Code == 0) RegistrarBitacora(codEmpresa, usuario, "ELIMINA", $"Opción de Sistema: {codigo}");
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
                if (resp.Code == 0) RegistrarBitacora(request, "REGISTRA");
            }
            else
            {
                resp = Opcion_Actualizar(request);
                if (resp.Code == 0) RegistrarBitacora(request, "MODIFICA");
            }

            return resp;
        }

        private void RegistrarBitacora(OpcionDto request, string movimiento)
        {
            var identificador = request.Cod_Opcion == 0 ? request.Opcion : request.Cod_Opcion?.ToString();
            RegistrarBitacora(request.Cod_Empresa.GetValueOrDefault(), request.Registro_Usuario, movimiento, $"Opción de Sistema: {identificador}");
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string movimiento, string detalle)
        {
            if (codEmpresa <= 0 || string.IsNullOrWhiteSpace(usuario)) return;

            _ = DBBitacora.Bitacora(new MProGrXSecurityMainBitacora
            {
                CodEmpresa = codEmpresa,
                usuario = usuario,
                vModulo = moduloBitacora,
                strTipoMovimiento = $"{movimiento} - WEB",
                strDetalleMovimiento = detalle,
            });
        }
    }
}
