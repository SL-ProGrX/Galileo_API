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
    public class FrmPreaBallonPaymentController : ControllerBase
    {
        private readonly FrmPreaBallonPaymentBL _bl;

        public FrmPreaBallonPaymentController(IConfiguration config)
        {
            _bl = new FrmPreaBallonPaymentBL(config);
        }

        [HttpGet("Prea_frmPrea_BallonPayment_Cargar")]
        public ErrorDto<FrmPreaBallonPaymentCargarResponse> Prea_frmPrea_BallonPayment_Cargar(
            int codEmpresa,
            string usuario,
            string cod_preanalisis)
        {
            return _bl.Prea_frmPrea_BallonPayment_Cargar(codEmpresa, usuario, cod_preanalisis);
        }

        [HttpGet("Prea_frmPrea_BallonPayment_TablaPagos_Obtener")]
        public ErrorDto<FrmPreaBallonPaymentTablaPagosResponse> Prea_frmPrea_BallonPayment_TablaPagos_Obtener(
            int codEmpresa,
            string usuario,
            string cod_preanalisis)
        {
            return _bl.Prea_frmPrea_BallonPayment_TablaPagos_Obtener(codEmpresa, usuario, cod_preanalisis);
        }

        [HttpPost("Prea_frmPrea_BallonPayment_Calcular")]
        public ErrorDto<FrmPreaBallonPaymentCalcularResponse> Prea_frmPrea_BallonPayment_Calcular(
            int codEmpresa,
            [FromBody] FrmPreaBallonPaymentCalcularRequest request)
        {
            return _bl.Prea_frmPrea_BallonPayment_Calcular(codEmpresa, request);
        }

        [HttpPost("Prea_frmPrea_BallonPayment_Condiciones_Guardar")]
        public ErrorDto<FrmPreaBallonPaymentCondicionesGuardarResponse> Prea_frmPrea_BallonPayment_Condiciones_Guardar(
            int codEmpresa,
            [FromBody] FrmPreaBallonPaymentCondicionesGuardarRequest request)
        {
            return _bl.Prea_frmPrea_BallonPayment_Condiciones_Guardar(codEmpresa, request);
        }

        [HttpPost("Prea_frmPrea_BallonPayment_Guardar")]
        public ErrorDto<FrmPreaBallonPaymentGuardarResponse> Prea_frmPrea_BallonPayment_Guardar(
            int codEmpresa,
            [FromBody] FrmPreaBallonPaymentGuardarRequest request)
        {
            return _bl.Prea_frmPrea_BallonPayment_Guardar(codEmpresa, request);
        }
    }
}
