using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.BusinessLogic
{
    public class FrmUsRolesMembresiasBl
    {
        private readonly IConfiguration _config;

        public FrmUsRolesMembresiasBl(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto Acceso_Equipo(EstacionAsignaDto req)
        {
            return new FrmUsRolesMembresiasDb(_config).Acceso_Equipo(req);
        }

        public ErrorDto Acceso_Horario(HorarioAsignaDto req)
        {
            return new FrmUsRolesMembresiasDb(_config).Acceso_Horario(req);
        }

        public List<UsuariosConsultaDto> UsuariosConsultar(string? usuario, bool adminView, bool dirGlobal, int codEmpresa)
        {
            return new FrmUsRolesMembresiasDb(_config).UsuariosConsultar(usuario, adminView, dirGlobal, codEmpresa);
        }

        public List<UsuariosVinculadosConsultaDto> UsuariosVinculadosConsultar(string? usuario, bool contabiliza, bool adminView, int codEmpresa)
        {
            return new FrmUsRolesMembresiasDb(_config).UsuariosVinculadosConsultar(usuario, contabiliza, adminView, codEmpresa);
        }

        public Limites Limites_Obtener(string usuario, int codEmpresa)
        {
            return new FrmUsRolesMembresiasDb(_config).Limites_Obtener(usuario, codEmpresa);
        }

        public List<RolConsultaDto> RolesConsultar(string usuario, string? filtro, int codEmpresa)
        {
            return new FrmUsRolesMembresiasDb(_config).RolesConsultar(usuario, filtro, codEmpresa);
        }

        public List<HorarioConsultaDto> HorariosConsultar(string usuario, string? filtro, int codEmpresa)
        {
            return new FrmUsRolesMembresiasDb(_config).HorariosConsultar(usuario, filtro, codEmpresa);
        }

        public List<EstacionConsultaDto> EstacionesConsultar(string usuario, string? filtro, int codEmpresa)
        {
            return new FrmUsRolesMembresiasDb(_config).EstacionesConsultar(usuario, filtro, codEmpresa);
        }

        public ErrorDto UsuarioRolAsigna(UsuarioRolAsignaDto req)
        {
            return new FrmUsRolesMembresiasDb(_config).UsuarioRolAsigna(req);
        }

        public ErrorDto UsuarioClienteAsigna(UsuarioClienteAsigna req)
        {
            return new FrmUsRolesMembresiasDb(_config).UsuarioClienteAsigna(req);
        }

        public ErrorDto Limita_Equipo(LimitaAcceso req)
        {
            return new FrmUsRolesMembresiasDb(_config).Limita_Equipo(req);
        }

        public ErrorDto Limita_Horario(LimitaAcceso req)
        {
            return new FrmUsRolesMembresiasDb(_config).Limita_Horario(req);
        }
    }
}