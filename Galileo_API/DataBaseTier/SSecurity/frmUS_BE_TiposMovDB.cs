using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmUsBeTiposMovDb
    {
        private readonly IConfiguration _config;
        private const string connectionStringName = "DefaultConnString";
        private const int moduloBitacora = 13;

        public FrmUsBeTiposMovDb(IConfiguration config)
        {
            _config = config;
        }

        public List<MovimientoBE> MovimientoBE_ObtenerTodos(int modulo)
        {
            List<MovimientoBE> types = new();
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));
                var procedure = "[spPGX_W_MovimientoBE_Obtener]";

                var values = new { Modulo = modulo };

                types = connection.Query<MovimientoBE>(
                    procedure, values, commandType: CommandType.StoredProcedure
                ).ToList();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return types;
        }

        private ErrorDto MovimientoBE_Insertar(MovimientoBE request)
        {
            ErrorDto resp = new();
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));

                var procedure = "[spPGX_W_MovimientoBE_Insertar]";
                var values = new
                {
                    Modulo = request.Modulo,
                    Movimiento = request.Movimiento,
                    Descripcion = request.Descripcion,
                    Registro_Usuario = request.Registro_Usuario
                };

                resp.Code = connection.Query<int>(
                    procedure, values, commandType: CommandType.StoredProcedure
                ).FirstOrDefault();

                resp.Description = "Ok";
                if (resp.Code == 0) RegistrarBitacora(request, "REGISTRA", $"Bitácora Especial - Tipo Movimiento: {request.Movimiento}..Modulo: {request.Modulo}");
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto MovimientoBE_Eliminar(string movimiento, int modulo, int codEmpresa, string usuario)
        {
            ErrorDto resp = new() { Code = 0 };
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));
                var procedure = "[spPGX_W_MovimientoBE_Eliminar]";
                var values = new
                {
                    Modulo = modulo,
                    Movimiento = movimiento
                };

                resp.Code = connection.Query<int>(
                    procedure, values, commandType: CommandType.StoredProcedure
                ).FirstOrDefault();

                resp.Description = "Ok";
                if (resp.Code == 0) RegistrarBitacora(new MovimientoBE { Movimiento = movimiento, Modulo = modulo, CodEmpresa = codEmpresa, Registro_Usuario = usuario }, "ELIMINA", $"Bitácora Especial - Tipo Movimiento: {movimiento}..Modulo: {modulo}");
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto MovimientoBE_Actualizar(MovimientoBE request)
        {
            ErrorDto resp = new();
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));
                var procedure = "[spPGX_W_MovimientoBE_Editar]";
                var values = new
                {
                    Modulo = request.Modulo,
                    Movimiento = request.Movimiento,
                    Descripcion = request.Descripcion
                };

                resp.Code = connection.Query<int>(
                    procedure, values, commandType: CommandType.StoredProcedure
                ).FirstOrDefault();

                resp.Description = "Ok";
                if (resp.Code == 0) RegistrarBitacora(request, "MODIFICA", $"Bitácora Especial - Tipo Movimiento: {request.Movimiento}..Modulo: {request.Modulo}");
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto MovimientoBE_Guardar(MovimientoBE request)
        {
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));
                var existe = connection.QuerySingle<int>("SELECT COUNT(*) FROM US_MOVIMIENTOS_BE WHERE MODULO = @Modulo AND MOVIMIENTO = @Movimiento", new { request.Modulo, request.Movimiento }) > 0;
                return existe ? MovimientoBE_Actualizar(request) : MovimientoBE_Insertar(request);
            }
            catch (Exception ex)
            {
                return new ErrorDto { Code = -1, Description = ex.Message };
            }
        }

        private void RegistrarBitacora(MovimientoBE request, string movimiento, string detalle)
        {
            if (request.CodEmpresa <= 0 || string.IsNullOrWhiteSpace(request.Registro_Usuario)) return;
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));
                connection.Execute("spSEG_Bitacora_Add", new { Cliente = request.CodEmpresa, Usuario = request.Registro_Usuario, Modulo = moduloBitacora, Movimiento = $"{movimiento} - WEB", Detalle = detalle, AppName = "ProGrX_WEB", AppVersion = "", LogEquipo = "", LogIP = "", LogEquipoMac = "" }, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex) { _ = ex.Message; }
        }
    }
}
