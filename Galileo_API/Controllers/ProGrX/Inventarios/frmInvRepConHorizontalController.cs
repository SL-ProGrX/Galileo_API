using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmInvRepConHorizontalController : ControllerBase
    {
        private readonly FrmInvRepConHorizontalBL _bl;
        public FrmInvRepConHorizontalController(IConfiguration config)
        {
            _bl = new FrmInvRepConHorizontalBL(config);
        }

        [HttpGet("Obtener_Bodegas")]
        // [Authorize]
        public ErrorDto<List<RepBodegaDto>> ListadoPrecios_Obtener(int CodEmpresa)
        {
            return _bl.Obtener_Bodegas(CodEmpresa);
        }
    }
}