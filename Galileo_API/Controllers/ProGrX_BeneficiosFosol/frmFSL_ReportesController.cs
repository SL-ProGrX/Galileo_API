using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol;

namespace Galileo_API.Controllers.ProGrX_BeneficiosFosol
{
    /// <summary>
    /// Endpoints de los Reportes de Beneficios Fosol (frmFSL_Reportes).
    /// </summary>
    [Route("api/frmFSL_Reportes")]
    [ApiController]
    public class FrmFslReportesController : ControllerBase
    {
        private readonly FrmFslReportesBL _bl;

        public FrmFslReportesController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new FrmFslReportesBL(config);
        }

        /// <summary>Catálogo de oficinas.</summary>
        [Authorize]
        [HttpGet("FSL_Oficinas_Obtener")]
        public ErrorDto<List<Oficina>> FSL_Oficinas_Obtener(int CodEmpresa)
            => _bl.FSL_Oficinas_Obtener(CodEmpresa);

        /// <summary>Catálogo de planes Fosol activos.</summary>
        [Authorize]
        [HttpGet("FSL_Planes_Obtener")]
        public ErrorDto<List<Plan>> FSL_Planes_Obtener(int CodEmpresa)
            => _bl.FSL_Planes_Obtener(CodEmpresa);
    }
}
