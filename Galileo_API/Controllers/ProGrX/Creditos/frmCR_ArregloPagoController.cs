using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrArregloPagoController : ControllerBase
    {
        private readonly FrmCrArregloPagoBl _bl;

        public FrmCrArregloPagoController(IConfiguration config)
        {
            _bl = new FrmCrArregloPagoBl(config);
        }

        [HttpGet("Cr_ArregloPago_CajaInicial_Obtener")]
        public ErrorDto<CrArregloPagoCajaInicialData> Cr_ArregloPago_CajaInicial_Obtener(
            int codEmpresa,
            string caja,
            string usuario)
            => _bl.Cr_ArregloPago_CajaInicial_Obtener(codEmpresa, caja, usuario);

        [HttpGet("Cr_ArregloPago_Operacion_Obtener")]
        public ErrorDto<CrArregloPagoOperacionData?> Cr_ArregloPago_Operacion_Obtener(
            int codEmpresa,
            int operacion,
            string usuario)
            => _bl.Cr_ArregloPago_Operacion_Obtener(codEmpresa, operacion, usuario);

        [HttpPost("Cr_ArregloPago_Capitaliza_Aplicar")]
        public ErrorDto<CrArregloPagoAplicacionResultadoData> Cr_ArregloPago_Capitaliza_Aplicar(
            int codEmpresa,
            [FromBody] CrArregloPagoCapitalizaRequest request)
            => _bl.Cr_ArregloPago_Capitaliza_Aplicar(codEmpresa, request);

        [HttpPost("Cr_ArregloPago_PeriodoGracia_Aplicar")]
        public ErrorDto Cr_ArregloPago_PeriodoGracia_Aplicar(
            int codEmpresa,
            [FromBody] CrArregloPagoPeriodoGraciaRequest request)
            => _bl.Cr_ArregloPago_PeriodoGracia_Aplicar(codEmpresa, request);

        [HttpPost("Cr_ArregloPago_VencimientoIntereses_Aplicar")]
        public ErrorDto<CrArregloPagoAplicacionResultadoData> Cr_ArregloPago_VencimientoIntereses_Aplicar(
            int codEmpresa,
            [FromBody] CrArregloPagoVencimientoInteresesRequest request)
            => _bl.Cr_ArregloPago_VencimientoIntereses_Aplicar(codEmpresa, request);

        [HttpPost("Cr_ArregloPago_AbonoEspecial_Aplicar")]
        public ErrorDto<CrArregloPagoAplicacionResultadoData> Cr_ArregloPago_AbonoEspecial_Aplicar(
            int codEmpresa,
            [FromBody] CrArregloPagoAbonoEspecialRequest request)
            => _bl.Cr_ArregloPago_AbonoEspecial_Aplicar(codEmpresa, request);
    }
}