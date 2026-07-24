using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Polizas;
using Galileo_API.Models.ProGrX_Polizas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Polizas
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public sealed class FrmCcPolizaBeneficiariosController : ControllerBase
    {
        private readonly FrmCcPolizaBeneficiariosBL _bl;

        public FrmCcPolizaBeneficiariosController(IConfiguration config)
        {
            _bl = new FrmCcPolizaBeneficiariosBL(config);
        }

        [HttpGet("CC_Poliza_Beneficiarios_Catalogos_Obtener")]
        public ErrorDto<CcPolizaBeneficiariosCatalogosDto> CC_Poliza_Beneficiarios_Catalogos_Obtener(
            int codEmpresa) =>
            _bl.CC_Poliza_Beneficiarios_Catalogos_Obtener(codEmpresa);

        [HttpGet("CC_Poliza_Beneficiarios_Obtener")]
        public ErrorDto<List<CcPolizaBeneficiarioDto>> CC_Poliza_Beneficiarios_Obtener(
            int codEmpresa,
            string cedula,
            string codPoliza) =>
            _bl.CC_Poliza_Beneficiarios_Obtener(codEmpresa, cedula, codPoliza);

        [HttpGet("CC_Poliza_Beneficiarios_Padron_Obtener")]
        public ErrorDto<CcPolizaBeneficiariosPadronDto?> CC_Poliza_Beneficiarios_Padron_Obtener(
            string identificacion) =>
            _bl.CC_Poliza_Beneficiarios_Padron_Obtener(identificacion);

        [HttpPost("CC_Poliza_Beneficiarios_Guardar")]
        public ErrorDto CC_Poliza_Beneficiarios_Guardar(
            int codEmpresa,
            string usuario,
            [FromBody] CcPolizaBeneficiariosGuardarRequest request) =>
            _bl.CC_Poliza_Beneficiarios_Guardar(codEmpresa, usuario, request);
    }
}
