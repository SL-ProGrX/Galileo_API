using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.Security;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Authorization;


namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmPgxVendedoresController : ControllerBase
    {
        private readonly IConfiguration _config;

        public FrmPgxVendedoresController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("Vendedor_ObtenerTodos")]
        public ErrorDto<List<Vendedor>> Vendedor_ObtenerTodos()
        {
            return new FrmPgxVendedoresBL(_config).Vendedor_ObtenerTodos();
        }


        [HttpPost("Vendedor_Insertar")]
        public ErrorDto Vendedor_Insertar(Vendedor request)
        {
            return new FrmPgxVendedoresBL(_config).Vendedor_Insertar(request);
        }

        [HttpPost("Vendedor_Eliminar")]
        public ErrorDto Vendedor_Eliminar(Vendedor request)
        {
            return new FrmPgxVendedoresBL(_config).Vendedor_Eliminar(request);
        }


        [HttpPost("Vendedor_Actualizar")]
        public ErrorDto Vendedor_Actualizar(Vendedor request)
        {
            return new FrmPgxVendedoresBL(_config).Vendedor_Actualizar(request);
        }
    }
}
