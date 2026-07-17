using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Beneficios;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.Controllers.ProGrX_Beneficios
{
    /// <summary>
    /// Endpoints de Consultas de Beneficios Integrales (FrmAfBeneficiosIntegralCon). Solo lectura.
    /// </summary>
    [Route("api/frmAF_Beneficios_Integral_Con")]
    [ApiController]
    public class FrmAfBeneficiosIntegralConController : ControllerBase
    {
        private readonly FrmAfBeneficiosIntegralConBL _bl;

        public FrmAfBeneficiosIntegralConController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new FrmAfBeneficiosIntegralConBL(config);
        }

        /// <summary>
        /// Lista filtrada de beneficios de la consulta general.
        /// </summary>
        [Authorize]
        [HttpGet("BeneConsultasLista_Obtener")]
        public ErrorDto<BeneConsultaDatosLista> BeneConsultasLista_Obtener(string filtro)
        {
            return _bl.BeneConsultasLista_Obtener(filtro);
        }

        /// <summary>
        /// Estados configurados para el beneficio según la categoría.
        /// </summary>
        [Authorize]
        [HttpGet("BeneConsultaEstados_Obtener")]
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> BeneConsultaEstados_Obtener(int CodCliente, string categoria)
        {
            return _bl.BeneConsultaEstados_Obtener(CodCliente, categoria);
        }

        /// <summary>
        /// Información del beneficio seleccionado en la consulta general.
        /// </summary>
        [Authorize]
        [HttpGet("BeneficioIntegral_Obtener")]
        public ErrorDto<BeneficioDto> BeneficioIntegral_Obtener(int CodCliente, long beneficio)
        {
            return _bl.BeneficioIntegral_Obtener(CodCliente, beneficio);
        }
    }
}
