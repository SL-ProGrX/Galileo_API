using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.BusinessLogic
{
    public class VendedorBL
    {
        private readonly IConfiguration _config;

        public VendedorBL(IConfiguration config)
        {
            _config = config;
        }

        public List<Vendedor> Vendedor_ObtenerTodos()
        {
            return new VendedorDB(_config).Vendedor_ObtenerTodos();
        }

        public ErrorDto Vendedor_Insertar(Vendedor request)
        {

            return new VendedorDB(_config).Vendedor_Insertar(request);
        }

        public ErrorDto Vendedor_Eliminar(Vendedor request)
        {

            return new VendedorDB(_config).Vendedor_Eliminar(request);
        }

        public ErrorDto Vendedor_Actualizar(Vendedor request)
        {

            return new VendedorDB(_config).Vendedor_Actualizar(request);
        }
    }
}