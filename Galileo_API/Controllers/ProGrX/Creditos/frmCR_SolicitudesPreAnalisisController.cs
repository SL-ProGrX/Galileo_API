using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FrmCrSolicitudesPreAnalisisController : ControllerBase
    {
        private readonly FrmCrSolicitudesPreAnalisisBl _bl;

        public FrmCrSolicitudesPreAnalisisController(IConfiguration config)
        {
            _bl = new FrmCrSolicitudesPreAnalisisBl(config);
        }

        [HttpGet("CrSolicitudesPreAnalisis_Pantalla_Obtener")]
        public ErrorDto<CrSolicitudesPreAnalisisPantallaData> CrSolicitudesPreAnalisis_Pantalla_Obtener(
            int codEmpresa)
            => _bl.CrSolicitudesPreAnalisis_Pantalla_Obtener(codEmpresa);

        [HttpGet("CrSolicitudesPreAnalisis_Consulta_Obtener")]
        public ErrorDto<CrSolicitudesPreAnalisisConsultaData> CrSolicitudesPreAnalisis_Consulta_Obtener(
            int codEmpresa,
            string request)
            => _bl.CrSolicitudesPreAnalisis_Consulta_Obtener(codEmpresa, request);
    }
}