using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;

namespace Galileo.Controllers
{
    [Route("api/frmCC_ActualizaDatos")]
    [ApiController]
    public class FrmCcActualizaDatosController : ControllerBase
    {
        readonly FrmCcActualizaDatosBl BL_CC_ActualizaDatos;
        public FrmCcActualizaDatosController(IConfiguration config)
        {
            BL_CC_ActualizaDatos = new FrmCcActualizaDatosBl(config);
        }

        [HttpGet("CC_ActualizaDatos_SP")]
        public ErrorDto CC_ActualizaDatos_SP(int CodEmpresa)
        {
            return BL_CC_ActualizaDatos.CC_ActualizaDatos_SP(CodEmpresa);
        }
    }
}