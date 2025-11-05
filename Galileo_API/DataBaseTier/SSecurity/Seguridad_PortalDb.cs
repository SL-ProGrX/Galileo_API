
using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class Seguridad_PortalDB
    {

        private readonly IConfiguration _config;

        public Seguridad_PortalDB(IConfiguration config)
        {
            _config = config;
        }

        public bool Sys_Portal_Admin_Valid(string Usuario)
        {
            int resp = 0;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var query = "SELECT dbo.fxSEG_Admin_Portal_Autenticate(@Usuario, @Token)";
                    var values = new
                    {
                        Usuario = Usuario,
                        Token = "#MyMasterK3y#"
                    };
                    resp = connection.Query<int>(query, values).FirstOrDefault();
                    //var procedure = "[fxSEG_Admin_Portal_Autenticate]";
                    //var values = new
                    //{
                    //    Usuario = Usuario,
                    //    Token = "#MyMasterK3y#"
                    //};
                    //resp = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp == 1 ? true : false;
        }


        public int UsuarioObtenerKeyAdmin(string Usuario)
        {
            int Result = 0;
            string sql = "select isnull(key_admin,0) as 'Admin' from us_usuarios where usuario = @Usuario";
            var values = new
            {
                Usuario = Usuario,
            };
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString"));
                Result = connection.Query<int>(sql, values).FirstOrDefault();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return Result;
        }

        public UsAdminClientesDTO US_ADMIN_CLIENTES_Obtener(string Usuario, int EmpresaId)
        {
            UsAdminClientesDTO Result = new UsAdminClientesDTO();
            string sql = "spSEG_Admin_Clients_Roles_Load";
            var values = new
            {
                Usuario = Usuario,
                EmpresaId = EmpresaId
            };
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString"));
                Result = connection.Query<UsAdminClientesDTO>(sql, values).FirstOrDefault();

            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return Result;
        }

        public UsuarioBloqueoDTO UsuarioBloqueoObtener(string Usuario)
        {

            UsuarioBloqueoDTO Result = new UsuarioBloqueoDTO();
            string sql = "spSEG_Bloqueo";
            var values = new
            {
                Usuario = Usuario,
            };
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString"));
                Result = connection.Query<UsuarioBloqueoDTO>(sql, values).FirstOrDefault();
                Result.Usuario = Usuario;

            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return Result;

        }

        public UsuarioCondicionDTO UsuarioCondicionObtener(string Usuario)
        {

            UsuarioCondicionDTO Result = new UsuarioCondicionDTO();
            string sql = "spSEG_USRCondicion";
            var values = new
            {
                Usuario = Usuario,
            };
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString"));
                Result = connection.Query<UsuarioCondicionDTO>(sql, values).FirstOrDefault();
                Result.Usuario = Usuario;

            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return Result;

        }

        public UsuarioVencimientoDTO UsuarioVencimientoObtener(string Usuario)
        {

            UsuarioVencimientoDTO Result = new UsuarioVencimientoDTO();
            string sql = "spSEG_Vencimiento";
            var values = new
            {
                Usuario = Usuario,
            };
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString"));
                Result = connection.Query<UsuarioVencimientoDTO>(sql, values).FirstOrDefault();
                Result.Usuario = Usuario;

            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return Result;

        }

        public int AppStatusObtener(string AppName, string AppVersion) //PREGUNTAR POR ESTE => (SP BLOQUEADO Y CUAL TABLA DEL DB?)
        {

            int Result = 0;
            string sql = "spSEG_App_Status";
            var values = new
            {
                AppName = AppName,
                AppVersion = AppVersion
            };
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString"));
                Result = connection.Query<int>(sql, values).FirstOrDefault();

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
            string sql = "spPortal_Sincroniza_WebApps";
            var values = new
            {
                Paso = Paso,
                Empresa = Empresa,
                Cedula = Cedula
            };
            try
            {

                using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString"));
                Result = connection.Query(sql, values).FirstOrDefault();

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
            string sql = "spPortal_Sincroniza_WebApps";
            var values = new
            {
                Paso = Paso,
                Empresa = Empresa,
                Cedula = Cedula
            };
            try
            {

                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                    Result = connection.Query(sql, values).FirstOrDefault();

            }
            catch (Exception ex)
            {

                _ = ex.Message;

            }
            return Result;
        }

        /// <summary>
        /// Método que crea la conexión a la BD dependiendo de la empresa seleccionada
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        //public PgxClienteDTO SeleccionarPgxClientePorCodEmpresa2(int CodEmpresa)
        //{

        //    PgxClienteDTO Result = new PgxClienteDTO();
        //    string sql = "select * from PGX_Clientes where cod_empresa = @CodEmpresa";
        //    var values = new
        //    {
        //        CodEmpresa = CodEmpresa,
        //    };
        //    try
        //    {
        //        using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString"));
        //        Result = connection.Query<PgxClienteDTO>(sql, values).FirstOrDefault();

        //    }
        //    catch (Exception ex)
        //    {
        //        _ = ex.Message;
        //    }
        //    return Result;

        //}

        public PgxClienteDTO SeleccionarPgxClientePorCodEmpresa(int CodEmpresa)
        {
            PgxClienteDTO Result = new PgxClienteDTO();
            string sql = "spPGX_W_Usuario_Access_tmp";
            var parameters = new { CodEmpresa = CodEmpresa };

            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString"));
                Result = connection.Query<PgxClienteDTO>(sql, parameters, commandType: CommandType.StoredProcedure).FirstOrDefault();
            }
            catch (Exception ex)
            {
                _ = ex.Message; // Log or handle the exception as needed
            }

            return Result;
        }


        public void spCore_Usuario_Sincroniza(string PGX_Core_Server, string PGX_Core_Key, string pCliente, string pUsuario, string pNombre, string pEstado) //REVISAR Y TRABAJAR EN ESTO (CONN A DB DIFERENTE)
        {
            //string sql = "spSEG_SincronizaUsuarios";
            var values = new
            {
                pCliente = pCliente,
                pUsuario = pUsuario,
                pNombre = pNombre,
                pEstado = pEstado,
            };
            try
            {
                //using var connection = new SqlConnection(stringConn);
                //Result = connection.Query<UsuarioVencimientoDTO>(sql, values).FirstOrDefault();

            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }


        }

        public UsMenuDTO ObtenerMenuPorNodoYUsuario(int pNodo, string Usuario)
        {
            UsMenuDTO respuesta = new UsMenuDTO();
            string sql = "select *, dbo.fxSEG_MenuAccess(1, @Usuario, Modulo, Formulario, Tipo) as 'Acceso' " +
             "from us_menus where menu_nodo = @Nodo";
            var values = new
            {
                Usuario = Usuario,
                Nodo = pNodo,
            };
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString"));
                respuesta = connection.Query<UsMenuDTO>(sql, values).FirstOrDefault();

            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }

            return respuesta;


        }

        public int ActualizarEstadisticasFavoritos(int menuNodo, int? Cliente, string Usuario)
        {
            int valorCliente = Cliente ?? 1; //si es null, asignar 1 por default
            Cliente = valorCliente;

            int Result = 0;
            string sql = "spSEG_MenuUsos";
            var values = new
            {

                Nodo = menuNodo,
                Cliente = Cliente,
                Usuario = Usuario,

            };
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString"));
                Result = connection.Query<int>(sql, values).FirstOrDefault();

            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return Result;

        }







    }//end class
}//end namespace
