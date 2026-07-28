using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Beneficios;

namespace Galileo_API.Controllers.ProGrX_Beneficios
{
    /// <summary>
    /// Endpoints del Monitor de Beneficios (frmAF_Beneficios_Monitor).
    /// </summary>
    [Route("api/frmAF_Beneficios_Monitor")]
    [ApiController]
    public class FrmAfBeneficiosMonitorController : ControllerBase
    {
        private readonly FrmAfBeneficiosMonitorBL _bl;

        public FrmAfBeneficiosMonitorController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new FrmAfBeneficiosMonitorBL(config);
        }

        /// <summary>Lista de beneficios del monitor con filtros.</summary>
        [Authorize]
        [HttpGet("BeneficiosMonitor_Obtener")]
        public ErrorDto<VBeneficiosIntegralDtoLista> BeneficiosMonitor_Obtener(int CodCliente, string filtroString)
            => _bl.BeneficiosMonitor_Obtener(CodCliente, filtroString);

        /// <summary>Lista de instituciones.</summary>
        [Authorize]
        [HttpGet("InstitucionesLista_Obtener")]
        public ErrorDto<List<OpcionesLista>> InstitucionesLista_Obtener(int CodCliente)
            => _bl.InstitucionesLista_Obtener(CodCliente);

        /// <summary>Lista de estados de persona.</summary>
        [Authorize]
        [HttpGet("EstadosLista_Obtener")]
        public ErrorDto<List<OpcionesLista>> EstadosLista_Obtener(int CodCliente)
            => _bl.EstadosLista_Obtener(CodCliente);

        /// <summary>Lista de oficinas.</summary>
        [Authorize]
        [HttpGet("OficinasLista_Obtener")]
        public ErrorDto<List<OpcionesLista>> OficinasLista_Obtener(int CodCliente)
            => _bl.OficinasLista_Obtener(CodCliente);

        /// <summary>Lista de beneficios activos.</summary>
        [Authorize]
        [HttpGet("BeneficiosLista_Obtener")]
        public ErrorDto<List<OpcionesLista>> BeneficiosLista_Obtener(int CodCliente)
            => _bl.BeneficiosLista_Obtener(CodCliente);
    }
}
