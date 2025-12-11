using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Activos_Fijos;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class FrmActivosComprasController : ControllerBase
    {
        private readonly FrmActivosComprasBL _bl;
        public FrmActivosComprasController(IConfiguration config)
        {
            _bl = new FrmActivosComprasBL(config);
        }

        [Authorize]
        [HttpGet("Activos_ComprasPendientes_Consultar")]
        public ErrorDto<List<ActivosComprasPendientesRegistroData>> Activos_ComprasPendientes_Consultar(int CodEmpresa, DateTime fecha, string tipo)
        {
            return _bl.Activos_ComprasPendientes_Consultar(CodEmpresa, fecha, tipo);
        }
    }
}
