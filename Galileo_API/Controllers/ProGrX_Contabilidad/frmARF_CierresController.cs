using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmArfCierresController : ControllerBase
    {
        private readonly FrmArfCierresBl _bl;

        public FrmArfCierresController(IConfiguration config) => _bl = new FrmArfCierresBl(config);
        
        [HttpGet("ARFCierres_CorteActual_Obtener")]
        public ErrorDto<ArfCierreData?> ARFCierres_CorteActual_Obtener(int codEmpresa)
        {
            return _bl.ARFCierres_CorteActual_Obtener(codEmpresa);
        }

        [HttpPost("ARFCierres_Cerrar")]
        public ErrorDto ARFCierres_Cerrar(int codEmpresa, ArfCierreData request)
        {
            return _bl.ARFCierres_Cerrar(codEmpresa, request);
        }
    }
}