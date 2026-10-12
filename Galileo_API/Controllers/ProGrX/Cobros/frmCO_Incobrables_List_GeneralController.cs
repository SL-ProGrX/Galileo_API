using Galileo.Models.ERROR;
using Galileo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc; 
using Galileo_API.BusinessLogic.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;
using Galileo.Models.ProGrX.Cobros;

namespace Galileo_API.Controllers.ProGrX.Cobros
{

    [Route("api/[controller]")]
    [ApiController]

    public class FrmCoIncobrablesListGeneralController : ControllerBase
    {

        private readonly FrmCoIncobrablesListGeneralBL _bl;

        public FrmCoIncobrablesListGeneralController(IConfiguration config)
            => _bl = new FrmCoIncobrablesListGeneralBL(config);

        [Authorize]
        [HttpGet("CoIncobrablesFechaServidor_Obtener")]
        public ErrorDto<DateTime> CoIncobrablesFechaServidor_Obtener(int CodEmpresa)
        {
            return _bl.CoIncobrablesFechaServidor_Obtener(CodEmpresa);
        }


        [Authorize]
        [HttpGet("CoIncobrablesListMovimiento_Obtener")]
        public ErrorDto<List<CbrIncobrableMovimientos>> CoIncobrablesListMovimiento_Obtener(int CodEmpresa, string pOperacion, string pCxC_Operacion)
        {
            return _bl.CoIncobrablesListMovimiento_Obtener(CodEmpresa, pOperacion, pCxC_Operacion);
        }

        [Authorize]
        [HttpGet("CoIncobrablesListGeneral_Obtener")]
        public ErrorDto<List<CbrIncobrableGeneral>> CoIncobrablesListGeneral_Obtener(int CodEmpresa, [FromQuery] CbrIncobrableFiltros filtros)
        {
            return _bl.CoIncobrablesListGeneral_Obtener(CodEmpresa,  filtros);
        }
    }
}
