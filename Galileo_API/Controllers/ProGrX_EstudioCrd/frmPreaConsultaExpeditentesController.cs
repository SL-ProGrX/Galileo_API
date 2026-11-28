using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_EstudioCrd
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmPreaConsultaExpeditentesController : ControllerBase
    {
        private readonly FrmPreaConsultaExpeditentesBL _bl;

        public FrmPreaConsultaExpeditentesController(IConfiguration configuration)
        {
            _bl = new FrmPreaConsultaExpeditentesBL(configuration);
        }

        /// <summary>
        /// Obtiene la lista de expedientes con paginación server-side y filtro global.
        /// </summary>
        [HttpGet("Prea_frmPreaConsultaExpeditentes_Lista_Obtener")]
        public ErrorDto<FrmPreaConsultaExpeditentesListaResponse> Prea_frmPreaConsultaExpeditentes_Lista_Obtener(
            [FromQuery] FrmPreaConsultaExpeditentesFiltroRequest request)
        {
            return _bl.Prea_frmPreaConsultaExpeditentes_Lista_Obtener(request);
        }

        [HttpGet("Prea_frmPreaConsultaExpeditentes_Estados_Obtener")]
        public ErrorDto<List<FrmPreaConsultaExpeditentesEstado>> Prea_frmPreaConsultaExpeditentes_Estados_Obtener(
            int codEmpresa)
            => _bl.Prea_frmPreaConsultaExpeditentes_Estados_Obtener(codEmpresa);
    }
}
