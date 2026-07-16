using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Beneficios;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.Controllers.ProGrX_Beneficios
{
    /// <summary>
    /// Endpoints del proceso Crece de Beneficios Integrales (frmAF_Beneficios_Integral_Cre).
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class frmAF_Beneficios_Integral_CreController : ControllerBase
    {
        private readonly frmAF_Beneficios_Integral_CreBL _bl;

        public frmAF_Beneficios_Integral_CreController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new frmAF_Beneficios_Integral_CreBL(config);
        }

        /// <summary>Registro Crece del beneficio.</summary>
        [Authorize]
        [HttpGet("BeneSocioCrece_Obtener")]
        public ErrorDto<AfiBeneSocioCreceDto> BeneSocioCrece_Obtener(int CodCliente, int consec, string cod_beneficio)
            => _bl.BeneSocioCrece_Obtener(CodCliente, consec, cod_beneficio);

        /// <summary>Guarda (inserta o actualiza) el registro Crece.</summary>
        [Authorize]
        [HttpPost("BeneSocioCrece_Guardar")]
        public ErrorDto BeneSocioCrece_Guardar(int CodCliente, [FromBody] AfiBeneSocioCreceDto beneficio)
            => _bl.BeneSocioCrece_Guardar(CodCliente, beneficio);

        /// <summary>Sesiones del beneficio Crece.</summary>
        [Authorize]
        [HttpGet("BeneSocioCreceSesiones_Obtener")]
        public ErrorDto<List<AfiBeneSocioCreceSesionesDto>> BeneSocioCreceSesiones_Obtener(int CodCliente, int consec, string cod_beneficio)
            => _bl.BeneSocioCreceSesiones_Obtener(CodCliente, consec, cod_beneficio);

        /// <summary>Guarda (inserta o actualiza) una sesión Crece.</summary>
        [Authorize]
        [HttpPost("BeneSocioCreceSesion_Guardar")]
        public ErrorDto BeneSocioCreceSesion_Guardar(int CodCliente, [FromBody] AfiBeneSocioCreceSesionesDto beneficio)
            => _bl.BeneSocioCreceSesion_Guardar(CodCliente, beneficio);
    }
}
