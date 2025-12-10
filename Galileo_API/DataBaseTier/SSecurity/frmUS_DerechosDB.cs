using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier
{
    public class FrmUsDerechosDb
    {
        private readonly IConfiguration _config;
        private const string connectionStringName = "DefaultConnString";

        public FrmUsDerechosDb(IConfiguration config)
        {
            _config = config;
        }

        public List<UsDerechosNewDto> ObtenerUsDerechosNewDTOs(string Rol, string Estado) //opciones
        {
            string? stringConn = _config.GetConnectionString(connectionStringName);
            if (stringConn is null)
                throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.");

            List<UsDerechosNewDto> Result = [];

            string sql = "SELECT DISTINCT O.*, ISNULL(P.ESTADO, 'Z') AS 'PermisoEstado' " +
                          "FROM US_OPCIONES O " +
                          "INNER JOIN US_FORMULARIOS F ON O.FORMULARIO = F.FORMULARIO " +
                          "LEFT JOIN US_ROL_PERMISOS P ON O.COD_OPCION = P.COD_OPCION AND P.COD_ROL = @rol  AND P.ESTADO = @estado " +
                          "ORDER BY O.COD_OPCION";
            var values = new
            {
                rol = Rol,
                estado = Estado,
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                Result = connection.Query<UsDerechosNewDto>(sql, values).ToList();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return Result;

        }//end ObtenerUSDerechosNewDTOs

        public List<UsRolDto> ObtenerUsRoles()
        {
            string? stringConn = _config.GetConnectionString(connectionStringName);
            if (stringConn is null)
                throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.");

            List<UsRolDto> Result = [];
            string sql = "SELECT * FROM US_ROLES";
            var values = new
            {
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                Result = connection.Query<UsRolDto>(sql, values).ToList();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return Result;

        }//end ObtenerUsRoles

        public int CrearUsDerechosNewDTO(CrearUsDerechosNewDto info)
        {
            string? stringConn = _config.GetConnectionString(connectionStringName);
            if (stringConn is null)
                throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.");
            int Result = 0;
            try
            {
                using var connection = new SqlConnection(stringConn);
                { 
                    string sqlValidar = $@"SELECT ESTADO FROM US_ROL_PERMISOS WHERE COD_OPCION = '{info.COD_OPCION}' AND COD_ROL = '{info.COD_ROL}'";
                    var existe = connection.Query<string>(sqlValidar).FirstOrDefault();

                    if (existe != null)
                    {
                        if (existe != info.ESTADO)
                        {
                            return 2;
                        }

                        EliminarUsDerechosNewDTO(info.COD_OPCION ?? 0, info.ESTADO, info.COD_ROL);
                    }
                    else
                    {
                        string sql = $@"INSERT US_ROL_PERMISOS(COD_OPCION, COD_ROL, ESTADO, REGISTRO_FECHA, REGISTRO_USUARIO) 
                                        VALUES('{info.COD_OPCION}', '{info.COD_ROL}', '{info.ESTADO}', '{info.REGISTRO_FECHA}', '{info.REGISTRO_USUARIO}')";
                        connection.Execute(sql);
                    }

                }
            }
            catch (Exception)
            {
                return 1;
            }

            return Result;
        }//end CrearUsDerechosNewDTO

        public int EliminarUsDerechosNewDTO(int COD_OPCION, string ESTADO, string COD_ROL)
        {
            string? stringConn = _config.GetConnectionString(connectionStringName);
            if (stringConn is null)
                throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.");

            int Result = 0;
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    string sql = @"DELETE US_ROL_PERMISOS WHERE COD_OPCION = @COD_OPCION AND ESTADO = @ESTADO AND COD_ROL = @COD_ROL";
                    Result = connection.Execute(sql, new { COD_OPCION, ESTADO, COD_ROL });
                }
            }
            catch (Exception ex)
            {
                Result = 1;
                _ = ex.Message;
            }
            return Result;

        }//end EliminarUsDerechosNewDTO

        public int EditarUsDerechosNew(int COD_OPCION, string ESTADO, string COD_ROL, string NUEVO_ESTADO)
        {

            string? stringConn = _config.GetConnectionString(connectionStringName);
            if (stringConn is null)
                throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.");

            int Result = 0;
            string sql = "UPDATE US_ROL_PERMISOS SET COD_OPCION = COD_OPCION, COD_ROL = COD_ROL, ESTADO = @NUEVO_ESTADO, REGISTRO_FECHA = REGISTRO_FECHA, REGISTRO_USUARIO = REGISTRO_USUARIO" +
                         " WHERE  COD_OPCION = @COD_OPCION AND ESTADO = @ESTADO AND COD_ROL = @COD_ROL";
            var values = new
            {

                NUEVO_ESTADO = NUEVO_ESTADO,
                COD_OPCION = COD_OPCION,
                ESTADO = ESTADO,
                COD_ROL = COD_ROL,

            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                Result = connection.Query<int>(sql, values).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Result = 1;
                _ = ex.Message;
            }
            return Result;

        }//end EditarUsDerechosNew

    }
}