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

        public List<UsDerechosNewDto> ObtenerUsDerechosNewDTOs(string Rol, string Estado)
        {
            string stringConn = _config.GetConnectionString(connectionStringName)
                ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.");

            const string sql = @"
                SELECT DISTINCT O.*, ISNULL(P.ESTADO, 'Z') AS PermisoEstado
                FROM US_OPCIONES O
                INNER JOIN US_FORMULARIOS F ON O.FORMULARIO = F.FORMULARIO
                LEFT JOIN US_ROL_PERMISOS P
                    ON O.COD_OPCION = P.COD_OPCION
                    AND P.COD_ROL = @rol
                    AND P.ESTADO = @estado
                ORDER BY O.COD_OPCION;";

            try
            {
                using var connection = new SqlConnection(stringConn);
                return connection.Query<UsDerechosNewDto>(sql, new { rol = Rol, estado = Estado }).ToList();
            }
            catch
            {
                return new List<UsDerechosNewDto>();
            }
        }

        public List<UsRolDto> ObtenerUsRoles()
        {
            string stringConn = _config.GetConnectionString(connectionStringName)
                ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.");

            const string sql = @"SELECT * FROM US_ROLES;";

            try
            {
                using var connection = new SqlConnection(stringConn);
                return connection.Query<UsRolDto>(sql).ToList();
            }
            catch
            {
                return new List<UsRolDto>();
            }
        }

        public int CrearUsDerechosNewDTO(CrearUsDerechosNewDto info)
        {
            string stringConn = _config.GetConnectionString(connectionStringName)
                ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.");

            try
            {
                using var connection = new SqlConnection(stringConn);

                // ✅ Parametrizado (sin injection)
                const string sqlValidar = @"
                    SELECT ESTADO
                    FROM US_ROL_PERMISOS
                    WHERE COD_OPCION = @COD_OPCION AND COD_ROL = @COD_ROL;";

                var existe = connection.QueryFirstOrDefault<string>(sqlValidar, new
                {
                    COD_OPCION = info.COD_OPCION,
                    COD_ROL = info.COD_ROL
                });

                if (existe != null)
                {
                    if (existe != info.ESTADO)
                        return 2;

                    // ya está parametrizado dentro del método
                    return EliminarUsDerechosNewDTO(info.COD_OPCION ?? 0, info.ESTADO, info.COD_ROL);
                }

                // ✅ Parametrizado (sin injection)
                const string sqlInsert = @"
                    INSERT INTO US_ROL_PERMISOS
                        (COD_OPCION, COD_ROL, ESTADO, REGISTRO_FECHA, REGISTRO_USUARIO)
                    VALUES
                        (@COD_OPCION, @COD_ROL, @ESTADO, @REGISTRO_FECHA, @REGISTRO_USUARIO);";

                connection.Execute(sqlInsert, new
                {
                    COD_OPCION = info.COD_OPCION,
                    COD_ROL = info.COD_ROL,
                    ESTADO = info.ESTADO,
                    REGISTRO_FECHA = info.REGISTRO_FECHA,
                    REGISTRO_USUARIO = info.REGISTRO_USUARIO
                });

                return 0;
            }
            catch
            {
                return 1;
            }
        }

        public int EliminarUsDerechosNewDTO(int COD_OPCION, string ESTADO, string COD_ROL)
        {
            string stringConn = _config.GetConnectionString(connectionStringName)
                ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.");

            try
            {
                using var connection = new SqlConnection(stringConn);
                const string sql = @"
                    DELETE FROM US_ROL_PERMISOS
                    WHERE COD_OPCION = @COD_OPCION AND ESTADO = @ESTADO AND COD_ROL = @COD_ROL;";

                return connection.Execute(sql, new { COD_OPCION, ESTADO, COD_ROL });
            }
            catch
            {
                return 1;
            }
        }

        public int EditarUsDerechosNew(int COD_OPCION, string ESTADO, string COD_ROL, string NUEVO_ESTADO)
        {
            string stringConn = _config.GetConnectionString(connectionStringName)
                ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.");

            const string sql = @"
                UPDATE US_ROL_PERMISOS
                SET ESTADO = @NUEVO_ESTADO
                WHERE COD_OPCION = @COD_OPCION AND ESTADO = @ESTADO AND COD_ROL = @COD_ROL;";

            try
            {
                using var connection = new SqlConnection(stringConn);

                // ✅ Execute para UPDATE (filas afectadas)
                return connection.Execute(sql, new
                {
                    NUEVO_ESTADO,
                    COD_OPCION,
                    ESTADO,
                    COD_ROL
                });
            }
            catch
            {
                return 1;
            }
        }
    }
}