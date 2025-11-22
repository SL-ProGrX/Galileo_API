using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.BusinessLogic
{
    public class FrmUsRolesBl
    {

        readonly FrmUsRolesDb RolesDB;

        public FrmUsRolesBl(IConfiguration config)
        {
            RolesDB = new FrmUsRolesDb(config);
        }

        public List<RolesObtenerDto> RolFiltroObtener(string filtro)
        {
            return RolesDB.RolFiltroObtener(filtro);
        }

        public List<RolesObtenerDto> RolesObtener()
        {
            return RolesDB.RolesObtener();
        }

        public ErrorDto RolGuardar(RolInsertarDto req)
        {
            return RolesDB.RolGuardar(req);
        }

        public ErrorDto RolEliminar(string RolId)
        {
            return RolesDB.RolEliminar(RolId);
        }

        public List<ClientesObtenerDto> ClientesObtener()
        {
            return RolesDB.ClientesObtener();
        }
    }
}
