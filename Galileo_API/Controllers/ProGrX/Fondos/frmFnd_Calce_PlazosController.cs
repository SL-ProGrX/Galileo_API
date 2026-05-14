using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers.ProGrX.Fondos
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmFndCalcePlazosController : ControllerBase
    {
        private readonly FrmFndCalcePlazosBL _BL;

        public FrmFndCalcePlazosController(IConfiguration config)
        {
            _BL = new FrmFndCalcePlazosBL(config);
        }

        [HttpGet("Periodos_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>> Periodos_Lista(int CodEmpresa)
        {
            return _BL.Periodos_Lista(CodEmpresa);
        }
    }
}