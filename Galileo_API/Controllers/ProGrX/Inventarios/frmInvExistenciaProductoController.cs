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
    public class FrmInvExistenciaProductoController : ControllerBase
    {
        private readonly FrmInvExistenciaProductoBL _bl;
        public FrmInvExistenciaProductoController(IConfiguration config)
        {
            _bl = new FrmInvExistenciaProductoBL(config);
        }

        [HttpGet("ExistenciaProducto_Obtener")]
        public ErrorDto<List<ExistenciaProductoDto>> existenciaProducto_Obtener(int CodCliente, string filtros)
        {
            return _bl.existenciaProducto_Obtener(CodCliente, filtros);
        }
    }
}