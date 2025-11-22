using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.Security;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReporteUsuariosController : ControllerBase
    {

        private readonly IConfiguration _config;

        public ReporteUsuariosController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("ReporteUsuariosListadoObtener")]
        //[Authorize]
        public List<ReporteUsuariosListaRespuestaDto> ReporteUsuariosListadoObtener(ReporteUsuariosListaSolicitudDto solicitudDto)
        {
            return new ReporteUsuariosBL(_config).ReporteUsuariosListadoObtener(solicitudDto);
        }


        [HttpPost("ReporteUsuariosRolesObtener")]
        //[Authorize]
        public List<ReporteUsuariosRolesRespuestaDto> ReporteUsuariosRolesObtener(ReporteUsuariosRolesSolicitudDto solicitudDto)
        {
            return new ReporteUsuariosBL(_config).ReporteUsuariosRolesObtener(solicitudDto);
        }


        [HttpPost("ReporteUsuariosPermisosObtener")]
        //[Authorize]
        public List<ReporteUsuariosPermisosRespuestaDto> ReporteUsuariosPermisosObtener(ReporteUsuariosPermisosSolicitudDto solicitudDto)
        {
            return new ReporteUsuariosBL(_config).ReporteUsuariosPermisosObtener(solicitudDto);
        }


        [HttpPost("ReporteRolesPermisosObtener")]
        //[Authorize]
        public List<ReporteRolesPermisosRespuestaDto> ReporteRolesPermisosObtener(ReporteRolesPermisosSolicitudDto solicitudDto)
        {
            return new ReporteUsuariosBL(_config).ReporteRolesPermisosObtener(solicitudDto);
        }


        [HttpGet("RolesObtener")]
        //[Authorize]
        public List<ReporteUsuarioRolesDto> RolesObtener()
        {
            return new ReporteUsuariosBL(_config).RolesObtener();
        }


        [HttpGet("VinculacionesObtener")]
        //[Authorize]
        public List<ReporteUsuarioVinculacionDto> VinculacionesObtener(int codEmpresa)
        {
            return new ReporteUsuariosBL(_config).VinculacionesObtener(codEmpresa);
        }
    }
}