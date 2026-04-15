
using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class SeguridadPortalDb
    {
        private readonly IConfiguration _config;
        private const string connectionStringName = "DefaultConnString";

        public SeguridadPortalDb(IConfiguration config)
        {
            _config = config;
        }

        public bool Sys_Portal_Admin_Valid(string Usuario)
        {
            int resp = 0;
            try
            {
                Usuario = NormalizeUsuario(Usuario);

                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    const string query = "SELECT dbo.fxSEG_Admin_Portal_Autenticate(@Usuario, @Token)";
                    var values = new
                    {
                        Usuario = Usuario,
                        Token = "#MyMasterK3y#"
                    };
                    resp = connection.Query<int>(query, values).FirstOrDefault();
                }
                return resp == 1;
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                return resp == 0;
            }
            
        }

        public int UsuarioObtenerKeyAdmin(string Usuario)
        {
            try
            {
                Usuario = NormalizeUsuario(Usuario);

                const string sql = "select isnull(key_admin,0) as Admin from us_usuarios where usuario = @Usuario";
                var values = new
                {
                    Usuario
                };

                using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));
                return connection.Query<int>(sql, values).FirstOrDefault();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                return -1;
            }
        }

        public UsAdminClientesDto US_ADMIN_CLIENTES_Obtener(string Usuario, int EmpresaId)
        {
            
            try
            {
                Usuario = NormalizeUsuario(Usuario);

                UsAdminClientesDto Result = new UsAdminClientesDto();
                const string sql = "spSEG_Admin_Clients_Roles_Load";
                var values = new
                {
                    Usuario = Usuario,
                    EmpresaId = EmpresaId
                };

                using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));
                var queryResult = connection.Query<UsAdminClientesDto>(sql, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                if (queryResult != null)
                {
                    Result = queryResult;
                }
                return Result;
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                return new UsAdminClientesDto();
            }
            
        }

        public UsuarioBloqueoDto UsuarioBloqueoObtener(string Usuario)
        {
            try
            {
                Usuario = NormalizeUsuario(Usuario);

                UsuarioBloqueoDto Result = new UsuarioBloqueoDto();
                const string sql = "spSEG_Bloqueo";
                var values = new
                {
                    Usuario = Usuario,
                };

                using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));
                var queryResult = connection.Query<UsuarioBloqueoDto>(sql, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                if (queryResult != null)
                {
                    Result = queryResult;
                    Result.Usuario = Usuario;
                }
                else
                {
                    Result.Usuario = Usuario;
                }
                return Result;
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                return new UsuarioBloqueoDto();
            }
            

        }

        public UsuarioCondicionDto UsuarioCondicionObtener(string Usuario)
        {
            try
            {
                Usuario = NormalizeUsuario(Usuario);

                UsuarioCondicionDto Result = new UsuarioCondicionDto();
                const string sql = "spSEG_USRCondicion";
                var values = new
                {
                    Usuario = Usuario,
                };

                using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));
                var queryResult = connection.Query<UsuarioCondicionDto>(sql, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                if (queryResult != null)
                {
                    Result = queryResult;
                    Result.Usuario = Usuario;
                }
                else
                {
                    Result.Usuario = Usuario;
                }
                return Result;
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                return new UsuarioCondicionDto();
            }
            

        }

        public UsuarioVencimientoDto UsuarioVencimientoObtener(string Usuario)
        {

            
            try
            {
                Usuario = NormalizeUsuario(Usuario);

                UsuarioVencimientoDto Result = new UsuarioVencimientoDto();
                const string sql = "spSEG_Vencimiento";
                var values = new
                {
                    Usuario = Usuario,
                };

                using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));
                var queryResult = connection.Query<UsuarioVencimientoDto>(sql, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                if (queryResult != null)
                {
                    Result = queryResult;
                    Result.Usuario = Usuario;
                }
                else
                {
                    Result.Usuario = Usuario;
                }
                return Result;
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                return new UsuarioVencimientoDto();
            }
            

        }

        public int AppStatusObtener(string AppName, string AppVersion) //PREGUNTAR POR ESTE => (SP BLOQUEADO Y CUAL TABLA DEL DB?)
        {

            int Result = 0;
            const string sql = "spSEG_App_Status";
            var values = new
            {
                AppName = AppName,
                AppVersion = AppVersion
            };
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));
                Result = connection.Query<int>(sql, values, commandType: CommandType.StoredProcedure).FirstOrDefault();

            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return Result;

        }

        public int sbWebApps_Sincroniza_Paso1y3(int Paso, int? Empresa, string? Cedula)
        {

            int Result = 0;
            const string sql = "spPortal_Sincroniza_WebApps";
            var values = new
            {
                Paso = Paso,
                Empresa = Empresa,
                Cedula = Cedula
            };
            try
            {

                using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));
                Result = connection.Execute(sql, values, commandType: CommandType.StoredProcedure);

            }
            catch (Exception ex)
            {

                _ = ex.Message;

            }
            return Result;
        }

        public int sbWebApps_Sincroniza_Paso2(int Paso, int Empresa, string Cedula) //REVISAR Y TRABAJAR EN ESTO (CONN A DB DIFERENTE)
        {

            int Result = 0;
            const string sql = "spPortal_Sincroniza_WebApps";
            var values = new
            {
                Paso = Paso,
                Empresa = Empresa,
                Cedula = Cedula
            };
            try
            {

                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                    Result = connection.Execute(sql, values, commandType: CommandType.StoredProcedure);

            }
            catch (Exception ex)
            {

                _ = ex.Message;

            }
            return Result;
        }

        public PgxClienteDto SeleccionarPgxClientePorCodEmpresa(int CodEmpresa)
        {
            PgxClienteDto Result = new PgxClienteDto();
            const string sql = "spPGX_W_Usuario_Access_tmp";
            var parameters = new { CodEmpresa = CodEmpresa };

            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));
                var queryResult = connection.Query<PgxClienteDto>(sql, parameters, commandType: CommandType.StoredProcedure).FirstOrDefault();
                if (queryResult != null)
                {
                    Result = queryResult;
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message; // Log or handle the exception as needed
            }

            return Result;
        }

        public void spCore_Usuario_Sincroniza(string pCliente, string pUsuario, string pNombre, string pEstado) //REVISAR Y TRABAJAR EN ESTO (CONN A DB DIFERENTE)
        {
            const string spName = "spSEG_SincronizaUsuarios";

            var parameters = new DynamicParameters();
            parameters.Add("@pCliente", pCliente);
            parameters.Add("@pUsuario", pUsuario);
            parameters.Add("@pNombre", pNombre);
            parameters.Add("@pEstado", pEstado);
            try
            {
                 using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));
                connection.Execute(spName, parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
        }

        public UsMenuDto ObtenerMenuPorNodoYUsuario(int pNodo, string Usuario)
        {
            
            try
            {
                Usuario = NormalizeUsuario(Usuario);
                UsMenuDto respuesta = new UsMenuDto();
                const string sql = @"select 
                                MENU_NODO,
                                NODO_PADRE ,
                                NODO_DESCRIPCION ,
                                TIPO ,
                                ICONO ,
                                MODO ,
                                MODAL ,
                                ACCESOS_DLL_ID ,
                                ACCESOS_DLL_CLS,
                                PRIORIDAD ,
                                FORMULARIO ,
                                MODULO ,
                                MIGRADO_WEB ,
                                ICONO_WEB ,
                                dbo.fxSEG_MenuAccess(1, @Usuario, Modulo, Formulario, Tipo) as 'Acceso' 
                            from us_menus where menu_nodo = @Nodo";
                var values = new
                {
                    Usuario = Usuario,
                    Nodo = pNodo,
                };

                using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));
                respuesta = connection.Query<UsMenuDto>(sql, values).FirstOrDefault() ?? new UsMenuDto();
                return respuesta;
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                return new UsMenuDto();
            }

            
        }

        public int ActualizarEstadisticasFavoritos(int menuNodo, int? Cliente, string Usuario)
        {
            
            try
            {
                Usuario = NormalizeUsuario(Usuario);
                int valorCliente = Cliente ?? 1; //si es null, asignar 1 por default
                Cliente = valorCliente;

                int Result = 0;
                const string sql = "spSEG_MenuUsos";
                var values = new
                {

                    Nodo = menuNodo,
                    Cliente = Cliente,
                    Usuario = Usuario,

                };

                using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));
                Result = connection.Query<int>(sql, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                return Result;
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                return -1;
            }
            

        }

        private static string NormalizeUsuario(string? usuario)
        {
            var normalized = (usuario ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(normalized))
                throw new ArgumentException("Usuario es requerido.", nameof(usuario));

            if (normalized.Length > 50)
                throw new ArgumentException("Usuario no es válido.", nameof(usuario));

            return normalized;
        }

    }//end class
}//end namespace
