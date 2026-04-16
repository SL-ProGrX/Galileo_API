using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Hipotecario;
using Galileo_API.Models.ProGrX_Hipotecario;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Hipotecario
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmVivTiposDesembolsosController : ControllerBase
    {
        private readonly FrmVivTiposDesembolsosBl _bl;

        public FrmVivTiposDesembolsosController(IConfiguration config)
        {
            _bl = new FrmVivTiposDesembolsosBl(config);
        }

        [HttpGet("VivTiposDesembolsos_Obtener")]
        public ErrorDto<List<VivTiposDesembolsosData>> VivTiposDesembolsos_Obtener(int codEmpresa)
        {
            return _bl.VivTiposDesembolsos_Obtener(codEmpresa);
        }

        [HttpPost("VivTiposDesembolsos_Guardar")]
        public ErrorDto VivTiposDesembolsos_Guardar(int codEmpresa, int operacion, VivTiposDesembolsosData request)
        {
            return _bl.VivTiposDesembolsos_Guardar(codEmpresa, operacion, request);
        }

        [HttpDelete("VivTiposDesembolsos_Eliminar")]
        public ErrorDto VivTiposDesembolsos_Eliminar(int codEmpresa, string codigo, string usuario)
        {
            return _bl.VivTiposDesembolsos_Eliminar(codEmpresa, codigo, usuario);
        }
    }
}
