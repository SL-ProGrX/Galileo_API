using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_ControlTramites;
using Galileo_API.Models.ProGrX_ControlTramites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_ControlTramites
{
    [ApiController]
    [Route("api/FrmFNDRecepcionDevoluciones")]
    public sealed class FrmFndRecepcionDevolucionesController : ControllerBase
    {
        private readonly FrmFndRecepcionDevolucionesBl _bl;

        public FrmFndRecepcionDevolucionesController(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _bl = new FrmFndRecepcionDevolucionesBl(config);
        }

        [Authorize]
        [HttpGet("FND_frmFNDRecepcionDevoluciones_Inicializar")]
        public ErrorDto<FndRecepcionDevolucionesInicializarData>
            FND_frmFNDRecepcionDevoluciones_Inicializar(int codEmpresa)
        {
            return _bl.FND_frmFNDRecepcionDevoluciones_Inicializar(codEmpresa);
        }

        [Authorize]
        [HttpGet("FND_frmFNDRecepcionDevoluciones_Contratos_Obtener")]
        public ErrorDto<List<FndRecepcionDevolucionesContratoBusquedaData>>
            FND_frmFNDRecepcionDevoluciones_Contratos_Obtener(
                int codEmpresa,
                string codPlan,
                string cedula = "")
        {
            return _bl.FND_frmFNDRecepcionDevoluciones_Contratos_Obtener(
                codEmpresa,
                codPlan,
                cedula);
        }

        [Authorize]
        [HttpGet("FND_frmFNDRecepcionDevoluciones_Contrato_Obtener")]
        public ErrorDto<FndRecepcionDevolucionesData?>
            FND_frmFNDRecepcionDevoluciones_Contrato_Obtener(
                int codEmpresa,
                string codPlan,
                long codContrato)
        {
            return _bl.FND_frmFNDRecepcionDevoluciones_Contrato_Obtener(
                codEmpresa,
                codPlan,
                codContrato);
        }

        [Authorize]
        [HttpPost("FND_frmFNDRecepcionDevoluciones_Aplicar")]
        public ErrorDto<FndRecepcionDevolucionesAplicarData>
            FND_frmFNDRecepcionDevoluciones_Aplicar(
                int codEmpresa,
                FndRecepcionDevolucionesAplicarRequest request)
        {
            request.Usuario = User.Identity?.Name?.Trim() ?? string.Empty;

            return _bl.FND_frmFNDRecepcionDevoluciones_Aplicar(
                codEmpresa,
                request);
        }
    }
}
