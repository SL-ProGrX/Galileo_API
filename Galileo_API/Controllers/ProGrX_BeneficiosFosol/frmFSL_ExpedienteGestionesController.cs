using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol;

namespace Galileo_API.Controllers.ProGrX_BeneficiosFosol
{
    /// <summary>
    /// Endpoints de las Gestiones de Expediente Fosol (frmFSL_ExpedienteGestiones).
    /// </summary>
    [Route("api/frmFSL_ExpedienteGestiones")]
    [ApiController]
    public class FrmFslExpedienteGestionesController : ControllerBase
    {
        private readonly FrmFslExpedienteGestionesBL _bl;

        public FrmFslExpedienteGestionesController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new FrmFslExpedienteGestionesBL(config);
        }

        /// <summary>Tipos de gestión activos.</summary>
        [Authorize]
        [HttpGet("FslGestiones_Obtener")]
        public ErrorDto<List<FslGestionesListaDatos>> FslGestiones_Obtener(int CodCliente)
            => _bl.FslGestiones_Obtener(CodCliente);

        /// <summary>Registra una gestión de expediente.</summary>
        [Authorize]
        [HttpPost("FslGestion_Agregar")]
        public ErrorDto FslGestion_Agregar(int CodCliente, [FromBody] FslGestionAgregar gestion)
            => _bl.FslGestion_Agregar(CodCliente, gestion);
    }
}
