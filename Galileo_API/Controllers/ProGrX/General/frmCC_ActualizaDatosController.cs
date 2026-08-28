using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers
{
    [Route("api/frmCC_ActualizaDatos")]
    [ApiController]
    [Authorize]
    public class FrmCcActualizaDatosController : ControllerBase
    {
        private readonly FrmCcActualizaDatosBl _bl;
        public FrmCcActualizaDatosController(IConfiguration config)
        {
            _bl = new FrmCcActualizaDatosBl(config);
        }

        [HttpPost("CC_ActualizaDatos_Proceso_Ejecutar")]
        public ErrorDto
            CC_ActualizaDatos_Proceso_Ejecutar(
                int CodEmpresa)
        {
            return _bl
                .CC_ActualizaDatos_Proceso_Ejecutar(
                    CodEmpresa);
        }
    }
}