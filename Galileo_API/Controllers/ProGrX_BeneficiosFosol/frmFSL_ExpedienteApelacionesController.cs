using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol;

namespace Galileo_API.Controllers.ProGrX_BeneficiosFosol
{
    /// <summary>
    /// Endpoints de las Apelaciones de Expediente Fosol (frmFSL_ExpedienteApelaciones).
    /// </summary>
    [Route("api/frmFSL_ExpedienteApelaciones")]
    [ApiController]
    public class FrmFslExpedienteApelacionesController : ControllerBase
    {
        private readonly FrmFslExpedienteApelacionesBL _bl;

        public FrmFslExpedienteApelacionesController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new FrmFslExpedienteApelacionesBL(config);
        }

        /// <summary>Tipos de apelación activos.</summary>
        [Authorize]
        [HttpGet("FslTipoApelacion_Obtener")]
        public ErrorDto<List<FslTipoApelacion>> FslTipoApelacion_Obtener(int CodCliente)
            => _bl.FslTipoApelacion_Obtener(CodCliente);

        /// <summary>Registra una apelación al expediente.</summary>
        [Authorize]
        [HttpPost("FslApelacion_Aplicar")]
        public ErrorDto FslApelacion_Aplicar(int CodCliente, [FromBody] FslApleacionAplicar expediente)
            => _bl.FslApelacion_Aplicar(CodCliente, expediente);

        /// <summary>Aplica la resolución de una apelación.</summary>
        [Authorize]
        [HttpPost("FslResolucionApelacion_Aplicar")]
        public ErrorDto FslResolucionApelacion_Aplicar(int CodCliente, [FromBody] string apelacion)
            => _bl.FslResolucionApelacion_Aplicar(CodCliente, apelacion);
    }
}
