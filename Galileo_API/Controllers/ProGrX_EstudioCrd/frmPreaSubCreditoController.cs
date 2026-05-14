using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_EstudioCrd
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmPreaSubCreditoController : Controller
    {
        private readonly FrmPreaSubCreditoBL _bl;

        public FrmPreaSubCreditoController(IConfiguration config)
        {
            _bl = new FrmPreaSubCreditoBL(config);
        }

        [HttpGet("Prea_frmPreaSubCredito_Cargar")]
        public ErrorDto<FrmPreaSubCreditoCargarResponse> Prea_frmPreaSubCredito_Cargar(
            int codEmpresa,
            string usuario,
            string cod_preanalisis)
        {
            return _bl.Prea_frmPreaSubCredito_Cargar(codEmpresa, usuario, cod_preanalisis);
        }

        [HttpPost("Prea_frmPreaSubCredito_Aplicar")]
        public ErrorDto<FrmPreaSubCreditoAplicarResponse> Prea_frmPreaSubCredito_Aplicar(
            int codEmpresa,
            [FromBody] FrmPreaSubCreditoAplicarRequest request)
        {
            return _bl.Prea_frmPreaSubCredito_Aplicar(codEmpresa, request);
        }

        [HttpPost("Prea_frmPreaSubCredito_Cuentas_Obtener")]
        public ErrorDto<FrmPreaSubCreditoCuentasResponse> Prea_frmPreaSubCredito_Cuentas_Obtener(
            int codEmpresa,
            [FromBody] FrmPreaSubCreditoCuentasRequest request)
        {
            return _bl.Prea_frmPreaSubCredito_Cuentas_Obtener(codEmpresa, request);
        }
    }
}
