using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.Controllers
{
    [Route("api/FrmUsRolesMembresias")]
    [Route("api/frmUS_Roles_Membresias")]
    [ApiController]
    public class FrmUsRolesMembresiasController : ControllerBase
    {
        private readonly IConfiguration _config;

        public FrmUsRolesMembresiasController(IConfiguration config)
        {
            _config = config;
        }


        [HttpPost("Acceso_Equipo")]
        //[Authorize]
        public ErrorDto Acceso_Equipo(EstacionAsignaDto req)
        {
            return new FrmUsRolesMembresiasBl(_config).Acceso_Equipo(req);
        }


        [HttpPost("Acceso_Horario")]
        //[Authorize]
        public ErrorDto Acceso_Horario(HorarioAsignaDto req)
        {
            return new FrmUsRolesMembresiasBl(_config).Acceso_Horario(req);
        }


        [HttpGet("UsuariosConsultar")]
        //[Authorize]
        public List<UsuariosConsultaDto> UsuariosConsultar(string? usuario, bool adminView, bool dirGlobal, int codEmpresa)
        {
            return new FrmUsRolesMembresiasBl(_config).UsuariosConsultar(usuario, adminView, dirGlobal, codEmpresa);
        }


        [HttpGet("UsuariosVinculadosConsultar")]
        //[Authorize]
        public List<UsuariosVinculadosConsultaDto> UsuariosVinculadosConsultar2(string? usuario, bool contabiliza, bool adminView, int codEmpresa)
        {
            return new FrmUsRolesMembresiasBl(_config).UsuariosVinculadosConsultar(usuario, contabiliza, adminView, codEmpresa);
        }


        [HttpGet("Limites_Obtener")]
        //[Authorize]
        public Limites Limites_Obtener(string usuario, int codEmpresa)
        {
            return new FrmUsRolesMembresiasBl(_config).Limites_Obtener(usuario, codEmpresa);
        }


        [HttpGet("RolesConsultar")]
        //[Authorize]
        public List<RolConsultaDto> RolesConsultar(string usuario, string? filtro, int codEmpresa)
        {
            return new FrmUsRolesMembresiasBl(_config).RolesConsultar(usuario, filtro, codEmpresa);
        }


        [HttpGet("HorariosConsultar")]
        //[Authorize]
        public List<HorarioConsultaDto> HorariosConsultar(string usuario, string? filtro, int codEmpresa)
        {
            return new FrmUsRolesMembresiasBl(_config).HorariosConsultar(usuario, filtro, codEmpresa);
        }


        [HttpGet("EstacionesConsultar")]
        //[Authorize]
        public List<EstacionConsultaDto> EstacionesConsultar(string usuario, string? filtro, int codEmpresa)
        {
            return new FrmUsRolesMembresiasBl(_config).EstacionesConsultar(usuario, filtro, codEmpresa);
        }


        [HttpPost("UsuarioRolAsigna")]
        //[Authorize]
        public ErrorDto UsuarioRolAsigna(UsuarioRolAsignaDto req)
        {
            return new FrmUsRolesMembresiasBl(_config).UsuarioRolAsigna(req);
        }


        [HttpPost("UsuarioClienteAsigna")]
        //[Authorize]
        public ErrorDto UsuarioClienteAsigna(UsuarioClienteAsigna req)
        {
            return new FrmUsRolesMembresiasBl(_config).UsuarioClienteAsigna(req);
        }


        [HttpPost("Limita_Equipo")]
        //[Authorize]
        public ErrorDto Limita_Equipo(LimitaAcceso req)
        {
            return new FrmUsRolesMembresiasBl(_config).Limita_Equipo(req);
        }


        [HttpPost("Limita_Horario")]
        //[Authorize]
        public ErrorDto Limita_Horario(LimitaAcceso req)
        {
            return new FrmUsRolesMembresiasBl(_config).Limita_Horario(req);
        }

    }
}