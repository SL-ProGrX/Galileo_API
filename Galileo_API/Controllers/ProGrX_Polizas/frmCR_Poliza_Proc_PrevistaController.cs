using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Polizas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Polizas
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrPolizaProcPrevistaController : ControllerBase
    {
        private readonly FrmCrPolizaProcPrevistaBL _bl;

        public FrmCrPolizaProcPrevistaController(IConfiguration config)
        {
            _bl = new FrmCrPolizaProcPrevistaBL(config);
        }

        [HttpGet("Cr_PolProcPrevista_PolizaFacturables_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>> Cr_PolProcPrevista_PolizaFacturables_Lista(int CodEmpresa)
        {
            return _bl.Cr_PolProcPrevista_PolizaFacturables_Lista(CodEmpresa);
        }
    }
}
