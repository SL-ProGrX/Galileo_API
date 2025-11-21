using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class MSecurityMainDb
    {
        private readonly IConfiguration _config;
        private const string connectionStringName = "DefaultConnString";

        public MSecurityMainDb(IConfiguration config)
        {
            _config = config;
        }

        public int Derecho(ParametrosAccesoDto req)
        {
            int resp = 0;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {

                    var procedure = "[spSEG_Access]";

                    var values = new
                    {
                        Cliente = req.EmpresaId,
                        Usuario = req.Usuario,
                        Modulo = req.Modulo,
                        FormX = req.FormName,
                        Opcion = req.Boton
                    };

                    resp = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public ErrorDto Bitacora(BitacoraInsertarDto req)
        {
            ErrorDto resp = new ErrorDto();
            try
            {

                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    connection.Open();

                    var strSQL = $@"INSERT INTO US_Bitacora (Cod_Empresa, Usuario, Fecha_Hora, Modulo, Movimiento, Detalle, APP_NOMBRE)
                                 VALUES ({req.EmpresaId}, '{req.Usuario}', '{DateTime.Now}' ,{req.Modulo},'{req.Movimiento.ToUpper()}',
                                 '{req.DetalleMovimiento.Substring(0, Math.Min(500, req.DetalleMovimiento.Length))}', 
                                 'ProGrX_WEB')";

                    resp.Code = connection.Query<int>(strSQL).FirstOrDefault();
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

        public ErrorDto SbSEGCuentaLog(SegLogInsertarDto req)
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "spSEG_Log";

                    var values = new
                    {
                        AppName = req.AppName,
                        AppVersion = req.AppVersion,
                        Usuario = req.Usuario,
                        PTransac = req.PTransac,
                        PNotas = req.PNotas.Substring(0, Math.Min(500, req.PNotas.Length)),
                        PUserMov = req.PUserMov,
                        AppMaquina = req.AppMaquina
                    };

                    resp.Code = connection.Execute(procedure, values, commandType: CommandType.StoredProcedure);
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

        public int DerechoMDI(DerechoMdiObtenerDto req)
        {
            int resp = 0;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "spSEG_Access";

                    var values = new
                    {
                        Cliente = req.Cliente,
                        Usuario = req.Usuario,
                        Modulo = req.Modulo,
                        FormX = req.FormX,
                        Opcion = req.Opcion
                    };

                     resp = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                }
            }
            catch (Exception)
            {
                resp = -1;
            }
            return resp;
        }

    }
}
