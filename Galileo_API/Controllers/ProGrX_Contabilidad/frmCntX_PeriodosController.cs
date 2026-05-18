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
    public class FrmCntXPeriodosController : ControllerBase
    {
        private readonly FrmCntXPeriodosBl _bl;

        public FrmCntXPeriodosController(IConfiguration config)
            => _bl = new FrmCntXPeriodosBl(config);

        [HttpGet("CntxPeriodos_Listar")]
        public ErrorDto<List<CntxPeriodoListaData>> CntxPeriodos_Listar(
            int codEmpresa,
            int codConta,
            string estado)
        {
            return _bl.CntxPeriodos_Listar(codEmpresa, codConta, estado);
        }
    }
}
