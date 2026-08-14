using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_ControlTramites;
using Galileo_API.Models.ProGrX_ControlTramites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_ControlTramites
{
    [ApiController]
    [Route("api/FrmAFRecepcionDevolucionesLiq")]
    [Authorize]
    public sealed class FrmAfRecepcionDevolucionesLiqController : ControllerBase
    {
        private readonly FrmAfRecepcionDevolucionesLiqBl _bl;

        public FrmAfRecepcionDevolucionesLiqController(IConfiguration config)
        {
            _bl = new FrmAfRecepcionDevolucionesLiqBl(config);
        }

        [HttpGet("AF_frmAF_RecepcionDevolucionesLiq_Inicializar")]
        public ErrorDto<AfRecepcionDevolucionesLiqInicializarData>
            AF_frmAF_RecepcionDevolucionesLiq_Inicializar(int codEmpresa)
        {
            return _bl.AF_frmAF_RecepcionDevolucionesLiq_Inicializar(
                codEmpresa);
        }

        [HttpGet("AF_frmAF_RecepcionDevolucionesLiq_Boleta_Obtener")]
        public ErrorDto<AfRecepcionDevolucionesLiqData?>
            AF_frmAF_RecepcionDevolucionesLiq_Boleta_Obtener(
                int codEmpresa,
                int numeroBoleta)
        {
            return _bl.AF_frmAF_RecepcionDevolucionesLiq_Boleta_Obtener(
                codEmpresa,
                numeroBoleta);
        }

        [HttpPost("AF_frmAF_RecepcionDevolucionesLiq_Aplicar")]
        public ErrorDto<AfRecepcionDevolucionesLiqAplicarData>
            AF_frmAF_RecepcionDevolucionesLiq_Aplicar(
                int codEmpresa,
                AfRecepcionDevolucionesLiqAplicarRequest? request)
        {
            if (request is not null)
            {
                request.usuario = User.Identity?.Name?.Trim() ?? string.Empty;
            }

            return _bl.AF_frmAF_RecepcionDevolucionesLiq_Aplicar(
                codEmpresa,
                request);
        }
    }
}