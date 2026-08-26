using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_ControlTramites;
using Galileo_API.Models.ProGrX_ControlTramites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_ControlTramites
{
    [ApiController]
    [Route("api/FrmAFRecepcionDevolucionesTags")]
    public sealed class FrmAfRecepcionDevolucionesTagsController : ControllerBase
    {
        private readonly FrmAfRecepcionDevolucionesTagsBl _bl;

        public FrmAfRecepcionDevolucionesTagsController(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _bl = new FrmAfRecepcionDevolucionesTagsBl(config);
        }

        [Authorize]
        [HttpGet("AF_frmAF_RecepcionDevolucionesTags_Inicializar")]
        public ErrorDto<AfRecepcionDevolucionesTagsInicializarData>
            AF_frmAF_RecepcionDevolucionesTags_Inicializar(int codEmpresa)
        {
            return _bl.AF_frmAF_RecepcionDevolucionesTags_Inicializar(codEmpresa);
        }

        [Authorize]
        [HttpGet("AF_frmAF_RecepcionDevolucionesTags_Cedula_Obtener")]
        public ErrorDto<AfRecepcionDevolucionesTagsData?>
            AF_frmAF_RecepcionDevolucionesTags_Cedula_Obtener(
                int codEmpresa,
                string cedula)
        {
            return _bl.AF_frmAF_RecepcionDevolucionesTags_Cedula_Obtener(
                codEmpresa,
                cedula);
        }

        [Authorize]
        [HttpPost("AF_frmAF_RecepcionDevolucionesTags_Aplicar")]
        public ErrorDto<AfRecepcionDevolucionesTagsAplicarData>
            AF_frmAF_RecepcionDevolucionesTags_Aplicar(
                int codEmpresa,
                AfRecepcionDevolucionesTagsAplicarRequest request)
        {
            request.Usuario = User.Identity?.Name?.Trim() ?? string.Empty;

            return _bl.AF_frmAF_RecepcionDevolucionesTags_Aplicar(
                codEmpresa,
                request);
        }
    }
}
