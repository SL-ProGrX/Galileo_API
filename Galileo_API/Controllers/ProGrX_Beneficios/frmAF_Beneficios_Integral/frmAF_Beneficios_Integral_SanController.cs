using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Beneficios;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.Controllers.ProGrX_Beneficios
{
    /// <summary>
    /// Endpoints de Sanciones de Beneficios Integrales (frmAF_Beneficios_Integral_San).
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class frmAF_Beneficios_Integral_SanController : ControllerBase
    {
        private readonly frmAF_Beneficios_Integral_SanBL _bl;

        public frmAF_Beneficios_Integral_SanController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new frmAF_Beneficios_Integral_SanBL(config);
        }

        /// <summary>Lista de tipos de sanción activos.</summary>
        [Authorize]
        [HttpGet("BeneSancionMotivoLista_Obtener")]
        public List<BeneficiosSancionesLista> BeneSancionMotivoLista_Obtener(int CodCliente)
            => _bl.BeneSancionMotivoLista_Obtener(CodCliente);

        /// <summary>Sanciones registradas del socio.</summary>
        [Authorize]
        [HttpGet("BeneSacionesSocio_Obtener")]
        public ErrorDto<List<AfiBeneSancionesDto>> BeneSacionesSocio_Obtener(int CodCliente, string cedula)
            => _bl.BeneSacionesSocio_Obtener(CodCliente, cedula);

        /// <summary>Guarda (inserta o actualiza) la sanción del socio.</summary>
        [Authorize]
        [HttpPost("BeneSancionesSocio_Guardar")]
        public ErrorDto BeneSancionesSocio_Guardar(int CodCliente, [FromBody] AfiBeneSancionesDto sancion)
            => _bl.BeneSancionesSocio_Guardar(CodCliente, sancion);
    }
}
