using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCOIncobrablesListController : ControllerBase
    {
        private readonly FrmCOIncobrablesListBL _bl;

        public FrmCOIncobrablesListController(IConfiguration config)
        {
            _bl = new FrmCOIncobrablesListBL(config);
        }

        [Authorize]
        [HttpGet("Nombre_Obtener")]
        public ErrorDto<string> Nombre_Obtener(int codEmpresa, string cedula)
        {
            return _bl.Nombre_Obtener(codEmpresa, cedula);
        }

        [Authorize]
        [HttpGet("CoIncobrablesList_Obtener")]
        public ErrorDto<List<CbrIncobrableListaItem>> CoIncobrablesList_Obtener(int codEmpresa, string cedula)
        {
            return _bl.CoIncobrablesList_Obtener(codEmpresa, cedula);
        }

        [Authorize]
        [HttpGet("CoIncobrablesListMovimientos_Obtener")]
        public ErrorDto<List<CbrIncobrableMovimientoItem>> CoIncobrablesListMovimientos_Obtener(
            int codEmpresa,
            int operacion,
            int cxcOperacion)
        {
            return _bl.CoIncobrablesListMovimientos_Obtener(codEmpresa, operacion, cxcOperacion);
        }
    }
}
