using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Procesos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Procesos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCcProcesoMensualPlanillaController : ControllerBase
    {
        private readonly FrmCcProcesoMensualPlanillaBL _bl;

        public FrmCcProcesoMensualPlanillaController(IConfiguration config)
        {
            _bl = new FrmCcProcesoMensualPlanillaBL(config);
        }
        [Authorize]
        [HttpGet("CC_ProcesoMensualPlanilla_Lista_Obtener")]
        public ErrorDto<TablasListaGenericaModel> CC_ProcesoMensualPlanilla_Lista_Obtener(int CodEmpresa, string Parametros)
        {
            return _bl.CC_ProcesoMensualPlanilla_Lista_Obtener(CodEmpresa, Parametros);
        }
        [Authorize]
        [HttpGet("CC_ProcesoMensualPlanilla_Lista_Export")]
        public ErrorDto<TablasListaGenericaModel> CC_ProcesoMensualPlanilla_Lista_Export(int CodEmpresa, string Parametros)
        {
            return _bl.CC_ProcesoMensualPlanilla_Lista_Export(CodEmpresa, Parametros);
        }
    }
}