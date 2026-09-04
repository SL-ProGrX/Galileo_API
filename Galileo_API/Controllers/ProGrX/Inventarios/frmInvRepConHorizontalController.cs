using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmInvRepConHorizontalController : ControllerBase
    {
        private readonly FrmInvRepConHorizontalBl _bl;
        public FrmInvRepConHorizontalController(IConfiguration config)
        {
            _bl = new FrmInvRepConHorizontalBl(config);
        }

        [HttpGet("Obtener_Bodegas")]
        public ErrorDto<List<DropDownListaGenericaModel>> ListadoPrecios_Obtener(int CodEmpresa)
        {
            return _bl.Obtener_Bodegas(CodEmpresa);
        }
    }
}