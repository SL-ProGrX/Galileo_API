using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrx_Personas;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;

namespace Galileo.Controllers.ProGrx_Personas
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAfSuspendidosConsultaController : ControllerBase
    {
        private readonly FrmAfSuspendidosConsultaBl BLAF_Suspendidos_Consulta;
        
        public FrmAfSuspendidosConsultaController(IConfiguration config)
        {
            BLAF_Suspendidos_Consulta = new FrmAfSuspendidosConsultaBl(config);
        }

        [Authorize]
        [HttpGet("AF_Suspendidos_Consulta_Obtener")]
        public ErrorDto<List<AfSuspendidosConsultaDto>> AF_Suspendidos_Consulta_Obtener(int CodEmpresa, string filtros)
        {
            return BLAF_Suspendidos_Consulta.AF_Suspendidos_Consulta_Obtener(CodEmpresa, filtros);
        }
    }
}
