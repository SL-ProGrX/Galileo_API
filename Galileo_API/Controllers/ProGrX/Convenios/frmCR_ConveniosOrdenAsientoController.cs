using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Convenios;
using Galileo_API.Models.ProGrX.Convenios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Convenios
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrConveniosOrdenAsientoController : ControllerBase
    {
        private readonly FrmCrConveniosOrdenAsientoBl _bl;

        public FrmCrConveniosOrdenAsientoController(IConfiguration config)
        {
            _bl = new FrmCrConveniosOrdenAsientoBl(config);
        }

        [HttpGet("CrConveniosOrdenAsiento_Obtener")]
        public ErrorDto<CrOrdenAsientoConsulta> CrConveniosOrdenAsiento_Obtener(
            [FromQuery] int codEmpresa,
            [FromQuery] string codConvenio,
            [FromQuery] int codOrden)
            => _bl.CrConveniosOrdenAsiento_Obtener(codEmpresa, codConvenio, codOrden);
    }
}
