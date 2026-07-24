using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Beneficios;

namespace Galileo_API.Controllers.ProGrX_Beneficios
{
    /// <summary>
    /// Endpoints del catálogo de Profesionales Apremiantes (frmAF_Beneficios_APT_Profesionales).
    /// </summary>
    [Route("api/frmAF_Beneficios_APT_Profesionales")]
    [ApiController]
    public class FrmAfBeneficiosAptProfesionalesController : ControllerBase
    {
        private readonly FrmAfBeneficiosAptProfesionalesBL _bl;

        public FrmAfBeneficiosAptProfesionalesController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new FrmAfBeneficiosAptProfesionalesBL(config);
        }

        /// <summary>Lista de profesionales apremiantes.</summary>
        [Authorize]
        [HttpGet("AfBeneAptPro_Obtener")]
        public ErrorDto<BeneAptProfesionalesDataLista> AfBeneAptPro_Obtener(int CodCliente, string filtros)
            => _bl.AfBeneAptPro_Obtener(CodCliente, filtros);

        /// <summary>Inserta un profesional (o actualiza si existe).</summary>
        [Authorize]
        [HttpPost("AfBeneAptPro_Insertar")]
        public ErrorDto AfBeneAptPro_Insertar(int CodCliente, [FromBody] BeneAptProfesionalesData profesional)
            => _bl.AfBeneAptPro_Insertar(CodCliente, profesional);

        /// <summary>Actualiza un profesional.</summary>
        [Authorize]
        [HttpPut("AfBeneAptPro_Actualizar")]
        public ErrorDto AfBeneAptPro_Actualizar(int CodCliente, [FromBody] BeneAptProfesionalesData profesional)
            => _bl.AfBeneAptPro_Actualizar(CodCliente, profesional);

        /// <summary>Elimina un profesional.</summary>
        [Authorize]
        [HttpDelete("AfBeneAptPro_Eliminar")]
        public ErrorDto AfBeneAptPro_Eliminar(int CodCliente, int id_profesional)
            => _bl.AfBeneAptPro_Eliminar(CodCliente, id_profesional);
    }
}
