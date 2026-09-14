using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
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

        private string GetConnectionString()
        {
            return _config.GetConnectionString(connectionStringName)
                ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.");
        }

        private static ErrorDto<T> Error<T>(Exception exception)
        {
            return new ErrorDto<T>
            {
                Code = -1,
                Description = exception.Message,
                Result = default
            };
        }

        private static ErrorDto Error(Exception exception)
        {
            return new ErrorDto
            {
                Code = -1,
                Description = exception.Message
            };
        }

        public ErrorDto<List<UsDerechosNewDto>> ObtenerUsDerechosNewDTOs(string Rol, string Estado)
        {
            const string sql = @"
                SELECT DISTINCT O.*, ISNULL(P.ESTADO, 'Z') AS PermisoEstado
                FROM US_OPCIONES O
                INNER JOIN US_FORMULARIOS F ON O.FORMULARIO = F.FORMULARIO
                LEFT JOIN US_ROL_PERMISOS P
                    ON O.COD_OPCION = P.COD_OPCION
                    AND P.COD_ROL = @rol
                    AND P.ESTADO = @estado
                ORDER BY O.OPCION_DESCRIPCION;";

            try
            {
                using var connection = new SqlConnection(GetConnectionString());
                return new ErrorDto<List<UsDerechosNewDto>>
                {
                    Code = 0,
                    Description = "Ok",
                    Result = connection.Query<UsDerechosNewDto>(sql, new { rol = Rol, estado = Estado }).ToList()
                };
            }
            catch (Exception ex)
            {
                return Error<List<UsDerechosNewDto>>(ex);
            }
        }

        public ErrorDto<List<UsModuloDto>> ObtenerUsModulos()
        {
            try
            {
                using var connection = new SqlConnection(GetConnectionString());
                const string sql = "SELECT * FROM US_MODULOS ORDER BY MODULO;";
                return new ErrorDto<List<UsModuloDto>>
                {
                    Code = 0,
                    Description = "Ok",
                    Result = connection.Query<UsModuloDto>(sql).ToList()
                };
            }
            catch (Exception ex)
            {
                return Error<List<UsModuloDto>>(ex);
            }
        }

        public ErrorDto<List<UsFormularioDto>> ObtenerUsFormularios()
        {
            try
            {
                using var connection = new SqlConnection(GetConnectionString());
                const string sql = "SELECT * FROM US_FORMULARIOS ORDER BY DESCRIPCION;";
                return new ErrorDto<List<UsFormularioDto>>
                {
                    Code = 0,
                    Description = "Ok",
                    Result = connection.Query<UsFormularioDto>(sql).ToList()
                };
            }
            catch (Exception ex)
            {
                return Error<List<UsFormularioDto>>(ex);
            }
        }

        public ErrorDto<List<UsRolDto>> ObtenerUsRoles()
        {

            try
            {
                using var connection = new SqlConnection(GetConnectionString());
                const string sql = @"SELECT * FROM US_ROLES ORDER BY DESCRIPCION;";
                return new ErrorDto<List<UsRolDto>>
                {
                    Code = 0,
                    Description = "Ok",
                    Result = connection.Query<UsRolDto>(sql).ToList()
                };
            }
            catch (Exception ex)
            {
                return Error<List<UsRolDto>>(ex);
            }
        }

        public ErrorDto CrearUsDerechosNewDTO(CrearUsDerechosNewDto info)
        {
            try
            {
                using var connection = new SqlConnection(GetConnectionString());

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
                    {
                        return new ErrorDto
                        {
                            Code = 2,
                            Description = "El registro ya existe en otro estado."
                        };
                    }

                    return new ErrorDto
                    {
                        Code = 0,
                        Description = "El permiso ya estaba registrado."
                    };
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

                return new ErrorDto
                {
                    Code = 0,
                    Description = "Ok"
                };
            }
            catch (Exception ex)
            {
                return Error(ex);
            }
        }

        public ErrorDto EliminarUsDerechosNewDTO(int COD_OPCION, string ESTADO, string COD_ROL)
        {
            try
            {
                using var connection = new SqlConnection(GetConnectionString());
                const string sql = @"
                    DELETE FROM US_ROL_PERMISOS
                    WHERE COD_OPCION = @COD_OPCION AND ESTADO = @ESTADO AND COD_ROL = @COD_ROL;";

                connection.Execute(sql, new { COD_OPCION, ESTADO, COD_ROL });
                return new ErrorDto { Code = 0, Description = "Ok" };
            }
            catch (Exception ex)
            {
                return Error(ex);
            }
        }

        public ErrorDto EditarUsDerechosNew(int COD_OPCION, string ESTADO, string COD_ROL, string NUEVO_ESTADO)
        {
            const string sql = @"
                UPDATE US_ROL_PERMISOS
                SET ESTADO = @NUEVO_ESTADO
                WHERE COD_OPCION = @COD_OPCION AND ESTADO = @ESTADO AND COD_ROL = @COD_ROL;";

            try
            {
                using var connection = new SqlConnection(GetConnectionString());

                connection.Execute(sql, new
                {
                    NUEVO_ESTADO,
                    COD_OPCION,
                    ESTADO,
                    COD_ROL
                });
                return new ErrorDto { Code = 0, Description = "Ok" };
            }
            catch (Exception ex)
            {
                return Error(ex);
            }
        }
    }
}
