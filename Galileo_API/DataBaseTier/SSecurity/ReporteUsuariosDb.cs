using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class ReporteUsuariosDb
    {
        private readonly IConfiguration _config;

        public ReporteUsuariosDb(IConfiguration config)
        {
            _config = config;
        }

        public List<ReporteUsuariosListaRespuestaDto> ReporteUsuariosListadoObtener(ReporteUsuariosListaSolicitudDto solicitudDto)
        {
            List<ReporteUsuariosListaRespuestaDto> result = null!;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
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
                    result = connection.Query<ReporteUsuariosListaRespuestaDto>(procedure, values, commandType: CommandType.StoredProcedure)!.ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return result;
        }

        public List<ReporteUsuariosRolesRespuestaDto> ReporteUsuariosRolesObtener(ReporteUsuariosRolesSolicitudDto solicitudDto)
        {
            List<ReporteUsuariosRolesRespuestaDto> result = null!;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var procedure = "[spSEG_Informe_Usuarios_Roles]";
                    var values = new
                    {
                        EmpresaId = solicitudDto.EmpresaId,
                        Usuario = solicitudDto.Usuario,
                        Contabiliza = solicitudDto.Contabiliza
                    };
                    result = connection.Query<ReporteUsuariosRolesRespuestaDto>(procedure, values, commandType: CommandType.StoredProcedure)!.ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return result;
        }

        public List<ReporteUsuariosPermisosRespuestaDto> ReporteUsuariosPermisosObtener(ReporteUsuariosPermisosSolicitudDto solicitudDto)
        {
            List<ReporteUsuariosPermisosRespuestaDto> result = null!;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var procedure = "[spSEG_Informe_Usuarios_Permisos]";
                    var values = new
                    {
                        EmpresaId = solicitudDto.EmpresaId,
                        Usuario = solicitudDto.Usuario,
                        Contabiliza = solicitudDto.Contabiliza
                    };
                    result = connection.Query<ReporteUsuariosPermisosRespuestaDto>(procedure, values, commandType: CommandType.StoredProcedure)!.ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return result;
        }

        public List<ReporteRolesPermisosRespuestaDto> ReporteRolesPermisosObtener(ReporteRolesPermisosSolicitudDto solicitudDto)
        {
            List<ReporteRolesPermisosRespuestaDto> result = null!;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var procedure = "[spSEG_Informe_Roles_Permisos]";
                    var values = new
                    {
                        RolId = solicitudDto.RolId
                    };
                    result = connection.Query<ReporteRolesPermisosRespuestaDto>(procedure, values, commandType: CommandType.StoredProcedure)!.ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return result;
        }

        public List<ReporteUsuarioRolesDto> RolesObtener()
        {
            List<ReporteUsuarioRolesDto> result = null!;

            string sql = "select COD_ROL as 'IdX', DESCRIPCION as 'ItmX' From US_ROLES Where ACTIVO = 1 order by DESCRIPCION";

            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString"));
                result = connection.Query<ReporteUsuarioRolesDto>(sql).ToList();

            }
            catch (Exception)
            {
                throw;
            }
            return result;
        }

        public List<ReporteUsuarioVinculacionDto> VinculacionesObtener(int codEmpresa)
        {
            List<ReporteUsuarioVinculacionDto> result = null!;

            string sql = "Select Usuario, Nombre, Estado_Desc, Vinculacion From vPGX_Usuarios_Empresa_Historica where cod_empresa = @CodEmpresa order by Nombre";
            var values = new
            {
                CodEmpresa = codEmpresa,
            };

            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString"));
                result = connection.Query<ReporteUsuarioVinculacionDto>(sql, values).ToList();
            }
            catch (Exception)
            {
                throw;
            }

            return result;
        }
    }
}
