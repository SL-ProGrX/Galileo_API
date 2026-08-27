using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_ControlTramites;
using Galileo_API.Models.ProGrX_ControlTramites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_ControlTramites
{
    [ApiController]
    [Route("api/FrmAFRecepcionDevolucionesBeneTags")]
    public sealed class FrmAfRecepcionDevolucionesBeneTagsController : ControllerBase
    {
        private readonly FrmAfRecepcionDevolucionesBeneTagsBl _bl;

        public FrmAfRecepcionDevolucionesBeneTagsController(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _bl = new FrmAfRecepcionDevolucionesBeneTagsBl(config);
        }

        [Authorize]
        [HttpGet("AF_frmAF_RecepcionDevolucionesBeneTags_Inicializar")]
        public ErrorDto<AfRecepcionDevolucionesBeneTagsInicializarData>
            AF_frmAF_RecepcionDevolucionesBeneTags_Inicializar(int codEmpresa)
        {
            return _bl.AF_frmAF_RecepcionDevolucionesBeneTags_Inicializar(codEmpresa);
        }

        [Authorize]
        [HttpGet("AF_frmAF_RecepcionDevolucionesBeneTags_Beneficio_Obtener")]
        public ErrorDto<AfRecepcionDevolucionesBeneTagsData?>
            AF_frmAF_RecepcionDevolucionesBeneTags_Beneficio_Obtener(
                int codEmpresa,
                string codBeneficio,
                string codigo)
        {
            return _bl.AF_frmAF_RecepcionDevolucionesBeneTags_Beneficio_Obtener(
                codEmpresa,
                codBeneficio,
                codigo);
        }

        [Authorize]
        [HttpPost("AF_frmAF_RecepcionDevolucionesBeneTags_Aplicar")]
        public ErrorDto<AfRecepcionDevolucionesBeneTagsAplicarData>
            AF_frmAF_RecepcionDevolucionesBeneTags_Aplicar(
                int codEmpresa,
                AfRecepcionDevolucionesBeneTagsAplicarRequest request)
        {
            request.Usuario = User.Identity?.Name?.Trim() ?? string.Empty;

            return _bl.AF_frmAF_RecepcionDevolucionesBeneTags_Aplicar(
                codEmpresa,
                request);
        }
    }
}
