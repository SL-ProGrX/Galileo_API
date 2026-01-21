using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.BusinessLogic.ProGrX.Bancos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmTesCambiosFechasController : ControllerBase
    {
        private readonly FrmTesCambiosFechasBL _CambiosFechasBL;

        public FrmTesCambiosFechasController(IConfiguration config)
        {
            _CambiosFechasBL = new FrmTesCambiosFechasBL(config);
        }

       
        [HttpGet("TES_CambioFechas_Obtener")]
        public ErrorDto<TesCambioFechasData> TES_CambioFechas_Obtener(int CodEmpresa, int solicitud)
        {
            return _CambiosFechasBL.TES_CambioFechas_Obtener(CodEmpresa, solicitud);
        }

        [HttpPost("TES_CambioFecha_Cambiar")]
        public ErrorDto TES_CambioFecha_Cambiar(int CodEmpresa, TesCambioFechasModel fechas)
        {
            return _CambiosFechasBL.TES_CambioFecha_Cambiar(CodEmpresa, fechas);
        }
    }
}
