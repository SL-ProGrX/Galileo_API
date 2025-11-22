using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.BusinessLogic
{
    public class FrmPgxClientesTiposIDsBl
    {
        readonly FrmPgxClientesTiposIDsDb ClientesTiposIDsDB;

        public FrmPgxClientesTiposIDsBl(IConfiguration config)
        {
            ClientesTiposIDsDB = new FrmPgxClientesTiposIDsDb(config);
        }

        public List<TipoId> TipoId_ObtenerTodos()
        {
            return ClientesTiposIDsDB.TipoId_ObtenerTodos();
        }

        public ErrorDto TipoId_Guardar(TipoId request)
        {
            return ClientesTiposIDsDB.TipoId_Guardar(request);
        }

        public ErrorDto TipoId_Eliminar(string tipo_id)
        {
            return ClientesTiposIDsDB.TipoId_Eliminar(tipo_id);
        }
    }
}