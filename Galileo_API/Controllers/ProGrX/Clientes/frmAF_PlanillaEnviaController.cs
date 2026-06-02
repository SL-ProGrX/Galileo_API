using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.BusinessLogic.ProGrX.Clientes;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAFPlanillaEnviaController : ControllerBase
    {
        private readonly FrmAFPlanillaEnviaBL BL_AF_PlanillaEnvia;

        public FrmAFPlanillaEnviaController(IConfiguration config)
        {
            BL_AF_PlanillaEnvia = new FrmAFPlanillaEnviaBL(config);
        }

        [Authorize]
        [HttpGet("AF_Instituciones_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Instituciones_Obtener(int CodEmpresa)
        {
            return BL_AF_PlanillaEnvia.AF_Instituciones_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_PeriodosProceso_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_PeriodosProceso_Obtener(int CodEmpresa)
        {
            return BL_AF_PlanillaEnvia.AF_PeriodosProceso_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_Archivo_Obtener")]
        public ErrorDto<List<AfArchivoResultadoDto>> AF_Archivo_Obtener(int CodEmpresa, string codinstitucion, string  fechaproceso)
        {
            return BL_AF_PlanillaEnvia.AF_Archivo_Obtener(CodEmpresa, codinstitucion, fechaproceso);
        }

        [Authorize]
        [HttpGet("AF_ArchivoPlanilla_Obtener")]
        public ErrorDto<AfArchivoPlanillaDto> AF_ArchivoPlanilla_Obtener(int CodEmpresa, string codinstitucion, string fechaproceso)
        {
            return BL_AF_PlanillaEnvia.AF_ArchivoPlanilla_Obtener(CodEmpresa, codinstitucion, fechaproceso);
        }
    }
}
