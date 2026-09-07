using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class FrmPgxClientesClasificaController : ControllerBase
    {
        readonly FrmPgxClientesClasificaBl ClientesClasificaBL;

        public FrmPgxClientesClasificaController(IConfiguration config)
        {
            ClientesClasificaBL = new FrmPgxClientesClasificaBl(config);
        }

        [HttpGet("ClienteClasifica_ObtenerTodos")]
        [Authorize]
        public ErrorDto<List<ClienteClasifica>> Cliente_Clasifica_ObtenerTodos()
        {
            return ClientesClasificaBL.Cliente_Clasifica_ObtenerTodos();
        }


        [HttpPost("ClienteClasifica_Guardar")]
        [Authorize]
        public ErrorDto Cliente_Clasifica_Guardar(ClienteClasifica request)
        {
            return ClientesClasificaBL.Cliente_Clasifica_Guardar(request);
        }


        [HttpDelete("ClienteClasifica_Eliminar")]
        [Authorize]
        public ErrorDto Cliente_Clasifica_Eliminar(string request, int codEmpresa = 0, string usuario = "")
        {
            return ClientesClasificaBL.Cliente_Clasifica_Eliminar(request, codEmpresa, usuario);
        }


        [HttpGet("Cliente_Selecciona_ObtenerTodos")]
        [Authorize]
        public List<ClienteSelecciona> Cliente_Selecciona_ObtenerTodos(string usuario)
        {
            return ClientesClasificaBL.Cliente_Selecciona_ObtenerTodos(usuario);
        }
    }
}
