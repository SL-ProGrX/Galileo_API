using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class AdminRolDB
    {
        private readonly IConfiguration _config;
        private const string connectionStringName = "DefaultConnString";


        public AdminRolDB(IConfiguration config)
        {
            _config = config;
        }

        public List<UsuarioPlataforma> UsuarioPlataforma_Obtener(string? usuarioFiltro)
        {
            List<UsuarioPlataforma> data = new List<UsuarioPlataforma>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spSEG_Usuarios_List]";

                    var values = new
                    {
                        Filtro = usuarioFiltro == null ? "" : usuarioFiltro,
                    };

                    data = connection.Query<UsuarioPlataforma>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return data;
        }

        public List<UsuarioAdmin> UsuarioAdmin_Obtener(string? usuarioFiltro)
        {
            List<UsuarioAdmin> data = new List<UsuarioAdmin>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spSEG_Admin_Local_List]";

                    var values = new
                    {
                        Filtro = usuarioFiltro == null ? "" : usuarioFiltro,
                    };

                    data = connection.Query<UsuarioAdmin>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return data;
        }

        public List<ClienteAsignado> ClientesAsigna_Obtener(string usuario, string? clienteFiltro)
        {
            List<ClienteAsignado> data = new List<ClienteAsignado>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spSEG_Admin_Clients_Load]";

                    var values = new
                    {
                        Usuario = usuario,
                        Filtro = clienteFiltro == null ? "" : clienteFiltro,
                    };

                    data = connection.Query<ClienteAsignado>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return data;
        }


        public AdminLocalRoles AdminRoles_Obtener(string usuario)
        {
            AdminLocalRoles data = new AdminLocalRoles();

            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spSEG_Admin_Local_Load]";

                    var values = new
                    {
                        Usuario = usuario,
                    };
                    data = connection.Query<AdminLocalRoles>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault() ?? new AdminLocalRoles();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return data;
        }

        public AdminLocalRolesCliente AdminRolesCliente_Obtener(string usuario, int cliente)
        {
            AdminLocalRolesCliente data = new AdminLocalRolesCliente();

            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spSEG_Admin_Clients_Roles_Load]";

                    var values = new
                    {
                        Usuario = usuario,
                        EmpresaId = cliente,
                    };
                    data = connection.Query<AdminLocalRolesCliente>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault() ?? new AdminLocalRolesCliente();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return data;
        }



        public ErrorDto AdminLocal_Insertar(AdminLocalInsert request)
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spSEG_Admin_Local_Add]";
                    var values = new
                    {
                        Usuario = request.Usuario,
                        Mov = request.Mov,
                        UsuarioRegister = request.UsuarioRegister,

                        R_Local_Grants = request.R_Local_Grants,
                        R_Local_Users = request.R_Local_Users,
                        R_Local_Key_Reset = request.R_Local_Key_Reset,
                        R_Global_Dir_Search = request.R_Global_Dir_Search,
                        R_Admin_Review = request.R_Admin_Review,

                        Propaga_Clientes = request.Propaga_Clientes,

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



        public ErrorDto AdminClienteRoles_Insertar(AdminLocalRolesInsert request)
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spSEG_Admin_Clients_Roles_Add]";
                    var values = new
                    {
                        Usuario = request.Usuario,
                        ClientId = request.ClienteId,
                        Mov = request.Mov,
                        UsuarioRegister = request.UsuarioRegister,

                        R_Local_Grants = request.R_Local_Grants,
                        R_Local_Users = request.R_Local_Users,
                        R_Local_Key_Reset = request.R_Local_Key_Reset,
                        R_Global_Dir_Search = request.R_Global_Dir_Search,
                        R_Admin_Review = request.R_Admin_Review,


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
    }
}
