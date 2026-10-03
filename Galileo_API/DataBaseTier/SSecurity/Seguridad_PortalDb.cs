
using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.Auth;
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
            Usuario = NormalizeUsuario(Usuario);

            using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));
            const string query = "SELECT dbo.fxSEG_Admin_Portal_Autenticate(@Usuario, @Token)";
            var values = new
            {
                Usuario,
                Token = "#MyMasterK3y#"
            };

            var resultado = connection.Query<int>(query, values).FirstOrDefault();
            return resultado == 1;
        }

        public int UsuarioObtenerKeyAdmin(string Usuario)
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

        public UsAdminClientesDto US_ADMIN_CLIENTES_Obtener(string Usuario, int EmpresaId)
        {
            Usuario = NormalizeUsuario(Usuario);

            const string sql = "spSEG_Admin_Clients_Roles_Load";
            var values = new
            {
                Usuario,
                EmpresaId
            };

            using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));
            return connection.Query<UsAdminClientesDto>(sql, values, commandType: CommandType.StoredProcedure).FirstOrDefault()
                ?? new UsAdminClientesDto();
        }

        public UsuarioBloqueoDto UsuarioBloqueoObtener(string Usuario)
        {
            Usuario = NormalizeUsuario(Usuario);
            const string sql = "spSEG_Bloqueo";
            using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));
            var result = connection.QueryFirstOrDefault<UsuarioBloqueoDto>(sql, new { Usuario }, commandType: CommandType.StoredProcedure)
                ?? throw new InvalidOperationException("spSEG_Bloqueo no devolvió información para el usuario.");
            result.Usuario = Usuario;
            return result;
        }

        public UsuarioCondicionDto UsuarioCondicionObtener(string Usuario)
        {
            Usuario = NormalizeUsuario(Usuario);
            const string sql = "spSEG_USRCondicion";
            using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));
            var result = connection.QueryFirstOrDefault<UsuarioCondicionDto>(sql, new { Usuario }, commandType: CommandType.StoredProcedure)
                ?? throw new InvalidOperationException("spSEG_USRCondicion no devolvió información para el usuario.");
            result.Usuario = Usuario;
            return result;
        }

        public UsuarioVencimientoDto UsuarioVencimientoObtener(string Usuario)
        {
            Usuario = NormalizeUsuario(Usuario);
            const string sql = "spSEG_Vencimiento";
            using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));
            var result = connection.QueryFirstOrDefault<UsuarioVencimientoDto>(sql, new { Usuario }, commandType: CommandType.StoredProcedure)
                ?? throw new InvalidOperationException("spSEG_Vencimiento no devolvió información para el usuario.");
            result.Usuario = Usuario;
            return result;
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

        public AppStatusDto AppStatusCompletoObtener(string AppName, string AppVersion)
        {
            const string sql = "spSEG_App_Status";
            var values = new
            {
                AppName,
                AppVersion
            };

            using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));
            return connection.QueryFirstOrDefault<AppStatusDto>(
                sql,
                values,
                commandType: CommandType.StoredProcedure)
                ?? throw new InvalidOperationException("spSEG_App_Status no devolvió información para Galileo Web.");
        }

        public WebAccessResultDto RegistrarDispositivoWeb(
            int EmpresaId,
            string Usuario,
            Guid DeviceId,
            string IpAddress,
            string AppVersion)
        {
            Usuario = NormalizeUsuario(Usuario);
            const string sql = "spSEG_Web_Access_Limit";
            var values = new
            {
                Empresa = EmpresaId,
                Usuario,
                DeviceId,
                IpAddress,
                AppVersion
            };

            using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));
            return connection.QueryFirstOrDefault<WebAccessResultDto>(
                sql,
                values,
                commandType: CommandType.StoredProcedure)
                ?? throw new InvalidOperationException("spSEG_Web_Access_Limit no devolvió información.");
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
                UsMenuDto respuesta;
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
