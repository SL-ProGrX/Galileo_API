using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.Security;
using Galileo.Models.ERROR;


namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VendedorController : ControllerBase
    {
        private readonly IConfiguration _config;

        public VendedorController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("Vendedor_ObtenerTodos")]
        // [Authorize]
        public List<Vendedor> Vendedor_ObtenerTodos()
        {
            return new VendedorBL(_config).Vendedor_ObtenerTodos();
        }


        [HttpPost("Vendedor_Insertar")]
        // [Authorize]
        public ErrorDto Vendedor_Insertar(Vendedor request)
        {
            return new VendedorBL(_config).Vendedor_Insertar(request);
        }

        [HttpPost("Vendedor_Eliminar")]
        //[Authorize]
        public ErrorDto Vendedor_Eliminar(Vendedor request)
        {
            return new VendedorBL(_config).Vendedor_Eliminar(request);
        }


        [HttpPost("Vendedor_Actualizar")]
        //[Authorize]
        public ErrorDto Vendedor_Actualizar(Vendedor request)
        {
            return new VendedorBL(_config).Vendedor_Actualizar(request);
        }
    }
}