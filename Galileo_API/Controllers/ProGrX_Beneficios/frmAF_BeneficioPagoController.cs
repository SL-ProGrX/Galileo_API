using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Beneficios;

namespace Galileo_API.Controllers.ProGrX_Beneficios
{
    /// <summary>
    /// Endpoints del Pago de Beneficios (frmAF_BeneficioPago).
    /// </summary>
    [Route("api/frmAF_BeneficioPago")]
    [ApiController]
    public class FrmAfBeneficioPagoController : ControllerBase
    {
        private readonly FrmAfBeneficioPagoBL _bl;

        public FrmAfBeneficioPagoController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new FrmAfBeneficioPagoBL(config);
        }

        /// <summary>Lista de beneficios habilitados para pago.</summary>
        [Authorize]
        [HttpGet("AfiBeneficioPagoLista_Obtener")]
        public ErrorDto<List<AfiBenePagoData>> AfiBeneficioPagoLista_Obtener(int CodCliente)
            => _bl.AfiBeneficioPagoLista_Obtener(CodCliente);

        /// <summary>Tabla de pagos pendientes de un beneficio.</summary>
        [Authorize]
        [HttpGet("AfiBeneficioPagosTabla_Obtener")]
        public ErrorDto<List<AfiBenePago>> AfiBeneficioPagosTabla_Obtener(int CodCliente, string cod_beneficio)
            => _bl.AfiBeneficioPagosTabla_Obtener(CodCliente, cod_beneficio);

        /// <summary>Nombre del beneficiario según cédula bancaria.</summary>
        [Authorize]
        [HttpGet("Beneficiarios_Obtener")]
        public ErrorDto Beneficiarios_Obtener(int CodCliente, int consec, string cedulabn, string cod_beneficio)
            => _bl.Beneficiarios_Obtener(CodCliente, consec, cedulabn, cod_beneficio);

        /// <summary>Genera el pago de los beneficios.</summary>
        [Authorize]
        [HttpPost("AfiBeneficioPago_Generar")]
        public ErrorDto AfiBeneficioPago_Generar(int CodCliente, string usuario, [FromBody] List<AfiBenePago> tabla)
            => _bl.AfiBeneficioPago_Generar(CodCliente, usuario, tabla);
    }
}
