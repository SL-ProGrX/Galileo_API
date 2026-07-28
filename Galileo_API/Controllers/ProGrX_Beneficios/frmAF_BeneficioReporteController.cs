using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Beneficios;

namespace Galileo_API.Controllers.ProGrX_Beneficios
{
    /// <summary>
    /// Endpoints de Reportes de Beneficios (frmAF_BeneficioReporte).
    /// </summary>
    [Route("api/frmAF_BeneficioReporte")]
    [ApiController]
    public class FrmAfBeneficioReporteController : ControllerBase
    {
        private readonly FrmAfBeneficioReporteBL _bl;

        public FrmAfBeneficioReporteController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new FrmAfBeneficioReporteBL(config);
        }

        /// <summary>Lista de beneficios para reportes.</summary>
        [Authorize]
        [HttpGet("BeneficioLista_Obtener")]
        public ErrorDto<List<AfiBeneficiosData>> BeneficioLista_Obtener(int CodCliente)
            => _bl.BeneficioLista_Obtener(CodCliente);
    }
}
