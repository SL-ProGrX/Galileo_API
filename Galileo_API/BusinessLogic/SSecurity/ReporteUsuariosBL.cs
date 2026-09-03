using Galileo.DataBaseTier;
using Galileo.Models.Security;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic
{
    public class ReporteUsuariosBL
    {
        private readonly IConfiguration _config;

        public ReporteUsuariosBL(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<List<ReporteUsuariosListaRespuestaDto>> ReporteUsuariosListadoObtener(ReporteUsuariosListaSolicitudDto solicitudDto)
        {
            return new ReporteUsuariosDb(_config).ReporteUsuariosListadoObtener(solicitudDto);
        }

        public ErrorDto<List<ReporteUsuariosRolesRespuestaDto>> ReporteUsuariosRolesObtener(ReporteUsuariosRolesSolicitudDto solicitudDto)
        {
            return new ReporteUsuariosDb(_config).ReporteUsuariosRolesObtener(solicitudDto);
        }

        public ErrorDto<List<ReporteUsuariosPermisosRespuestaDto>> ReporteUsuariosPermisosObtener(ReporteUsuariosPermisosSolicitudDto solicitudDto)
        {
            return new ReporteUsuariosDb(_config).ReporteUsuariosPermisosObtener(solicitudDto);
        }

        public ErrorDto<List<ReporteRolesPermisosRespuestaDto>> ReporteRolesPermisosObtener(ReporteRolesPermisosSolicitudDto solicitudDto)
        {
            return new ReporteUsuariosDb(_config).ReporteRolesPermisosObtener(solicitudDto);
        }

        public ErrorDto<List<ReporteUsuarioRolesDto>> RolesObtener()
        {
            return new ReporteUsuariosDb(_config).RolesObtener();
        }

        public ErrorDto<List<ReporteUsuarioVinculacionDto>> VinculacionesObtener(int codEmpresa)
        {
            return new ReporteUsuariosDb(_config).VinculacionesObtener(codEmpresa);
        }
    }
}
