using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.BusinessLogic
{
    public class FrmPgxClientesClasificaBl
    {
        readonly FrmPgxClientesClasificaDb ClientesClasificaDB;

        public FrmPgxClientesClasificaBl(IConfiguration config)
        {
            ClientesClasificaDB = new FrmPgxClientesClasificaDb(config);
        }

        public List<ClienteClasifica> Cliente_Clasifica_ObtenerTodos()
        {
            return ClientesClasificaDB.Cliente_Clasifica_ObtenerTodos();
        }

        public ErrorDto Cliente_Clasifica_Guardar(ClienteClasifica request)
        {
            return ClientesClasificaDB.Cliente_Clasifica_Guardar(request);
        }

        public ErrorDto Cliente_Clasifica_Eliminar(string request)
        {
            return ClientesClasificaDB.Cliente_Clasifica_Eliminar(request);
        }

        public List<ClienteSelecciona> Cliente_Selecciona_ObtenerTodos(string usuario)
        {
            return ClientesClasificaDB.Cliente_Selecciona_ObtenerTodos(usuario);
        }
    }
}