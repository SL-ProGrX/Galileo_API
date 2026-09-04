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
        private const int moduloBitacora = 13;

        public FrmPgxClientesTiposIDsDb(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<List<TipoId>> TipoId_ObtenerTodos()
        {
            var response = new ErrorDto<List<TipoId>> { Result = new List<TipoId>(), Code = 0 };
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_W_TiposId_Obtener]";

                    response.Result = connection.Query<TipoId>(procedure, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            response.Description = response.Code == 0 ? "Ok" : response.Description;
            return response;
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
                    var query = "SELECT COUNT(*) FROM PGX_TIPOS_ID WHERE TIPO_ID = @TipoId";
                    var existe = connection.Query<int>(query, new { TipoId = request.tipo }).FirstOrDefault();
                    if (existe > 0)
                    {
                        //Actualizo activa a 1
                        var queryUpdate = "UPDATE PGX_TIPOS_ID SET ACTIVA = 1 WHERE TIPO_ID = @TipoId";
                        connection.Query<int>(queryUpdate, new { TipoId = request.tipo });
                        resp.Description = "Ok";
                        RegistrarBitacora(request, "MODIFICA", $"Tipo de ID: {request.tipo}");
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
                    if (resp.Code == 0) RegistrarBitacora(request, "REGISTRA", $"Tipo de ID: {request.tipo}");
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto TipoId_Eliminar(string tipo_id, int codEmpresa, string usuario)
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
                    if (resp.Code == 0) RegistrarBitacora(new TipoId { tipo = tipo_id, CodEmpresa = codEmpresa, Registro_Usuario = usuario }, "ELIMINA", $"Tipo de ID: {tipo_id}");
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
                    if (resp.Code == 0) RegistrarBitacora(request, "MODIFICA", $"Tipo de ID: {request.Tipo_Id}");
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

        private void RegistrarBitacora(TipoId request, string movimiento, string detalle)
        {
            if (request.CodEmpresa <= 0 || string.IsNullOrWhiteSpace(request.Registro_Usuario)) return;

            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));
                connection.Execute("spSEG_Bitacora_Add", new
                {
                    Cliente = request.CodEmpresa,
                    Usuario = request.Registro_Usuario,
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
