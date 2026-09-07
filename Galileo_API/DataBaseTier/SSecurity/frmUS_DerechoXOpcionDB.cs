using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier
{
    public class FrmUsDerechoXOpcionDb
    {
        private readonly IConfiguration _config;
        private const string connectionStringName = "DefaultConnString";

        public FrmUsDerechoXOpcionDb(IConfiguration config)
        {
            _config = config;
        }

        public List<ModuloResultDto> ModulosObtener()
        {
            List<ModuloResultDto> resp;
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));
                var strSQL = "SELECT [MODULO],[NOMBRE],[DESCRIPCION],[ACTIVO],[KEYENT]" +
                    " FROM [PGX_Portal].[dbo].[US_MODULOS]" +
                    " ORDER BY modulo";

                resp = connection.Query<ModuloResultDto>(strSQL).ToList();
            }
            catch (Exception)
            {
                resp = [];
            }
            return resp;
        }

        public List<FormularioResultDto> FormulariosObtener()
        {
            List<FormularioResultDto> resp;
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));
                var strSQL = "SELECT [FORMULARIO],[MODULO],[DESCRIPCION],[REGISTRO_FECHA],[REGISTRO_USUARIO]" +
                              " FROM[PGX_Portal].[dbo].[US_FORMULARIOS]" +
                              " ORDER BY modulo,Descripcion";

                resp = connection.Query<FormularioResultDto>(strSQL).ToList();
            }
            catch (Exception)
            {
                resp = [];
            }
            return resp;
        }

        public List<OpcionResultDto> OpcionesObtener()
        {
            List<OpcionResultDto> resp;
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));
                var strSQL = "SELECT [COD_OPCION],[FORMULARIO],[MODULO],[OPCION],[OPCION_DESCRIPCION],[REGISTRO_FECHA],[REGISTRO_USUARIO]" +
                             " FROM[PGX_Portal].[dbo].[US_OPCIONES]";

                resp = connection.Query<OpcionResultDto>(strSQL).ToList();
            }
            catch (Exception)
            {
                resp = new List<OpcionResultDto>();
            }
            return resp;
        }

        public List<DatosResultDto> DatosObtener(int opcion, char estado)
        {
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));

                const string sql = @"
                                SELECT 
                                    R.cod_rol,
                                    R.descripcion,
                                    ISNULL(P.Estado,'Z') AS Estado
                                FROM US_ROLES R
                                LEFT JOIN US_ROL_PERMISOS P
                                    ON R.cod_Rol = P.cod_Rol
                                    AND P.cod_Opcion = @Opcion
                                    AND P.Estado = @Estado
                                WHERE R.Activo = 1
                                ORDER BY ISNULL(P.Estado,'Z'), R.Descripcion;";

                return connection.Query<DatosResultDto>(sql, new { Opcion = opcion, Estado = estado.ToString() }).ToList();
            }
            catch
            {
                return new List<DatosResultDto>();
            }
        }

        public List<DatosUsuarioResultDto> DatosUsuariosObtener(int opcion, char estado, int codEmpresa)
        {
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));
                const string sql = @"
                    SELECT DISTINCT U.Nombre AS Usuario, P.Estado
                    FROM US_ROL_PERMISOS P
                    INNER JOIN US_OPCIONES O ON P.cod_Opcion = O.cod_Opcion
                    INNER JOIN US_ROL_MIEMBROS M ON P.cod_Rol = M.cod_Rol
                    INNER JOIN US_USUARIOS U ON U.Usuario = M.Usuario
                    WHERE O.cod_Opcion = @Opcion
                      AND P.Estado = @Estado
                      AND (@CodEmpresa = 0 OR M.cod_Empresa = @CodEmpresa)
                    ORDER BY U.Nombre";
                return connection.Query<DatosUsuarioResultDto>(sql, new { Opcion = opcion, Estado = estado.ToString(), CodEmpresa = codEmpresa }).ToList();
            }
            catch
            {
                return [];
            }
        }

        public ErrorDto RolPermisosActualizar(OpcionRolRequestDto req)
        {
            ErrorDto resp = new();
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));
                if (req.check == true)
                {
                    var strSQL = "INSERT INTO [dbo].[US_ROL_PERMISOS]([COD_OPCION],[COD_ROL],[ESTADO],[REGISTRO_FECHA],[REGISTRO_USUARIO]) VALUES(@COD_OPCION,@COD_ROL,@ESTADO,@REGISTRO_FECHA,@REGISTRO_USUARIO)";

                    resp.Code = connection.Execute(strSQL, new { COD_OPCION = req.opcion, COD_ROL = req.rol, ESTADO = req.tipo, REGISTRO_FECHA = DateTime.Now, REGISTRO_USUARIO = req.usuario });
                    resp.Description = "Inserted";
                }
                else
                {
                    var strSQL = "DELETE FROM Us_Rol_Permisos WHERE cod_Opcion = @cod_Opcion AND estado = @estado AND cod_rol = @cod_rol";

                    resp.Code = connection.Execute(strSQL, new { cod_Opcion = req.opcion, estado = req.tipo, cod_rol = req.rol });
                    resp.Description = "Deleted";
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
