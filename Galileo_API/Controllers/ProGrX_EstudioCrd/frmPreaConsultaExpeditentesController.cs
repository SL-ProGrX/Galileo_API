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

        [HttpGet("Prea_frmPreaConsultaExpeditentes_Lista_Obtener")]
        public ErrorDto<FrmPreaConsultaExpeditentesListaResponse> Prea_frmPreaConsultaExpeditentes_Lista_Obtener(
            int codEmpresa,
            string? buscar = null)
        {
            return _bl.Prea_frmPreaConsultaExpeditentes_Lista_Obtener(codEmpresa, buscar);
        }
    }
}
