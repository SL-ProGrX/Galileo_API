using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.BusinessLogic.ProGrX.Bancos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PgxAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmTesMonitoreoController : ControllerBase
    {
        private readonly FrmTesMonitoreoBL MonitoreoBL;

        public FrmTesMonitoreoController(IConfiguration config)
        {
            MonitoreoBL = new FrmTesMonitoreoBL(config);
        }

        [Authorize]
        [HttpGet("TES_Monitoreo_Obtener")]
        public ErrorDto<List<TesMonitoreoDto>> TES_Monitoreo_Obtener(int CodEmpresa, DateTime fechaCorte)
        {
            return MonitoreoBL.TES_Monitoreo_Obtener(CodEmpresa, fechaCorte);
        }

        [Authorize]
        [HttpGet("TES_Monitoreo_Documentos_Obtener")]
        public ErrorDto<List<TesMonitoreoDto>> TES_Monitoreo_Documentos_Obtener(int CodEmpresa, string Corte)
        {
            return MonitoreoBL.TES_Monitoreo_Documentos_Obtener(CodEmpresa, Corte);
        }
    }
}