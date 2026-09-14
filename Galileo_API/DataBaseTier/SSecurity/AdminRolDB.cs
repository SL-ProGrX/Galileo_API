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

        public ErrorDto<List<UsuarioPlataforma>> UsuarioPlataforma_Obtener(string? usuarioFiltro)
        {
            var response = new ErrorDto<List<UsuarioPlataforma>> { Result = [], Code = 0 };
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spSEG_Usuarios_List]";

                    var values = new
                    {
                        Filtro = usuarioFiltro == null ? "" : usuarioFiltro,
                    };

                    response.Result = connection.Query<UsuarioPlataforma>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                }
                response.Description = "Ok";
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        public ErrorDto<List<UsuarioAdmin>> UsuarioAdmin_Obtener(string? usuarioFiltro)
        {
            var response = new ErrorDto<List<UsuarioAdmin>> { Result = [], Code = 0 };
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spSEG_Admin_Local_List]";

                    var values = new
                    {
                        Filtro = usuarioFiltro == null ? "" : usuarioFiltro,
                    };

                    response.Result = connection.Query<UsuarioAdmin>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                }
                response.Description = "Ok";
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        public ErrorDto<List<ClienteAsignado>> ClientesAsigna_Obtener(string usuario, string? clienteFiltro)
        {
            var response = new ErrorDto<List<ClienteAsignado>> { Result = [], Code = 0 };
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

                    response.Result = connection.Query<ClienteAsignado>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                }
                response.Description = "Ok";
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }


        public ErrorDto<AdminLocalRoles> AdminRoles_Obtener(string usuario)
        {
            var response = new ErrorDto<AdminLocalRoles> { Result = new AdminLocalRoles(), Code = 0 };

            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spSEG_Admin_Local_Load]";

                    var values = new
                    {
                        Usuario = usuario,
                    };
                    response.Result = connection.Query<AdminLocalRoles>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault() ?? new AdminLocalRoles();

                }
                response.Description = "Ok";
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        public ErrorDto<AdminLocalRolesCliente> AdminRolesCliente_Obtener(string usuario, int cliente)
        {
            var response = new ErrorDto<AdminLocalRolesCliente> { Result = new AdminLocalRolesCliente(), Code = 0 };

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
                    response.Result = connection.Query<AdminLocalRolesCliente>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault() ?? new AdminLocalRolesCliente();

                }
                response.Description = "Ok";
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
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
