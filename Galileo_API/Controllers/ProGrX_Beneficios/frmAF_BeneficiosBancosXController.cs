using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Beneficios;

namespace Galileo_API.Controllers.ProGrX_Beneficios
{
    /// <summary>
    /// Endpoints de Bancos habilitados para Beneficios (frmAF_BeneficiosBancosX).
    /// </summary>
    [Route("api/frmAF_BeneficiosBancosX")]
    [ApiController]
    public class FrmAfBeneficiosBancosXController : ControllerBase
    {
        private readonly FrmAfBeneficiosBancosXBL _bl;

        public FrmAfBeneficiosBancosXController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new FrmAfBeneficiosBancosXBL(config);
        }

        /// <summary>Lista de bancos habilitados para beneficios.</summary>
        [Authorize]
        [HttpGet("BeneficiosBancosX_Obtener")]
        public ErrorDto<AfBeneficiosBancosDataLista> BeneficiosBancosX_Obtener(int CodCliente, string filtros)
            => _bl.BeneficiosBancosX_Obtener(CodCliente, filtros);

        /// <summary>Actualiza la configuración de un banco (cheque/transferencia).</summary>
        [Authorize]
        [HttpPut("BeneficiosBancosX_Actualizar")]
        public ErrorDto<AfBeneficiosBancosData> BeneficiosBancosX_Actualizar(int CodCliente, [FromBody] AfBeneficiosBancosData data)
            => _bl.BeneficiosBancosX_Actualizar(CodCliente, data);
    }
}
