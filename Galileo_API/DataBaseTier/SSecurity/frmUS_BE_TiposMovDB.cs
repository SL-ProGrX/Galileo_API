using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmUS_BE_TiposMovDB
    {
        private readonly IConfiguration _config;

        public frmUS_BE_TiposMovDB(IConfiguration config)
        {
            _config = config;
        }

        public List<MovimientoBE> MovimientoBE_ObtenerTodos(int modulo)
        {
            List<MovimientoBE> types = new List<MovimientoBE>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var procedure = "[spPGX_W_MovimientoBE_Obtener]";

                    var values = new
                    {
                        Modulo = modulo,
                    };

                    types = connection.Query<MovimientoBE>(procedure, values, commandType: CommandType.StoredProcedure).ToList();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return types;
        }

        private ErrorDTO MovimientoBE_Insertar(MovimientoBE request)
        {
            ErrorDTO resp = new ErrorDTO();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var Query = $"SELECT TOP 1 MOVIMIENTO + 1  FROM US_MOVIMIENTOS_BE WHERE MODULO = {request.Modulo} ORDER BY MOVIMIENTO DESC";
                    var id = connection.Query<int>(Query).FirstOrDefault();
                    var movimiento = id.ToString().PadLeft(2, '0');
                    var procedure = "[spPGX_W_MovimientoBE_Insertar]";
                    var values = new
                    {
                        Modulo = request.Modulo,
                        Movimiento = movimiento,
                        Descripcion = request.Descripcion,
                        Registro_Usuario = request.Registro_Usuario

                    };

                    resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();



                    resp.Description = id.ToString().PadLeft(2, '0');
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDTO MovimientoBE_Eliminar(string movimiento, int modulo)
        {
            ErrorDTO resp = new ErrorDTO();
            resp.Code = 0;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var procedure = "[spPGX_W_MovimientoBE_Eliminar]";
                    var values = new
                    {
                        Modulo = modulo,
                        Movimiento = movimiento

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

        public ErrorDTO MovimientoBE_Actualizar(MovimientoBE request)
        {
            ErrorDTO resp = new ErrorDTO();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var procedure = "[spPGX_W_MovimientoBE_Editar]";
                    var values = new
                    {
                        Modulo = request.Modulo,
                        Movimiento = request.Movimiento,
                        Descripcion = request.Descripcion

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

        public ErrorDTO MovimientoBE_Guardar(MovimientoBE request)
        {
            ErrorDTO resp = new ErrorDTO();
            resp.Code = 0;
            try
            {
                if (request.Movimiento == "00")
                {
                    resp = MovimientoBE_Insertar(request);
                }
                else
                {
                    resp = MovimientoBE_Actualizar(request);
                }
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
