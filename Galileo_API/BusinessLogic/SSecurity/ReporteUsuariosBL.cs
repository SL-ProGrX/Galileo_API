using Galileo.DataBaseTier;
using Galileo.Models.Security;

namespace Galileo.BusinessLogic
{
    public class ReporteUsuariosBL
    {
        private readonly IConfiguration _config;

        public ReporteUsuariosBL(IConfiguration config)
        {
            _config = config;
        }

        public List<ReporteUsuariosListaRespuestaDto> ReporteUsuariosListadoObtener(ReporteUsuariosListaSolicitudDto solicitudDto)
        {
            return new ReporteUsuariosDb(_config).ReporteUsuariosListadoObtener(solicitudDto);
        }

        public List<ReporteUsuariosRolesRespuestaDto> ReporteUsuariosRolesObtener(ReporteUsuariosRolesSolicitudDto solicitudDto)
        {
            return new ReporteUsuariosDb(_config).ReporteUsuariosRolesObtener(solicitudDto);
        }

        public List<ReporteUsuariosPermisosRespuestaDto> ReporteUsuariosPermisosObtener(ReporteUsuariosPermisosSolicitudDto solicitudDto)
        {
            return new ReporteUsuariosDb(_config).ReporteUsuariosPermisosObtener(solicitudDto);
        }

        public List<ReporteRolesPermisosRespuestaDto> ReporteRolesPermisosObtener(ReporteRolesPermisosSolicitudDto solicitudDto)
        {
            return new ReporteUsuariosDb(_config).ReporteRolesPermisosObtener(solicitudDto);
        }

        public List<ReporteUsuarioRolesDto> RolesObtener()
        {
            return new ReporteUsuariosDb(_config).RolesObtener();
        }

        public List<ReporteUsuarioVinculacionDto> VinculacionesObtener(int codEmpresa)
        {
            return new ReporteUsuariosDb(_config).VinculacionesObtener(codEmpresa);
        }
    }
}
