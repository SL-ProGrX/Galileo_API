using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmInvReporteInventariosController : ControllerBase
    {
        private readonly FrmInvReporteInventariosBL _bl;
        public FrmInvReporteInventariosController(IConfiguration config)
        {
            _bl = new FrmInvReporteInventariosBL(config);
        }

        [HttpGet("Obtener_Bodegas")]
        public ErrorDto<List<BodegaReporteInvMCdto>> Obtener_Bodegas(int CodEmpresa)
        {
            return _bl.Obtener_Bodegas(CodEmpresa);
        }

        [HttpGet("Obtener_Lineas")]
        public ErrorDto<List<LineasInvMCdto>> Obtener_Lineas(int CodEmpresa)
        {
            return _bl.Obtener_Lineas(CodEmpresa);
        }
    }
}