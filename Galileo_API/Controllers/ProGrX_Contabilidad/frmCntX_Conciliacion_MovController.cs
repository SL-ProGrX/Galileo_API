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
    public class FrmCntXConciliacionMovController : ControllerBase
    {
        private readonly FrmCntXConciliacionMovBl _bl;

        public FrmCntXConciliacionMovController(IConfiguration config) => 
            _bl = new FrmCntXConciliacionMovBl(config);

        [HttpPost("CntXConciliacionMov_Conciliar")]
        public ErrorDto<CntXConciliacionResult> CntXConciliacionMov_Conciliar(int codEmpresa, CntXConciliacionMovRequest request)
        {
            return _bl.CntXConciliacionMov_Conciliar(codEmpresa, request);
        }
    }
}