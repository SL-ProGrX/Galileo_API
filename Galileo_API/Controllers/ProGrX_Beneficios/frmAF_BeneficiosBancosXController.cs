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
    [Authorize]
    public class FrmAfBeneficiosBancosXController : ControllerBase
    {
        private readonly FrmAfBeneficiosBancosXbl _bl;

        public FrmAfBeneficiosBancosXController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new FrmAfBeneficiosBancosXbl(config);
        }

        /// <summary>Lista de bancos habilitados para beneficios con paginación, filtro y ordenamiento.</summary>
        [HttpGet("BeneficiosBancosX_Obtener")]
        public ErrorDto<AfBeneficiosBancosDataLista> BeneficiosBancosX_Obtener(int CodCliente, string? filtros)
            => _bl.BeneficiosBancosX_Obtener(CodCliente, filtros);

        /// <summary>Exporta la lista de bancos aplicando el filtro vigente, sin paginar.</summary>
        [HttpGet("BeneficiosBancosX_Exportar")]
        public ErrorDto<List<AfBeneficiosBancosData>> BeneficiosBancosX_Exportar(int CodCliente, string? filtros)
            => _bl.BeneficiosBancosX_Exportar(CodCliente, filtros);

        /// <summary>Actualiza la configuración de un banco (cheque/transferencia).</summary>
        [HttpPut("BeneficiosBancosX_Actualizar")]
        public ErrorDto<AfBeneficiosBancosData> BeneficiosBancosX_Actualizar(int CodCliente, [FromBody] AfBeneficiosBancosData data)
            => _bl.BeneficiosBancosX_Actualizar(CodCliente, data);
    }
}
