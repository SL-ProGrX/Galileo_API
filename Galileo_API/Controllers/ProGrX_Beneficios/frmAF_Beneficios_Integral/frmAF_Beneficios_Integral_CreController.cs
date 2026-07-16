using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Beneficios;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.Controllers.ProGrX_Beneficios
{
    /// <summary>
    /// Endpoints del proceso Crece de Beneficios Integrales (FrmAfBeneficiosIntegralCre).
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAfBeneficiosIntegralCreController : ControllerBase
    {
        private readonly FrmAfBeneficiosIntegralCreBL _bl;

        public FrmAfBeneficiosIntegralCreController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new FrmAfBeneficiosIntegralCreBL(config);
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
