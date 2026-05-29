using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.Controllers.ProGrX.Clientes
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAfPromotoresReportesController : ControllerBase
    {
        private readonly FrmAfPromotoresReportesBL _bl;

        public FrmAfPromotoresReportesController(IConfiguration config)
        {
            _bl = new FrmAfPromotoresReportesBL(config);
        }

        [Authorize]
        [HttpGet("AF_PromotoresReportes_Obtener")]
        public ErrorDto<TablasListaGenericaModel> AF_PromotoresReportes_Obtener(int CodEmpresa, string filtro)
        {
            return _bl.AF_PromotoresReportes_Obtener(CodEmpresa, filtro);
        }

    }
}