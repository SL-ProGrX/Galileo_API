using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.Security;
using Galileo.Models.ERROR;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class ReporteUsuariosDb
    {
        private readonly IConfiguration _config;
        private const string connectionStringName = "DefaultConnString";

        public ReporteUsuariosDb(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<List<ReporteUsuariosListaRespuestaDto>> ReporteUsuariosListadoObtener(ReporteUsuariosListaSolicitudDto solicitudDto)
        {
            var response = new ErrorDto<List<ReporteUsuariosListaRespuestaDto>> { Code = 0, Result = [] };
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spSEG_Informe_Usuarios_Lista]";
                    var values = new
                    {
                        EmpresaId = solicitudDto.EmpresaId,
                        Usuario = solicitudDto.Usuario,
                        Estado = solicitudDto.Estado,
                        Vinculado = solicitudDto.Vinculado,
                        Contabiliza = solicitudDto.Contabiliza
                    };
                    response.Result = connection.Query<ReporteUsuariosListaRespuestaDto>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex) { response.Code = -1; response.Description = ex.Message; }
            return response;
        }

        public ErrorDto<List<ReporteUsuariosRolesRespuestaDto>> ReporteUsuariosRolesObtener(ReporteUsuariosRolesSolicitudDto solicitudDto)
        {
            var response = new ErrorDto<List<ReporteUsuariosRolesRespuestaDto>> { Code = 0, Result = [] };
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spSEG_Informe_Usuarios_Roles]";
                    var values = new
                    {
                        EmpresaId = solicitudDto.EmpresaId,
                        Usuario = solicitudDto.Usuario,
                        Contabiliza = solicitudDto.Contabiliza
                    };
                    response.Result = connection.Query<ReporteUsuariosRolesRespuestaDto>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex) { response.Code = -1; response.Description = ex.Message; }
            return response;
        }

        public ErrorDto<List<ReporteUsuariosPermisosRespuestaDto>> ReporteUsuariosPermisosObtener(ReporteUsuariosPermisosSolicitudDto solicitudDto)
        {
            var response = new ErrorDto<List<ReporteUsuariosPermisosRespuestaDto>> { Code = 0, Result = [] };
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spSEG_Informe_Usuarios_Permisos]";
                    var values = new
                    {
                        EmpresaId = solicitudDto.EmpresaId,
                        Usuario = solicitudDto.Usuario,
                        Contabiliza = solicitudDto.Contabiliza
                    };
                    response.Result = connection.Query<ReporteUsuariosPermisosRespuestaDto>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex) { response.Code = -1; response.Description = ex.Message; }
            return response;
        }

        public ErrorDto<List<ReporteRolesPermisosRespuestaDto>> ReporteRolesPermisosObtener(ReporteRolesPermisosSolicitudDto solicitudDto)
        {
            var response = new ErrorDto<List<ReporteRolesPermisosRespuestaDto>> { Code = 0, Result = [] };
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spSEG_Informe_Roles_Permisos]";
                    var values = new
                    {
                        RolId = solicitudDto.RolId
                    };
                    response.Result = connection.Query<ReporteRolesPermisosRespuestaDto>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex) { response.Code = -1; response.Description = ex.Message; }
            return response;
        }

        public ErrorDto<List<ReporteUsuarioRolesDto>> RolesObtener()
        {
            var response = new ErrorDto<List<ReporteUsuarioRolesDto>> { Code = 0, Result = [] };

            string sql = "select COD_ROL as 'IdX', DESCRIPCION as 'ItmX' From US_ROLES Where ACTIVO = 1 order by DESCRIPCION";

            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));
                response.Result = connection.Query<ReporteUsuarioRolesDto>(sql).ToList();
            }
            catch (Exception ex) { response.Code = -1; response.Description = ex.Message; }
            return response;
        }

        public ErrorDto<List<ReporteUsuarioVinculacionDto>> VinculacionesObtener(int codEmpresa)
        {
            var response = new ErrorDto<List<ReporteUsuarioVinculacionDto>> { Code = 0, Result = [] };

            string sql = "Select Usuario, Nombre, Estado_Desc, Vinculacion From vPGX_Usuarios_Empresa_Historica where cod_empresa = @CodEmpresa order by Nombre";
            var values = new
            {
                CodEmpresa = codEmpresa,
            };

            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));
                response.Result = connection.Query<ReporteUsuarioVinculacionDto>(sql, values).ToList();
            }
            catch (Exception ex) { response.Code = -1; response.Description = ex.Message; }

            return response;
        }
    }
}
